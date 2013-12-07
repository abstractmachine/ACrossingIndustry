using UnityEngine;
using System.Collections;
using System.Collections.Generic; // used for Dictionary

public class Dialog : MonoBehaviour {

	// the prefab we use to generate dialog bubbles
	public GameObject phylacterePrefab;
	GameObject phylactere = null;

	bool areTalkingToSomeone = false;
	bool didInitiateDialog = false;

	// a pointer to the other we're discussing with
	GameObject otherPersona = null;
	Dialog otherDialog = null;
	int otherId = 0;
	// a dictionary of all the <int>GameObject.GetInstanceID, and their <int>stateIndex
    Dictionary<int,int> conversations = new Dictionary<int,int>();

	// our previous rotation
	Quaternion previousOrientation = Quaternion.identity;


	//////////////////////////


	void OnApplicationQuit() {

		killPhylacteres();

	}


	void OnDisable() {

		killPhylacteres();

	}


	void OnDestroy() {

		killPhylacteres();

	}


	void killPhylacteres() {

		// if exists, kill it
		if (phylactere != null) Destroy(phylactere);

	}


	//////////////////////////


	void Update () {

	}


	//////////////////////////


	void startDialog() {

		// tell the submissive other to start the actual talking
		otherDialog.reply(speechIndex());

	}



	int speechIndex() {

		// if we've never started this conversation
		if (!conversations.ContainsKey(otherId)) {
			// generate a dictionary entry for this instance
			// start the index at 0 (beginning of phrasebook)
			conversations.Add(otherId,0);
		}

		return conversations[otherId];

	}


	void setSpeechIndex(int index) {

		conversations[otherId] = index;

	}


	void nextSpeechIndex() {

		// TODO: check to see if this is at the end of the index

		conversations[otherId]++;

	}



	void speak(string phrase) {

		// make sure we don't have any dangling phylacteres
		killPhylacteres();

		phylactere = (GameObject)Instantiate(phylacterePrefab);
		phylactere.name = "Phylactere";
		phylactere.transform.parent = this.transform;
		phylactere.transform.localPosition = new Vector3(0,1.5f,0);
		phylactere.GetComponent<Phylactere>().speak(phrase,-1);

	}


	void reply(int index) {

		setSpeechIndex(index);

		if (didInitiateDialog) replyActive();
		else replyPassive();

	}


	void replyPassive() {

		string phrase = "";

		switch(speechIndex()) {

			case 0:
			phrase = "Oh hey it's you. Hello " + otherPersona.name;
			break;

			case 1:
			phrase = "How are you, " + otherPersona.name + "?";
			break;

			case 2:
			phrase = "I'm doing ok.";
			break;

			default:
			phrase = "...";
			break;

		}

		speak(phrase);

	}


	void replyActive() {

		string phrase = "";

		switch(speechIndex()) {

			case 0:
			phrase = "Hello " + otherPersona.name;
			break;

			case 1:
			phrase = "I'm fine, " + otherPersona.name + ". And you?";
			break;

			case 2:
			phrase = "That's good.";
			break;

			default:
			phrase = "...";
			break;

		}

		speak(phrase);

	}


	public void finishedSpeaking() {

		// if there's no more other, we must have finished speaking. No need to reply
		if (otherPersona == null || otherDialog == null) return;

		// if we're the subordinate one, let the initiator reply
		if (!didInitiateDialog) {
			// tell the other to reply to use
			otherDialog.reply(speechIndex());
		}

		// if we're the dominant one
		if (didInitiateDialog) {

			// increment the index by 1
			nextSpeechIndex();
			// tell the other to reply to use
			otherDialog.reply(speechIndex());
		} 

	}


	//////////////////////////


	// MARK: Start Dialog as Active (activateDialog) or Passive (submitPassivelyToDialog)

	public void activateDialog(GameObject other) {

		// get that other's Dialog
		otherDialog = other.GetComponent<Dialog>();
		// if no social engine, quit discussing with this persona
		if (otherDialog == null) return;
		// tell the other to submit to this dialog request and see if they're ok
		if (!otherDialog.submitPassivelyToDialog(gameObject))  {
			// if there was some error in initalizing dialog
			print("unable to establish dialog with other");
			// forget that other's social engine
			forgetOther();
			return;
		}
		// note that we're in a dialog
		areTalkingToSomeone = true;
		// remember that I started this dialog
		didInitiateDialog = true;
		// remember who that Persona is
		rememberOther(other);
		// the dialog has opened, start the actual discussion
		startDialog();

	}


	public bool submitPassivelyToDialog(GameObject other) {

		// if we're already talking to someone
		if (areTalkingToSomeone) return false;

		// note that we're in a dialog
		areTalkingToSomeone = true;
		// remember that the other Persona started this dialog
		didInitiateDialog = false;
		// remember who that Persona is
		rememberOther(other);
		// remember where we were facing before
		previousOrientation = transform.localRotation;
		// turn to face that person
		transform.LookAt(other.transform, Vector3.up);
		// ok, we are free to start dialog with the other
		return true;

	}


	public void abortDialog() {

		// if we initiated this dialog
		if (didInitiateDialog) {
			// stop talking
			abortActive();
		}

		// if we were the passive responder to a dialog
		if (!didInitiateDialog) {
			// stop being socially submissive! ;-)
			abortPassive();
		}

		// reset "other" flag
		forgetOther();

	}


	void abortActive() {

		//speak("Goodbye " + otherPersona.name);
		// tell the other it's over
		otherDialog.abortDialog();

	}


	void abortPassive() {

		//speak("Goodbye " + otherPersona.name);

		// turn back to whatever we were doing
		transform.localRotation = previousOrientation;
		// clear Previous Orientation variable
		previousOrientation = Quaternion.identity;

	}


	//////////////////////////


	void rememberOther(GameObject other) {

		// remember who that Persona is
		otherPersona = other;
		// get that other's Dialog
		otherDialog = other.GetComponent<Dialog>();
		// get that other's ID
		otherId = other.GetInstanceID();

	}


	void forgetOther() {
		
		areTalkingToSomeone = false;
		didInitiateDialog = false;
		otherPersona = null;
		otherDialog = null;
		otherId = 0;

	}


}
