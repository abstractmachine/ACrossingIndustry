using UnityEngine;
using System.Collections;

public class Dialogue : MonoBehaviour {

	TextMesh textMesh;

	int phraseIndex = -1;
	string phrase = "";

	float indexCountDownDuration = 0.05f;
	float indexCountDown = 0.05f; // seconds
	float finishedCountDown = 0.0f;
	float finishedDuration = 2.0f; // seconds

	// Use this for initialization
	void Start () {
	
		textMesh = GetComponent<TextMesh>();
		transform.renderer.material.color = Color.grey;

	}
	
	// Update is called once per frame
	void Update () {
	
		lookAtMainCamera();

		if (phraseIndex > -1 && phraseIndex < phrase.Length) {
			spellString();
		} else if (phraseIndex > -1 && phraseIndex >= phrase.Length) {
			finishedCountDown -= Time.deltaTime;
			if (finishedCountDown < 0.0f) eraseString();
		}

		if (phraseIndex > -1 && finishedCountDown < 0.0f) {
			eraseString();
		}


		if (Input.GetKeyDown(KeyCode.Space)) {
			// create the phrase we have to speak
			createString("Nothing to be done.");
		}

	}


	void lookAtMainCamera() {

		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0.0f,180.0f,0.0f);

	}


	void createString(string newString) {

		phraseIndex = 0;
		indexCountDown = indexCountDownDuration;
		finishedCountDown = finishedDuration;

		phrase = newString;

	}


	void spellString() {

		indexCountDown -= Time.deltaTime;

		if (indexCountDown < 0) {

			indexCountDown = indexCountDownDuration;
			phraseIndex++;

			string drawText = phrase.Substring(0,phraseIndex);
			textMesh.text = drawText;
			// black or white? contrast with background
			if (GameState.Instance.getTimeSaturation() > 0.4) { // FIXME: this seems backwards
				transform.renderer.material.color = Color.white;
			} else {
				transform.renderer.material.color = Color.black;
			}
		}

	}


	void eraseString() {

		textMesh.text = "";

	}

}
