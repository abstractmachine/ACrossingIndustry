using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Fungus;

public class PlayerDialogue : MonoBehaviour {

	Flowchart currentFlowchart = null;
	GameObject currentPersona = null;


	public void Click() {
		OnMouseDown();
	}

	void OnMouseDown() {

		// if we're not talking to anyone
		if (currentFlowchart == null || currentPersona == null) {
			return;
		}

		// get our menuDialog gameObject
		GameObject playerMenuDialog = transform.FindChild("Dialogues/MenuDialog").gameObject;

		// if we currently have a menuDialog active
		if (playerMenuDialog.activeSelf) {
			return;
		}

		// ok, we do NOT have a menuDialog active

		// get the sayDialog gameobject
		GameObject playerSayDialog = this.transform.FindChild("Dialogues/SayDialog").gameObject;

		// are we talking?
		if (playerSayDialog.activeSelf) {
			// push dat button!
			playerSayDialog.GetComponent<SayDialog>().continueButton.onClick.Invoke();
			return; // all done
		}

		// is the Persona talking?
		GameObject otherSayDialog = currentPersona.transform.FindChild("Dialogues/SayDialog").gameObject;
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
				print("currentPersona == null");         
			}    
			return;         
		}

		print("End of OnMouseDown()");

	}



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


	void OnTriggerExit(Collider other) {

		if (other.gameObject.tag != "Persona") {
			return;
		}

		HideCurrentFlowchart();
      
		// make sure this is the actual person we were interacting with
		if (other.gameObject != currentPersona) {
			Debug.LogWarning("OnTriggerExit()\tother.gameObject != currentPersona");
			return;
		}

		currentPersona = null;
		currentFlowchart = null;

	}


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
		Transform playerSayTransform = transform.FindChild("Dialogues/SayDialog");

		if (playerSayTransform != null) {
			playerSayTransform.gameObject.SetActive(false);
		}

		Transform playerMenuTransform = transform.FindChild("Dialogues/MenuDialog");

		if (playerMenuTransform != null) {

			GameObject playerMenu = playerMenuTransform.gameObject;

			if (playerMenu != null) {
				if (playerMenu.transform.FindChild("ButtonGroup/TimeoutSlider").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/TimeoutSlider").gameObject.SetActive(false);
				if (playerMenu.transform.FindChild("ButtonGroup/OptionButton0").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/OptionButton0").gameObject.SetActive(false);
				if (playerMenu.transform.FindChild("ButtonGroup/OptionButton1").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/OptionButton1").gameObject.SetActive(false);
				if (playerMenu.transform.FindChild("ButtonGroup/OptionButton2").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/OptionButton2").gameObject.SetActive(false);
				if (playerMenu.transform.FindChild("ButtonGroup/OptionButton3").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/OptionButton3").gameObject.SetActive(false);
				if (playerMenu.transform.FindChild("ButtonGroup/OptionButton4").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/OptionButton4").gameObject.SetActive(false);
				if (playerMenu.transform.FindChild("ButtonGroup/OptionButton5").gameObject.activeInHierarchy)
					playerMenu.transform.FindChild("ButtonGroup/OptionButton5").gameObject.SetActive(false);
//				if (playerMenu.transform.FindChild("Triangle").gameObject.activeSelf)
//					playerMenu.transform.FindChild("Triangle").gameObject.SetActive(false);
				if (playerMenu.activeInHierarchy)
					playerMenu.SetActive(false);
			}

		}

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


	GameObject GetDialogues(string name) {

		// get the flowchart with this person's name
		GameObject persona = Persona(name);

		// if we couldn't find this persona
		if (persona == null) {
			Debug.LogError("could find persona " + name);
			return null;
		}

		// get that persona's SayDialog
		GameObject dialogues = persona.transform.FindChild("Dialogues").gameObject;

		return dialogues;

	}


	GameObject Persona(string name) {

		GameObject personae = GameObject.Find("Personae");
		if (personae == null) {
			Debug.LogError("personae == null");
			return null;
		}
		GameObject persona = personae.transform.FindChild(name).gameObject;
		if (persona == null) {
			Debug.LogError("persona == null");
			return null;
		}

		return persona;

	}


}
