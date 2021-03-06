﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Fungus;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]

public class Player : MonoBehaviour {

	#region Variables

	Flowchart currentFlowchart = null;
	GameObject currentPersona = null;

	GameObject targetObject;
	Vector3 goal;

	public GameObject touchPointPrefab;
	public GameObject xSpotPrefab;

	NavMeshAgent agent;
	Animator animator;

	Vector2 smoothDeltaPosition = Vector2.zero;
	Vector2 velocity = Vector2.zero;

	bool idleBoredom = false;

	#endregion


	#region getter/setter

	bool IsWalking { get { return agent.velocity.sqrMagnitude > 0.01f; } }

	#endregion


	#region Init

	void Start() {

		// get components   
		animator = GetComponent<Animator>();
		agent = GetComponent<NavMeshAgent>();
		// Don’t update position automatically
		agent.updatePosition = false;

	}

	#endregion


	#region Animation

	void Update() {

		UpdateIdle();
		UpdatePosition();

	}


	void UpdateIdle() {

		if (Random.Range(0, 500) < 1) {
			idleBoredom = !idleBoredom;
			animator.SetBool("bored", idleBoredom);
		}

	}

	void UpdatePosition() {

		Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

		// Map 'worldDeltaPosition' to local space
		float dx = Vector3.Dot(transform.right, worldDeltaPosition);
		float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
		Vector2 deltaPosition = new Vector2(dx, dy);

		// Low-pass filter the deltaMove
		float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
		smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

		// Update velocity if time advances
		if (Time.deltaTime > 1e-5f)
			velocity = smoothDeltaPosition / Time.deltaTime;

//		bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

		// Update animation parameters
		animator.SetFloat("velocity", velocity.magnitude);

//		print(animator.rootPosition);

		// move head      
		//GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;
	}

	void OnAnimatorMove() {
		// Update position to agent position
		transform.position = agent.nextPosition;

		// Update position based on animation movement using navigation surface height
//		Vector3 position = animator.rootPosition;
//		position.y = agent.nextPosition.y;
//		transform.position = position;
	}

	#endregion


	#region Interaction


	void OnMouseDown() {

		OnClick(null);

	}

