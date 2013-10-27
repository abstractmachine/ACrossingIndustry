using UnityEngine;
using System.Collections;

public class BigBen : MonoBehaviour {

	int currentHour = -1;

	float indexCountDownDuration = 0.05f;
	float indexCountDown = 0.05f; // seconds
	float finishedCountDown = 0.0f;
	float finishedDuration = 2.0f; // seconds

	string phrase = "";
	int praseIndex = -1;

	TextMesh textMesh;

	// Use this for initialization
	void Start () {
	
		textMesh = GetComponent<TextMesh>();
		transform.renderer.material.color = Color.grey;

		//line = transform.parent.Find("Line");
		//lineRenderer = line.GetComponent<LineRenderer>();
		//lineRenderer.gameObject.SetActive(true);
		//lineRenderer.material.color = Color.grey;

	}
	
	// Update is called once per frame
	void Update () {
	
		lookAtMainCamera();

		if (praseIndex > -1 && praseIndex < phrase.Length) {
			spellString();
		} else if (praseIndex > -1 && praseIndex >= phrase.Length) {
			finishedCountDown -= Time.deltaTime;
			if (finishedCountDown < 0.0f) eraseString();
		}

		if (praseIndex > -1 && finishedCountDown < 0.0f) {
			eraseString();
		}

		// is this a new hour?
		if (currentHour == GameState.Instance.getTimeHour()) return;
		currentHour = GameState.Instance.getTimeHour();

		string speech = createString("The time is " + currentHour + " o'clock");

		// create the phrase we have to speak
		speakString(speech);

	}


	void lookAtMainCamera() {

		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0.0f,180.0f,0.0f);

	}


	string createString(string newString) {

		return newString;

	}


	void speakString(string newString) {

		praseIndex = 0;
		indexCountDown = indexCountDownDuration;
		finishedCountDown = finishedDuration;

		phrase = Gervais.obscurify(newString);

		//line.gameObject.SetActive(true);

	}


	void spellString() {

		indexCountDown -= Time.deltaTime;

		if (indexCountDown < 0) {

			indexCountDown = indexCountDownDuration;
			praseIndex++;

			string drawText = phrase.Substring(0,praseIndex);
			drawText = Gervais.colorize(drawText);
			textMesh.text = drawText;

			float sat = GameState.Instance.getTimeSaturation();

			// black or white? contrast with background
			if (sat > 0.55) { // FIXME: this seems backwards
				transform.renderer.material.color = Color.black;
				//lineRenderer.material.color = Color.black;
			} else {
				transform.renderer.material.color = Color.white;
				//lineRenderer.material.color = Color.white;
			}

		} // if (indexCountDown < 0)

	} // spellString()


	void eraseString() {

		textMesh.text = "";
		//line.gameObject.SetActive(false);

	}

}
