using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class Player : MonoBehaviour {

	#region Variables

	Flowchart currentFlowchart = null;
	GameObject currentPersona = null;

	GameObject targetObject;
	Vector3 goal;

	public GameObject touchPointPrefab;
	public GameObject xSpotPrefab;

	NavMeshAgent agent;

	#endregion


	#region getter/setter

	bool IsWalking { get { return agent.velocity.sqrMagnitude > 0.01f; } }

	#endregion


	#region Init

	void Start() {

		agent = GetComponent<NavMeshAgent>();

	}

	#endregion


	#region Interaction

	public void Click() {

		OnMouseDown();

	}

	void OnMouseDown() {

		// if we're not talking to anyone
		if (currentFlowchart == null || currentPersona == null) {

			// are we walking?
			if (IsWalking) {
				StopWalking();
				return;
			}

			return;
		}

		// get our menuDialog gameObject
		GameObject playerMenuDialog = transform.FindChild("Dialogues/Player_MenuDialog").gameObject;

		// if we currently have a menuDialog active
		if (playerMenuDialog.activeSelf) {
			return;
		}

		// ok, we do NOT have a menuDialog active

		// get the sayDialog gameobject
		GameObject playerSayDialog = this.transform.FindChild("Dialogues/Player_SayDialog").gameObject;

		// are we talking?
		if (playerSayDialog.activeSelf) {
			// push dat button!
			playerSayDialog.GetComponent<SayDialog>().continueButton.onClick.Invoke();
			return; // all done
		}

		string personaDialogPath = "Dialogues/" + currentPersona.name + "_SayDialog";
		// is the Persona talking?
		GameObject otherSayDialog = currentPersona.transform.FindChild(personaDialogPath).gameObject;
		// is it active?
		if (otherSayDialog.activeSelf) {
			// push dat button!
			otherSayDialog.GetComponent<SayDialog>().continueButton.onClick.Invoke();
			return; // all done
		}

		// try to advance the current dialogue
		if (!currentFlowchart.HasExecutingBlocks()) {   
			// if we're still in collision with a Persona
			if (currentPersona != null) {
				// try to force restart that previous dialogue
				StartFlowchart(currentPersona);
			} else {
//				Debug.LogWarning("currentPersona == null");
			}    
			return;
		}

		//		Debug.LogWarning("End of OnMouseDown()");

	}

	#endregion


	#region Trigger


	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag != "Persona") {
			return;
		}
      
		// make sure we're not already talking with someone else
		if (currentPersona != null && currentPersona != other.gameObject) {
			return;
		}

		StartFlowchart(other.gameObject);

	}


	void OnTriggerStay(Collider other) {

		// if we're interacting with another character
		if (other.gameObject.tag == "Persona" && other.gameObject == targetObject) {
			// get our distance to that character
			float distance = CalculateDistanceToObject(other.gameObject);
			// if too close
			if (distance < 2.5f) {
				// stop current movement
				StopWalking();
				// if we were already showing a click exploder
				RemovePreviousClicks();
			}
		}

		// if we're touch the xSpot && we're at the end
		if (other.gameObject.tag == "xSpot" && IsAtDestination()) {
			// get rid of the xSpo
			Destroy(other.gameObject);         
		}

	}


	void OnTriggerExit(Collider other) {

		if (other.gameObject.tag != "Persona") {
			return;
		}
      
		// make sure this is the actual person we were interacting with
		if (other.gameObject != currentPersona) {
//			Debug.LogWarning("OnTriggerExit()\tother.gameObject != currentPersona");
			return;
		}

		HideCurrentFlowchart();

		currentPersona = null;
		currentFlowchart = null;

	}

	#endregion


	#region NavMesh

	public void GoToPosition(Vector3 position) {

		targetObject = null;
		goal = position;
		GetComponent<NavMeshAgent>().destination = goal; 

		// if we were already showing a click exploder
		RemovePreviousClicks();
		// show click
		ShowClick(position);

	}


	public void GoToObject(GameObject other) {

		targetObject = other;

		Vector3 position = other.transform.position;
		position.y = 0.01f;
		goal = position;
		GetComponent<NavMeshAgent>().destination = goal;
      
		// if we were already showing a click exploder
		RemovePreviousClicks();
		// show click
		ShowClick(position);

	}

	void StopWalking() {

		// go to where we already are
		targetObject = null;
		goal = transform.position;
		GetComponent<NavMeshAgent>().destination = goal; 

	}

	bool IsAtDestination() {
      
		if (agent.pathPending) {
			return true;
		}

		if (agent.remainingDistance <= agent.stoppingDistance) {
			if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) {
				// Done
				//Debug.Log("Executes 2 times");
				/*if (goal >= points.Length - 1) { // if it's a last point
                        targetPoint = 0;
                    } else {
                        targetPoint++;
                    }*/
				return true;
			}
		}

		return false;

	}

	#endregion


	#region Tools

	float CalculateDistanceToObject(GameObject other) {
		// get their position
		Vector3 personaPosition = other.transform.position;
		// annul y
		personaPosition.y = 0f;
		// get our position
		Vector3 playerPosition = this.transform.position;
		// annul y
		playerPosition.y = 0f;
		// get the distance
		return Vector3.Magnitude(playerPosition - personaPosition);      
	}

	#endregion


	#region Flowchart

	void StartFlowchart(GameObject other) {
       
		currentPersona = other;

		currentFlowchart = GetFlowchart(other.name);

		// if we found this persona
		if (currentFlowchart != null) {
			Fungus.Flowchart.BroadcastFungusMessage(other.name);
		}

	}


	void HideCurrentFlowchart() {
   
		if (currentFlowchart != null) {
			currentFlowchart.GetComponent<Flowchart>().StopAllBlocks();
			currentFlowchart.GetComponent<Flowchart>().StopAllCoroutines();
		}

		// hide any possible menus of our own
		Transform playerSayTransform = transform.FindChild("Dialogues/Player_SayDialog");

		if (playerSayTransform != null) {
			playerSayTransform.gameObject.SetActive(false);
		}

		Transform playerMenuTransform = transform.FindChild("Dialogues/Player_MenuDialog");

		if (playerMenuTransform != null) {

			GameObject playerMenu = playerMenuTransform.gameObject;

			if (playerMenu != null) {
				if (ChildIsActive(playerMenu, "TimeoutSlider"))
					SetActive(playerMenu, "TimeoutSlider", false);
				if (ChildIsActive(playerMenu, "ButtonGroup/OptionButton0"))
					SetActive(playerMenu, "ButtonGroup/OptionButton0", false);
				if (ChildIsActive(playerMenu, "ButtonGroup/OptionButton1"))
					SetActive(playerMenu, "ButtonGroup/OptionButton1", false);
				if (ChildIsActive(playerMenu, "ButtonGroup/OptionButton2"))
					SetActive(playerMenu, "ButtonGroup/OptionButton2", false);
				if (ChildIsActive(playerMenu, "ButtonGroup/OptionButton3"))
					SetActive(playerMenu, "ButtonGroup/OptionButton3", false);
				if (ChildIsActive(playerMenu, "ButtonGroup/OptionButton4"))
					SetActive(playerMenu, "ButtonGroup/OptionButton4", false);
				if (ChildIsActive(playerMenu, "ButtonGroup/OptionButton5"))
					SetActive(playerMenu, "ButtonGroup/OptionButton5", false);
				if (playerMenu.activeInHierarchy)
					playerMenu.SetActive(false);
			}

		}

	}


	bool ChildIsActive(GameObject parentObject, string path) {
		return parentObject.transform.FindChild(path).gameObject.activeInHierarchy;
	}


	void SetActive(GameObject parentObject, string path, bool newState) {
		parentObject.transform.FindChild(path).gameObject.SetActive(newState);
	}


	Flowchart GetFlowchart(string name) {

		// get the flowchart with this person's name
		GameObject flowcharts = GameObject.Find("Flowcharts");
		GameObject flowchartObject = flowcharts.transform.FindChild(name).gameObject;

		if (flowchartObject == null) {
			return null;
		}

		return flowchartObject.GetComponent<Flowchart>();

	}


	GameObject FindDialogues(string name) {

		// get the flowchart with this person's name
		GameObject persona = FindPersona(name);

		// if we couldn't find this persona
		if (persona == null) {
//			Debug.LogError("could find persona " + name);
			return null;
		}

		// get that persona's SayDialog
		GameObject dialogues = persona.transform.FindChild("Dialogues").gameObject;

		return dialogues;

	}


	GameObject FindPersona(string name) {

		GameObject personae = GameObject.Find("Personae");
		if (personae == null) {
//			Debug.LogError("personae == null");
			return null;
		}
		GameObject persona = personae.transform.FindChild(name).gameObject;
		if (persona == null) {
//			Debug.LogError("persona == null");
			return null;
		}

		return persona;

	}


	#endregion


	#region Clicks




	void RemovePreviousClicks() {

		// kill all other clicks
		GameObject[] otherClicks;
		// first exploders
		otherClicks = GameObject.FindGameObjectsWithTag("TouchPoint");
		foreach (GameObject obj in otherClicks) {
			Destroy(obj);
		}

		RemoveXSpot();

	}


	public void RemoveXSpot() {

		GameObject[] otherClicks;
		// then xSpots
		otherClicks = GameObject.FindGameObjectsWithTag("xSpot");
		foreach (GameObject obj in otherClicks) {
			Destroy(obj);
		}

	}


	void ShowClick(Vector3 position) {

		// show xSpot
		GameObject xSpot = Instantiate(xSpotPrefab, position, Quaternion.Euler(90, 0, 0)) as GameObject;
		xSpot.name = "xSpot";
		xSpot.transform.parent = GameObject.Find("Ground").transform;

		// montrer où on a cliqué
		GameObject touchPoint = Instantiate(touchPointPrefab, position, Quaternion.Euler(90, 0, 0)) as GameObject;
		touchPoint.name = "TouchPoint";
		touchPoint.transform.parent = GameObject.Find("Ground").transform;
		// blow up in a co-routine
		StartCoroutine(Explode(touchPoint));

	}


	IEnumerator Explode(GameObject touchPoint) {

//      // match background color for the sprite color
//      float timeSaturation = Camera.main.GetComponent<Daylight>().TimeSaturation;
//      timeSaturation = Mathf.Min(1.0f, 1.3f - timeSaturation);
//      Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);
//      GetComponent<SpriteRenderer>().color = c;

		float explosionSpeed = 0.025f;

		while (touchPoint != null && touchPoint.transform.localScale.x < 0.5f) {      
			touchPoint.transform.localScale = touchPoint.transform.localScale + new Vector3(explosionSpeed, explosionSpeed, explosionSpeed);
			yield return new WaitForEndOfFrame();
		}

		Destroy(touchPoint);

	}

	#endregion

}
