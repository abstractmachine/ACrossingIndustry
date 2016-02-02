﻿using UnityEngine;
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
      
		// make sure this is the actual person we were interacting with
		if (other.gameObject != currentPersona) {
//			Debug.LogWarning("OnTriggerExit()\tother.gameObject != currentPersona");
			return;
		}

		HideCurrentFlowchart();

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


	GameObject GetDialogues(string name) {

		// get the flowchart with this person's name
		GameObject persona = Persona(name);

		// if we couldn't find this persona
		if (persona == null) {
//			Debug.LogError("could find persona " + name);
			return null;
		}

		// get that persona's SayDialog
		GameObject dialogues = persona.transform.FindChild("Dialogues").gameObject;

		return dialogues;

	}


	GameObject Persona(string name) {

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

}
