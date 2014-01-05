using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List & Dictionary

public class Talk : MonoBehaviour {

	
	// are we already talking to someone?
	public bool IsTalking { get { return otherTalker != null; } }
	// the history of each dialog <stringCombiningTwoInstancesIDs,indexOfDialog>
	private static Dictionary<string,DialogHistory> histories;
	// the prefab we use to generate dialog bubbles
	public GameObject phylacterePrefab;
	GameObject phylactere = null;
	// a pointer to the other we're discussing with
	GameObject otherPersona = null;
	Talk otherTalker = null;
	// are we the active one who initiated the conversation
	bool initiatedDialog = false;
	// the dialogID is based on the active (initiator) objectId
	// the IDs of the two dialoging objects
	string dialogID = "";
	string instanceID = "";
	// our previous rotation before the dialog started
	Quaternion previousOrientation = Quaternion.identity;


	////////////////////// Init

	void Awake() {

        Scenario.Instance.LoadDialogues();

	}

	//////////////////////


	void OnApplicationQuit() {

		KillPhylactere();

	}


	void OnDisable() {

		KillPhylactere();

	}


	void OnDestroy() {

		KillPhylactere();

	}


	void KillPhylactere() {

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


	void StartDialog() {

		// if we haven't created the Dictionary yet
		if (histories == null) histories = new Dictionary<string,DialogHistory>();

		// check to see if there's already a key for this dialog couple
		if (histories.ContainsKey(instanceID) && !histories[instanceID].IsTooOld()) {
			histories[instanceID].ResetTimer();
			print("Updating old dialog (" + dialogID + "/" + instanceID + ")");
		} else {
			// create the entry in the dictionary
			histories[instanceID] = new DialogHistory(0);
			print("Created new dialog (" + dialogID + "/" + instanceID + ")");
		}

		// if the dialogue is currently at zero, start it
		if (Scenario.Instance.IsNeutral(dialogID,instanceID)) {
			Scenario.Instance.StartConversation(dialogID,instanceID);
		}

		// tell the submissive other to start the actual talking
		otherTalker.Reply();

	}



	void Speak(List<string> phrases) {

		CreatePhylactere();
		phylactere.GetComponent<Phylactere>().Speak(phrases);

	}


	void CreatePhylactere() {

		// make sure we don't have any dangling phylacteres
		KillPhylactere();

		phylactere = (GameObject)Instantiate(phylacterePrefab);
		phylactere.name = "Phylactere";
		phylactere.transform.parent = this.transform;
		phylactere.transform.localPosition = new Vector3(0,1.5f,0);

	}


	void Reply() {

		if (initiatedDialog) ReplyActive();
		else ReplyPassive();

	}


	void ReplyPassive() {

		List<string> phrases = Scenario.Instance.GetPhrases(dialogID,instanceID,false);

		// if no Reply
		if (phrases.Count == 0) {
			// stop talking
			AbortDialog();
			// don't Speak
			return;
		}

		// if there are several possible phrases, randomly choose one
		if (phrases.Count > 1) {
			string randomPhrase = phrases[(int)Random.Range(0,phrases.Count)];
			// empty the list
			phrases = new List<string>();
			phrases.Add(randomPhrase);
		}

		Speak(phrases);

	}


	void ReplyActive() {

		List<string> phrases = Scenario.Instance.GetPhrases(dialogID,instanceID,true);

		// if we don't have anything to Reply
		if (phrases.Count == 0) {
			// set the conversation index back to zero
			Scenario.Instance.Reset(dialogID,instanceID);
			// stop talking
			AbortDialog();
			// get outta here
			return;
		}

		Speak(phrases);

	}


	public void FinishedSpeaking(string chosenPhrase) {

		// if there's no more other, we must have finished Speaking. No need to Reply
		if (otherPersona == null || otherTalker == null) return;

		// if we're the subordinate one, let the initiator Reply
		if (!initiatedDialog) {
			// tell the other to Reply to use
			otherTalker.Reply();
		}

		// if we're the dominant one
		if (initiatedDialog) {
			// tell the dialogue engine we've made a choice
			Scenario.Instance.Choose(dialogID,instanceID,chosenPhrase);
			// tell the other to Reply
			otherTalker.Reply();
		} 

	}


	public void ClickAccelerate() {

		// if we're not talking, get outta here
		if (!IsTalking) return;

		// if we're the one talking
		if (phylactere != null) phylactere.GetComponent<Phylactere>().ClickAccelerate();

		// otherwise it's the other one that's probably talking
		else otherTalker.ClickAccelerateFromOther();

	}


	public void ClickAccelerateFromOther() {

		// if we're not talking, get outta here
		if (!IsTalking) return;

		// if we're the one talking
		if (phylactere != null) phylactere.GetComponent<Phylactere>().ClickAccelerate();

		// avoid infinite loop of recursive accelerators

	}


	//////////////////////////


	// MARK: Start Dialog as Active (activateDialog) or Passive (SubmitPassivelyToDialog)

	public void ActivateDialog(GameObject other) {

		// get that other's Dialog
		otherTalker = other.GetComponent<Talk>();
		// if no social engine, quit discussing with this persona
		if (otherTalker == null) return;
		// tell the other to submit to this dialog request and see if they're ok
		if (!otherTalker.SubmitPassivelyToDialog(gameObject))  {
			// if there was some error in initalizing dialog
			print("unable to establish dialog with other");
			// forget that other's social engine
			ForgetOther();
			return;
		}
		// remember that I started this dialog
		initiatedDialog = true;
		// remember who that Persona is
		RememberOther(other);
		// kill any previous phylacteres
		KillPhylactere();
		// the dialog has opened, start the actual discussion
		StartDialog();

	}


	public bool SubmitPassivelyToDialog(GameObject other) {

		// if we're already talking to someone
		if (IsTalking) {
			print("Already Talking");
			return false;
		}

		// remember that the other Persona started this dialog
		initiatedDialog = false;
		// remember who that Persona is
		RememberOther(other);
		// kill any previous phylacteres
		KillPhylactere();
		// remember where we were facing before
		previousOrientation = transform.localRotation;
		// turn to face that person
		transform.LookAt(other.transform.position, Vector3.up);
		// ok, we are free to start dialog with the other
		return true;

	}


	public void AbortDialog() {

		if (!IsTalking) return;

		// if we initiated this dialog
		if (initiatedDialog) {
			// stop talking
			AbortActive();
		}

		// if we were the passive responder to a dialog
		if (!initiatedDialog) {
			// stop being socially submissive! ;-)
			AbortPassive();
		}

		// reset "other" flag
		ForgetOther();

	}


	void AbortActive() {

		// we've broken the conversation, stop talking to indicate this
		KillPhylactere();
		// tell the other it's over
		otherTalker.AbortDialog();

	}


	void AbortPassive() {

		// turn back to whatever we were doing
		transform.localRotation = previousOrientation;
		// clear Previous Orientation variable
		previousOrientation = Quaternion.identity;

	}


	//////////////////////////


	void RememberOther(GameObject other) {

		// remember who that Persona is
		otherPersona = other;
		// get that other's Dialog
		otherTalker = other.GetComponent<Talk>();

		// are we the active or passive one?
		if (initiatedDialog) {
			dialogID = "" + GetID() + "-" + otherTalker.GetID();
			instanceID = "" + GetInstanceID() + "_" + otherTalker.GetInstanceID();
		} else {
			dialogID = "" + otherTalker.GetID() + "-" + GetID();
			instanceID = "" + otherTalker.GetInstanceID() + "_" + GetInstanceID();
		}

	}


	string GetID() {
		return gameObject.name;
	}


	void ForgetOther() {
		
		initiatedDialog = false;

		otherPersona = null;
		otherTalker = null;

		dialogID = "";
		instanceID = "";

	}


}
