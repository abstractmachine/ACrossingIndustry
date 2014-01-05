using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List & Dictionary

public class Talk : MonoBehaviour {

	
	// are we already talking to someone?
	public bool IsTalking { get { return otherTalker != null; } }
	// the name (ex: Enfant, Passeur, etc) of the person
	string ID { get { return gameObject.name; } }
	// the history of each dialog <stringCombiningTwoInstancesIDs,indexOfDialog>
	private static Dictionary<string,DataHistory> histories;
	// the prefab we use to generate dialog bubbles
	public GameObject phylacterePrefab;
	GameObject phylactere = null;
	// a pointer to the other we're discussing with
	GameObject otherPerson = null;
	Talk otherTalker = null;
	// are we the active one who initiated the conversation
	bool initiatedDialog = false;
	// the dialogID is based on the active (Player) name + passive (Persona) name
	string dialogID = "";
	// the memory instance ID of the two dialoging objects
	string instanceID = "";
	// our previous rotation before the dialog started
	Quaternion previousOrientation = Quaternion.identity;
	// access to Data object
	Data data;


	void Start() {

		data = Camera.main.GetComponent<Data>();

	}


	//////////////////////////


	void StartDialog() {

		// if we haven't created the Dictionary yet
		if (histories == null) histories = new Dictionary<string,DataHistory>();

		// check to see if there's already a key for this DialogHistory
		if (!histories.ContainsKey(instanceID)) {
			// create the entry in the dictionary
			histories[instanceID] = new DataHistory();
		}

		// if we need to start the dialog
		if (histories[instanceID].IsNeutral) {
			histories[instanceID].Start();
		}

		// tell the submissive other to start the actual talking
		otherTalker.Reply();

	}



	void Speak(List<string> phrases) {

		CreatePhylactere();
		phylactere.GetComponent<Phylactere>().Speak(phrases);

	}


	void Reply() {

		if (initiatedDialog) ReplyActive();
		else ReplyPassive();

	}


	void ReplyPassive() {

		// what is the current index of this conversation?
		int index = histories[instanceID].Index;
		// using current index level, get a List<> of possible phrases
		List<string> phrases = data.GetPersonaPhrasesFromIndex(dialogID,index);

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
			// add it back with the chosen phrase
			phrases.Add(randomPhrase);
		}

		Speak(phrases);

	}


	void ReplyActive() {

		// what is the current index of this conversation?
		int index = histories[instanceID].Index;
		// using current index level, get a List<> of possible phrases
		List<string> phrases = data.GetPlayerPhrasesFromIndex(dialogID,index);

		// if we don't have anything to Reply
		if (phrases.Count == 0) {
			// set the conversation index back to zero
			histories[instanceID].Reset();
			// stop talking
			AbortDialog();
			// get outta here
			return;
		}

		Speak(phrases);

	}


	public void FinishedSpeaking(string chosenPhrase) {

		// if there's no more other, we must have finished Speaking. No need to Reply
		if (otherPerson == null || otherTalker == null) return;

		// if we're the subordinate one, let the initiator Reply
		if (!initiatedDialog) {

			// TODO: figure out Action based on chosen phrase (if it was randomly chosen)

			// tell the other to Reply to use
			otherTalker.Reply();
		}

		// if we're the dominant one
		if (initiatedDialog) {

			// figure out the index of the player's choice

			// what is the current index of this conversation?
			int index = histories[instanceID].Index;
			// figure out the index of this player phrase
			Utterance utterance = data.GetUtteranceFromIndex(dialogID,index);
			// a flag to know if we found the phrase
			bool foundPhrase = false;
			// go through the player speech acts
			foreach(SpeechAct speechAct in utterance.player) {
				// if this is not our phrase, move on
				if (speechAct.phrase != chosenPhrase) continue;
				// ok, we found it
				foundPhrase = true;
				// get the list of consequences
				SetNextDialogFromConsequences(speechAct.consequences);
			}

			// if we didn't find the phrase
			if (!foundPhrase) {
				// then that's some sort of error
				print("Couldn't find phrase for index " + index + " in dialogID " + dialogID);
				histories[instanceID].Reset();
			}

			// tell the dialogue engine we've made a choice
			//Scenario.Instance.Choose(dialogID,instanceID,chosenPhrase);		// kill this
			// tell the other to Reply
			otherTalker.Reply();
		} 

	}


	void DoPlayerAction(string action, string arguments) {



	}


	void SetNextDialogFromConsequences(List<Consequence> consequences) {

		// error trapping
		if (consequences == null || consequences.Count == 0) {
			print("No valid consequences for index " + histories[instanceID].Index + " in " + dialogID);
			return;
		}

		/*
		// TODO: choose consequence based on conditions
		foreach(Consequence consequence in consequences) {

			// get the condition
			string function = consequence.ConditionFunction();
			string arguments = consequence.ConditionArguments();
			List<int> nextInts = consequence.nexts;

		}*/

		// for the moment, just choose the first consequence
		int whichConsequence = 0;
		// get that consequence
		Consequence consequence = consequences[whichConsequence];
		// extract next values
		List<int> nextIndexes = consequence.nexts;
		// choose one randomly
		int nextIndex = nextIndexes[(int)UnityEngine.Random.Range(0,nextIndexes.Count)];
		// set that to be our next dialog index
		SetDialogIndex(nextIndex);

	}


	void SetDialogIndex(int nextIndex) {

		// this will also automatically reset the timeout timer
		histories[instanceID].Index = nextIndex;

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
		// other position
		Vector3 otherPosition = other.transform.position;
		// annul y translation
		otherPosition.y = transform.position.y;
		// turn to face that person
		transform.LookAt(otherPosition, Vector3.up);
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
		otherPerson = other;
		// get that other's Dialog
		otherTalker = other.GetComponent<Talk>();

		// are we the active or passive one?
		if (initiatedDialog) {
			dialogID = "" + this.ID + "-" + otherTalker.ID;
			instanceID = "" + this.GetInstanceID() + "_" + otherTalker.GetInstanceID();
		} else {
			dialogID = "" + otherTalker.ID + "-" + this.ID;
			instanceID = "" + otherTalker.GetInstanceID() + "_" + this.GetInstanceID();
		}

	}


	void ForgetOther() {
		
		initiatedDialog = false;

		otherPerson = null;
		otherTalker = null;

		dialogID = "";
		instanceID = "";

	}


	//////////////////////


	void CreatePhylactere() {

		// make sure we don't have any dangling phylacteres
		KillPhylactere();

		phylactere = (GameObject)Instantiate(phylacterePrefab);
		phylactere.name = "Phylactere";
		phylactere.transform.parent = this.transform;
		phylactere.transform.localPosition = new Vector3(0,1.5f,0);

	}


	void KillPhylactere() {

		// if exists, kill it
		if (phylactere != null) {
			phylactere.GetComponent<Phylactere>().abortReply();
			Destroy(phylactere);
		}

	}


	//////////////////////


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


}