	public void OnClick(GameObject clickedObject) {
      
		// if we're not talking to anyone
		if (currentFlowchart == null || currentPersona == null) {      
			// are we walking?
			if (IsWalking) {
				StopWalking();            
			}         
			return;
		}

		// if we currently have a menuDialog active
		if (transform.FindChild("Dialogues/Player_MenuDialog").gameObject.activeSelf) {
			return;
		}

		// ok, we do NOT have a menuDialog active

		// if there is no current dialogue
		if (!currentFlowchart.HasExecutingBlocks()) {   
			// if we're still in collision with a Persona
			if (currentPersona != null) {
				// try to force restart that previous dialogue
				TryToStartFlowchart(currentPersona);
			}
			// whatever the case, leave this method
			return;
		}

		List<GameObject> charactersInFlowchart = GetCharactersInFlowchart(currentFlowchart);

		// if the clicked object isn't even in the current dialog
		if (!charactersInFlowchart.Contains(clickedObject) && clickedObject != null) {
			Debug.LogWarning("Character " + GetPath(this.gameObject.transform) + " isn't in flowchart " + currentFlowchart.name);
			return;
		}

		// go through each persona we're potentially talking to
		foreach (GameObject characterObject in charactersInFlowchart) {

			// make sure that object isn't us
//			if (characterObject.name == this.gameObject.name) {
//				continue;
//			}
			// get the path to their SayDialog
			SayDialog personaSayDialog = characterObject.GetComponentInChildren<SayDialog>();
			// if this dialog is actually something
			if (personaSayDialog != null) {
				// check to see if that dialog object is active
				if (personaSayDialog.gameObject.activeSelf) {
					// ok, push dat button!
					personaSayDialog.continueButton.onClick.Invoke();
					// all done
					return;
				}
			}
		}

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

		TryToStartFlowchart(other.gameObject);

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
			}
		}

		// if we're touching the xSpot && we're at the end
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
		// if we were already showing a click exploder
		RemovePreviousClicks();

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


	#region Flowchart

	void TryToStartFlowchart(GameObject other) {

		Flowchart flowchart = GetFlowchart(other.name); 

		// if we found this persona's flowchart
		if (flowchart != null) {
			currentPersona = other;
			currentFlowchart = flowchart;
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


	Flowchart GetFlowchart(GameObject gameObject) {

		return GetFlowchart(gameObject.name);

	}


	Flowchart GetFlowchart(string name) {

		// get the flowchart with this person's name
		GameObject flowcharts = GameObject.Find("Flowcharts");
		// try to find the flowchart using this name
		Transform flowchartTransform = flowcharts.transform.FindChild(name);
		if (flowchartTransform == null) {
			return null;
		}
		// get the game object
		GameObject flowchartObject = flowchartTransform.gameObject;
      

		return flowchartObject.GetComponent<Flowchart>();

	}


	// TODO: Add all characters in all blocks
	List<GameObject> GetCharactersInFlowchart(Flowchart flowchart) {

		List<GameObject> possiblePersonaObjects = new List<GameObject>();

		if (flowchart == null) {
			Debug.LogError("Flowchart == null");
			return possiblePersonaObjects;
		}

		// FIXME: This doesn't work when there is no executing block
		// if we have a currently executing block
		List<Block> blocks = flowchart.GetExecutingBlocks();
		// go through each executing block
		foreach (Block block in blocks) {
			// get the command list
			List<Command> commands = block.commandList;
			// go through the command list
			foreach (Command command in commands) {
				// if this is a say command
				if (command.GetType().ToString() == "Fungus.Say") {
					// force type to say
					Say sayCommand = (Say)command;
					// get the gameobject attached to this character
					GameObject persona = sayCommand.character.gameObject.transform.parent.gameObject;
					// make sure this one isn't already in the list
					if (possiblePersonaObjects.Contains(persona)) {
						continue;
					}
					// ok, add it to the list of possible people we're talking to
					possiblePersonaObjects.Add(persona);
				} // if type
			} // foreach Command
		} // foreach(Block

		// if this list doesn't contain the player
		if (!possiblePersonaObjects.Contains(this.gameObject)) {
//			print("Force-add Player");
			possiblePersonaObjects.Add(this.gameObject);
		}

		return possiblePersonaObjects;

	}


	public bool IsCharacterInFlowchart(GameObject character) {

		// if there's no current persona we're interacting with
		if (currentPersona == null) {
			return false;
		}

		Flowchart flowchart = null;

		// if there isn't even a flowchart, forget it
		if (currentFlowchart != null) {
			flowchart = currentFlowchart;
		} else {
			// try to get a flowchart from the current persona
			flowchart = GetFlowchart(currentPersona.name);
		}
		// still no flowchart? null
		if (flowchart == null) {
			return false;
		}
		// ok, we've got a flowchart, who's in it?
		List<GameObject> characters = GetCharactersInFlowchart(currentFlowchart);

		// is this character in it?
		if (characters.Contains(character)) {
			return true;
		}
		// if we're here, then the answer is no
		return false;

	}



	//	GameObject FindDialogues(string name) {
	//
	//		// get the flowchart with this person's name
	//		GameObject persona = FindPersona(name);
	//
	//		// if we couldn't find this persona
	//		if (persona == null) {
	////			Debug.LogError("could find persona " + name);
	//			return null;
	//		}
	//
	//		// get that persona's SayDialog
	//		GameObject dialogues = persona.transform.FindChild("Dialogues").gameObject;
	//
	//		return dialogues;
	//
	//	}


	//	GameObject FindPersona(string name) {
	//
	//		GameObject personae = GameObject.Find("Personae");
	//		if (personae == null) {
	////			Debug.LogError("personae == null");
	//			return null;
	//		}
	//		GameObject persona = personae.transform.FindChild(name).gameObject;
	//		if (persona == null) {
	////			Debug.LogError("persona == null");
	//			return null;
	//		}
	//
	//		return persona;
	//
	//	}


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

	public static string GetPath(Transform current) {
		if (current.parent == null)
			return "/" + current.name;
		return GetPath(current.parent) + "/" + current.name;
	}

	#endregion

}
