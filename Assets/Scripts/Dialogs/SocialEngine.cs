using UnityEngine;
using System.Collections;

public class SocialEngine : MonoBehaviour {

	// the prefab we use to generate dialog bubbles
	public GameObject phylacterePrefab;
	GameObject phylactere = null;

	bool areTalkingToSomeone = false;
	bool didInitiateDialog = false;

	// a pointer to the other we're discussing with
	GameObject otherPersona = null;
	SocialEngine otherSocialEngine = null;

	// our previous rotation
	Quaternion previousOrientation = Quaternion.identity;

	int speechIndex = -1;

	// Use this for initialization
	void Start () {
	
	}


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

	
	// Update is called once per frame
	void Update () {

	}


	void startDialog() {

		// start the index at 0 (beginning of phrasebook)
		// TODO: change this to figure out where the last index was when we left off
		speechIndex = 0;
		// tell the submissive other to start the actual talking
		otherSocialEngine.reply(speechIndex);

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

		speechIndex = index;

		if (didInitiateDialog) replyActive();
		else replyPassive();

	}


	void replyPassive() {

		string phrase = "";

		switch(speechIndex) {

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

		switch(speechIndex) {

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
		if (otherPersona == null || otherSocialEngine == null) return;

		// if we're the subordinate one, let the initiator reply
		if (!didInitiateDialog) {
			// tell the other to reply to use
			otherSocialEngine.reply(speechIndex);
		}

		// if we're the dominant one
		if (didInitiateDialog) {

			// increment the index by 1
			speechIndex++;
			// tell the other to reply to use
			otherSocialEngine.reply(speechIndex);
		} 

	}






	// MARK: Start Dialog as Active (activateDialog) or Passive (submitPassivelyToDialog)

	public void activateDialog(GameObject other) {

		// get that other's SocialEngine
		otherSocialEngine = other.GetComponent<SocialEngine>();
		// if no social engine, quit discussing with this persona
		if (otherSocialEngine == null) return;
		// tell the other to submit to this dialog request and see if they're ok
		if (!otherSocialEngine.submitPassivelyToDialog(gameObject))  {
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
		// remember who I'm talking to
		otherPersona = other;
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
		// get that other's SocialEngine
		otherSocialEngine = other.GetComponent<SocialEngine>();
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

		speak("Goodbye " + otherPersona.name);
		// tell the other it's over
		otherSocialEngine.abortDialog();

	}


	void abortPassive() {

		speak("Goodbye " + otherPersona.name);

		// turn back to whatever we were doing
		transform.localRotation = previousOrientation;
		// clear Previous Orientation variable
		previousOrientation = Quaternion.identity;

	}


	void rememberOther(GameObject other) {

		// remember who that Persona is
		otherPersona = other;

	}


	void forgetOther() {
		
		areTalkingToSomeone = false;
		didInitiateDialog = false;
		otherPersona = null;
		otherSocialEngine = null;

	}


}
