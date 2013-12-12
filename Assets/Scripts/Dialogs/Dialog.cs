using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List

public class Dialog : MonoBehaviour {

	// the prefab we use to generate dialog bubbles
	public GameObject phylacterePrefab;
	GameObject phylactere = null;

	public bool IsTalking() { return otherDialog != null; }

	// a pointer to the other we're discussing with
	GameObject otherPersona = null;
	Dialog otherDialog = null;
	// are we the active one who initiated the conversation
	bool initiatedDialog = false;
	// the dialogID is based on the active (initiator) objectId
	// the IDs of the two dialoging objects
	string dialogID = "";

	// our previous rotation
	Quaternion previousOrientation = Quaternion.identity;


	//////////////////////////


	void OnApplicationQuit() {

		killPhylactere();

	}


	void OnDisable() {

		killPhylactere();

	}


	void OnDestroy() {

		killPhylactere();

	}


	void killPhylactere() {

		// if exists, kill it
		if (phylactere != null) {
			phylactere.GetComponent<Phylactere>().abortReply();
			Destroy(phylactere);
		}

	}


	//////////////////////////


	void Update () {

	}


	//////////////////////////


	void startDialog() {

		// tell the submissive other to start the actual talking
		otherDialog.reply();

	}



	void speak(List<string> phrases) {

		createPhylactere();
		phylactere.GetComponent<Phylactere>().speak(phrases);

	}


	void createPhylactere() {

		// make sure we don't have any dangling phylacteres
		killPhylactere();

		phylactere = (GameObject)Instantiate(phylacterePrefab);
		phylactere.name = "Phylactere";
		phylactere.transform.parent = this.transform;
		phylactere.transform.localPosition = new Vector3(0,1.5f,0);

	}


	void reply() {

		if (initiatedDialog) replyActive();
		else replyPassive();

	}


	void replyPassive() {

		List<string> phrases = Scenario.Instance.GetPhrases(dialogID,false);

		// if error
		if (phrases.Count == 0) return;

		// if there are several possible phrases, randomly choose one
		if (phrases.Count > 1) {
			string phrase = phrases[(int)Random.Range(0,phrases.Count)];
			// empty the list
			phrases = new List<string>();
			phrases.Add(phrase);
		}

		speak(phrases);

	}


	void replyActive() {

		List<string> phrases = Scenario.Instance.GetPhrases(dialogID,true);

		// if error
		if (phrases.Count == 0) return;

		speak(phrases);

	}


	public void finishedSpeaking(int choiceIndex) {

		// if there's no more other, we must have finished speaking. No need to reply
		if (otherPersona == null || otherDialog == null) return;
		// tell the dialogue engine we've made a choice
		Scenario.Instance.Choose(dialogID, choiceIndex);
		// tell the other to reply
		otherDialog.reply();

	}


	public void finishedSpeaking() {

		// if there's no more other, we must have finished speaking. No need to reply
		if (otherPersona == null || otherDialog == null) return;

		// if we're the subordinate one, let the initiator reply
		if (!initiatedDialog) {
			// tell the other to reply to use
			otherDialog.reply();
		}

		// if we're the dominant one
		if (initiatedDialog) {

			// increment the index by 1
			Scenario.Instance.Next(dialogID);
			// tell the other to reply
			otherDialog.reply();
		} 

	}



	public void ClickAccelerate() {

		// if we're not talking, get outta here
		if (!IsTalking()) return;

		// if we're the one talking
		if (phylactere != null) phylactere.GetComponent<Phylactere>().ClickAccelerate();

		// otherwise it's the other one that's probably talking
		else otherDialog.ClickAccelerateFromOther();

	}


	public void ClickAccelerateFromOther() {

		// if we're not talking, get outta here
		if (!IsTalking()) return;

		// if we're the one talking
		if (phylactere != null) phylactere.GetComponent<Phylactere>().ClickAccelerate();

		// avoid infinite loop of recursive accelerators

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
		// remember that I started this dialog
		initiatedDialog = true;
		// remember who that Persona is
		rememberOther(other);
		// kill any previous phylacteres
		killPhylactere();
		// the dialog has opened, start the actual discussion
		startDialog();

	}


	public bool submitPassivelyToDialog(GameObject other) {

		// if we're already talking to someone
		if (IsTalking()) {
			print("Already Talking");
			return false;
		}

		// remember that the other Persona started this dialog
		initiatedDialog = false;
		// remember who that Persona is
		rememberOther(other);
		// kill any previous phylacteres
		killPhylactere();
		// remember where we were facing before
		previousOrientation = transform.localRotation;
		// turn to face that person
		transform.LookAt(other.transform.position, Vector3.up);
		// ok, we are free to start dialog with the other
		return true;

	}


	public void abortDialog() {

		if (!IsTalking()) return;

		// if we initiated this dialog
		if (initiatedDialog) {
			// stop talking
			abortActive();
		}

		// if we were the passive responder to a dialog
		if (!initiatedDialog) {
			// stop being socially submissive! ;-)
			abortPassive();
		}

		// reset "other" flag
		forgetOther();

	}


	void abortActive() {

		// we've broken the conversation, stop talking to indicate this
		killPhylactere();
		// tell the other it's over
		otherDialog.abortDialog();

	}


	void abortPassive() {

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

		// are we the active or passive one?
		if (initiatedDialog) {
			dialogID = "" + getId() + "," + otherDialog.getId();
		} else {
			dialogID = "" + otherDialog.getId() + "," + getId();
		}

	}


	int getId() {
		return GetInstanceID();
	}


	void forgetOther() {
		
		initiatedDialog = false;

		otherPersona = null;
		otherDialog = null;

		dialogID = "";

	}


}
