/* TODO:
 * Determine if dialog is between two Personae or a Player-Persona.
   - If it's between two Personae, the phylacteres dissapear automatically
   - If it's between Player-Persona, the dialog requires clicks to advance
 */


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
		// the dialogName is based on the active (Player) name + passive (Persona) name
		string dialogName = "";
		// the memory instance ID of the two dialoging objects
		string dialogID = "";
		// our previous rotation before the dialog started
		Quaternion previousOrientation = Quaternion.identity;
		// access to Data object
		Data data;
		// access to condition object
		Condition condition;
	
	
		void Start() {
		
				data = Camera.main.GetComponent<Data>();
				condition = GetComponent<Condition>();
		
		}
	
	
		//////////////////////////
	
	
		void StartDialog() {
		
				// if we haven't created the Dictionary yet
				if (histories == null)
						histories = new Dictionary<string,DataHistory>();
		
				// check to see if there's already a key for this DialogHistory
				if (!histories.ContainsKey(dialogID)) {
						// create the entry in the dictionary
						histories[dialogID] = new DataHistory();
				}
		
				// if we need to start the dialog
				if (histories[dialogID].IsNeutral) {
						histories[dialogID].Start();
				}
		
				// tell the submissive other to start the actual talking
				otherTalker.Reply();
		
		}
	
	
	
		void Speak(List<string> phrases) {
		
				CreatePhylactere();
				//phylactere.GetComponent<Phylactere>().Speak(phrases);
				phylactere.GetComponent<Phylactere>().Speak(phrases);
	}
	
	
		void Reply() {
		
				if (initiatedDialog)
						ReplyActive();
				else
						ReplyPassive();
		
		}
	
	
		void ReplyPassive() {
		
				// what is the current index of this conversation?
				int index = histories[dialogID].Index;
		
				// get the current available utterances
				Utterance utterance = data.GetUtteranceFromIndex(dialogName, index);
				
				if (utterance == null) {
						// error report
						print("null utterance at " + dialogName + "/" + index);
						// stop talking
						AbortDialog();
						// don't Speak
						return;
				}
				
				// create an empty list of phrases
				//List<string> phrases = new List<string>();
				
				// TODO: go through the list of Persona SpeechActs
/*				foreach(SpeechAct speechAct in utterance.PersonaSpeechActs) {

				}
*/		
				// using current index level, get a List<> of possible phrases
				List<string> phrases = data.GetPersonaPhrasesFromIndex(dialogName, index);
		
				// if no Reply
				if (phrases.Count == 0) {
						// stop talking
						AbortDialog();
						// don't Speak
						return;
				}
		
				// if there are several possible phrases, randomly choose one
				if (phrases.Count > 1) {
						int randomIndex = (int)Random.Range(0, phrases.Count);
						string randomPhrase = phrases[randomIndex];
						// empty the list
						phrases = new List<string>();
						// add it back with the chosen phrase
						phrases.Add(randomPhrase);
				}
		
				Speak(phrases);
		
		}
	
	
		void ReplyActive() {
		
				// what is the current index of this conversation?
				int index = histories[dialogID].Index;
				// using current index level, get a List<> of possible phrases
				List<string> phrases = data.GetPlayerPhrasesFromIndex(dialogName, index);
		
				// if we don't have anything to Reply
				if (phrases.Count == 0) {
						// set the conversation index back to zero
						histories[dialogID].Reset();
						// stop talking
						AbortDialog();
						// get outta here
						return;
				}
		
				Speak(phrases);
		
		}
	
	
		public void FinishedSpeaking(string chosenPhrase) {
		
				// if there's no more other, we must have finished Speaking. No need to Reply
				if (otherPerson == null || otherTalker == null){
						return;
				}
				// if we're the subordinate one, let the initiator Reply
				if (!initiatedDialog){
						FinishedSpeakingPersona(chosenPhrase);
				}
				// if we're the dominant one
				if (initiatedDialog){
						FinishedSpeakingPlayer(chosenPhrase);
				}
		}
	
	
		void FinishedSpeakingPersona(string chosenPhrase) {
		
				// TODO: figure out Action based on chosen phrase (if it was randomly chosen)
				//print("Persona Said:" + chosenPhrase);
		
				// tell the other to Reply to use
				otherTalker.Reply();
		
		}
	
	
		void FinishedSpeakingPlayer(string chosenPhrase) {
		
				// figure out the index of the player's choice
		
				//print("Player said:" + chosenPhrase);
		
				// what is the current index of this conversation?
				int index = histories[dialogID].Index;
				// figure out the index of this player phrase
				Utterance utterance = data.GetUtteranceFromIndex(dialogName, index);
				// a flag to know if we found the phrase
				bool foundPhrase = false;
				// go through the player speech acts
				foreach(SpeechAct speechAct in utterance.player) {
						// if this is not our phrase, move on
						if (speechAct.phrase != chosenPhrase)
								continue;
						// ok, we found it
						foundPhrase = true;
						// get the list of consequences
						SetNextDialogFromPlayerConsequences(speechAct.consequences);
				}
		
				// if we didn't find the phrase
				if (!foundPhrase) {
						// then that's some sort of error
						print("Couldn't find phrase for index " + index + " in dialogName " + dialogName);
						histories[dialogID].Reset();
				}
		
				// tell the other to Reply
				otherTalker.Reply();
		
		}
	
	
		void DoPersonaAction(string action, string arguments) {
		
		
		
		}
	
	
		void DoPlayerAction(string action, string arguments) {
		
		
		}
	
	
		void SetNextDialogFromPlayerConsequences(List<Consequence> consequences) {
		
				// error trapping
				if (consequences == null || consequences.Count == 0) {
						print("No valid consequences for index " + histories[dialogID].Index + " in " + dialogName);
						histories[dialogID].Reset();
						return;
				}
		
				// begin with no consequence
				int whichConsequence = -1;
		
				// choose consequence based on conditions
				for(int i=0; i<consequences.Count; i++) {
				
						Consequence consequence = consequences[i];	
						// don't check null values
						if (consequence.RawString == null || consequence.RawString == "")
								continue;
						// check to see if conditions are correct
						if (condition.Check(consequence.RawString)) {
								// ok
								whichConsequence = i;
						}	
			
				}
		
				// if there was no chosen consequence, just choose one at random
				if (whichConsequence == -1) {
						// choose the first
						whichConsequence = (int)UnityEngine.Random.Range(0, consequences.Count);
				}
				// get that consequence
				Consequence selectedConsequence = consequences[whichConsequence];
				// extract next values
				List<int> nextIndexes = selectedConsequence.nexts;
				// choose one randomly
				int nextIndex = nextIndexes[(int)UnityEngine.Random.Range(0, nextIndexes.Count)];
				// set that to be our next dialog index
				SetDialogIndex(nextIndex);
		
		}
	
	
		void SetDialogIndex(int nextIndex) {
		
				// this will also automatically reset the timeout timer
				histories[dialogID].Index = nextIndex;
		
		}
	
	
		//////////////////////////
	
	
		// MARK: Start Dialog as Active (activateDialog) or Passive (SubmitPassivelyToDialog)
	
		public void ActivateDialog(GameObject other) {
		
				// get that other's Dialog
				otherTalker = other.GetComponent<Talk>();
				// if no social engine, quit discussing with this persona
				if (otherTalker == null)
						return;
				// tell the other to submit to this dialog request and see if they're ok
				if (!otherTalker.SubmitPassivelyToDialog(gameObject)) {
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
//	Souldn't this be other.transform.position?
				Vector3 otherPosition = other.transform.position;
				// annul y translation
				otherPosition.y = transform.position.y;
				// turn to face that person
				transform.LookAt(otherPosition, Vector3.up);
				// ok, we are free to start dialog with the other
				return true;
		
		}
	
	
		public void AbortDialog() {
		
				if (!IsTalking)
						return;
		
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
				KillPhylactere();
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
						dialogName = "" + this.ID + "-" + otherTalker.ID;
						dialogID = "" + this.GetInstanceID() + "_" + otherTalker.GetInstanceID();
				} else {
						dialogName = "" + otherTalker.ID + "-" + this.ID;
						dialogID = "" + otherTalker.GetInstanceID() + "_" + this.GetInstanceID();
				}
		
		}
	
	
		void ForgetOther() {
		
				initiatedDialog = false;
		
				otherPerson = null;
				otherTalker = null;
		
				dialogName = "";
				dialogID = "";
		
		}
	
	
		//////////////////////
	
	
		void CreatePhylactere() {
		
				// make sure we don't have any dangling phylacteres
				KillPhylactere();
		/*
				phylactere = (GameObject)Instantiate(phylacterePrefab);
				phylactere.name = "Phylactere";
				phylactere.transform.parent = this.transform;
				phylactere.transform.localPosition = new Vector3 (0, 1.62f, 0);
		*/
		phylactere = (GameObject)Instantiate(phylacterePrefab);
		phylactere.name = "Phylactere";
		phylactere.transform.parent = this.transform;
		phylactere.transform.localPosition = new Vector3 (0, 7.88f, 0);
		
		}
	
	
		void KillPhylactere() {
		
				// if exists, kill it
				if (phylactere != null) {
		/*				phylactere.GetComponent<Phylactere>().AbortReply();
						Destroy(phylactere);
		*/		
						phylactere.GetComponent<Phylactere>().AbortReply();
						Destroy(phylactere);
				}
		
		}
	
	
		//////////////////////
	
	
		public void ClickAccelerate() {
		
				// if we're not talking, get outta here
				if (!IsTalking)
						return;
		
				// if we're the one talking
				if (phylactere != null){
						//phylactere.GetComponent<Phylactere>().ClickAccelerate();
						phylactere.GetComponent<Phylactere>().ClickAccelerate();
				}
		// otherwise it's the other one that's probably talking
		else
						otherTalker.ClickAccelerateFromOther();
		
		}
	
	
		public void ClickAccelerateFromOther() {
		
				// if we're not talking, get outta here
				if (!IsTalking)
						return;
		
				// if we're the one talking
				if (phylactere != null)
						//phylactere.GetComponent<Phylactere>().ClickAccelerate();
						phylactere.GetComponent<Phylactere>().ClickAccelerate();
		
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
