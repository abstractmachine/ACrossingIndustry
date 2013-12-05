using UnityEngine;
using System.Collections;

public class SocialEngine : MonoBehaviour {

	// the prefab we use to generate dialog bubbles
	public GameObject phylacterePrefab;

	bool areTalkingToSomeone = false;
	bool didInitiateDialog = false;

	// a pointer to the other we're discussing with
	GameObject currentOther = null;
	SocialEngine otherSocialEngine = null;

	// our previous rotation
	Quaternion previousOrientation = Quaternion.identity;

	int speechIndex = -1;

	// Use this for initialization
	void Start () {
	
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

		// instantiate new GameObject from prefab
		GameObject obj = (GameObject)Instantiate(phylacterePrefab);
		// attach this object to us
		obj.transform.parent = transform;
		// position the phylactere object
		obj.transform.localPosition = new Vector3(0,1.5f,0);
		// get pointer to text field
		TextField textField = obj.transform.GetComponentInChildren<TextField>();
		// give this phylactere object it's text, base time delay on length of text
		textField.speak(phrase, -1);

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
			phrase = "Hello " + currentOther.name;
			break;

			case 1:
			phrase = "How are you, " + currentOther.name + "?";
			break;

			case 2:
			phrase = "I'm doing ok.";
			break;

			default:
			speak("...");
			break;

		}

		speak(phrase);

	}


	void replyActive() {

		string phrase = "";

		switch(speechIndex) {

			case 0:
			phrase = "Hello " + currentOther.name;
			break;

			case 1:
			phrase = "I'm fine, " + currentOther.name + ". And you?";
			break;

			case 2:
			phrase = "That's good.";
			break;

			default:
			speak("...");
			break;

		}

		speak(phrase);

	}


	void abortActive() {

		speak("Goodbye " + currentOther.name);
		// tell the other it's over
		otherSocialEngine.abortDialog();

	}


	void abortPassive() {

		speak("Goodbye " + currentOther.name);

		// turn back to whatever we were doing
		transform.localRotation = previousOrientation;
		// clear Previous Orientation variable
		previousOrientation = Quaternion.identity;

	}


	public void finishedSpeaking() {

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
		currentOther = other;
		// remember who that Persona is
		rememberOther(other);
		// make sure we don't have any dangling phylacteres
		killPhylacteres();
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
		// make sure we don't have any dangling phylacteres
		killPhylacteres();
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

		// if we weren't actually talking to someone (error trapping)
		if (currentOther == null || otherSocialEngine == null) {
			print("error " + currentOther + "\t - \t" + otherSocialEngine);
			forgetOther();
			return;
		}

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


	void killPhylacteres() {

		// kill all previous phylacteres
		Transform previousPhylactere = transform.Find("Phylactere(Clone)");
		// if exists, kill it
		if (previousPhylactere != null) Destroy(previousPhylactere.gameObject);

	}


	void rememberOther(GameObject other) {

		// remember who that Persona is
		currentOther = other;

	}


	void forgetOther() {
		
		areTalkingToSomeone = false;
		didInitiateDialog = false;
		currentOther = null;
		otherSocialEngine = null;

	}


}
