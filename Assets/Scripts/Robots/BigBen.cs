using UnityEngine;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public class BigBen : MonoBehaviour {

	int currentHour = -1;

	float indexCountDownDuration = 0.05f;
	float indexCountDown = 0.05f; // seconds
	float finishedCountDown = 0.0f;
	float finishedDuration = 2.0f; // seconds

	string phrase = "";
	int praseIndex = -1;

	TextMesh textMesh;
	Daylight daylight;

	// Use this for initialization
	void Start () {
	
		daylight = Camera.main.GetComponent<Daylight>();

		textMesh = GetComponent<TextMesh>();
		transform.renderer.material.color = Color.grey;

		//line = transform.parent.Find("Line");
		//lineRenderer = line.GetComponent<LineRenderer>();
		//lineRenderer.gameObject.SetActive(true);
		//lineRenderer.material.color = Color.grey;

	}
	
	// Update is called once per frame
	void Update () {
	
		LookAtMainCamera();

		if (praseIndex > -1 && praseIndex < phrase.Length) {
			SpellString();
		} else if (praseIndex > -1 && praseIndex >= phrase.Length) {
			finishedCountDown -= Time.deltaTime;
			if (finishedCountDown < 0.0f) EraseString();
		}

		if (praseIndex > -1 && finishedCountDown < 0.0f) {
			EraseString();
		}

		// is this a new hour?
		if (currentHour == daylight.TimeHour) return;
		currentHour = daylight.TimeHour;

		string speech = CreateString("The time is " + currentHour + " o'clock");

		// create the phrase we have to speak
		SpeakString(speech);

	}


	void LookAtMainCamera() {

		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0.0f,180.0f,0.0f);

	}


	string CreateString(string newString) {

		return newString;

	}


	void SpeakString(string newString) {

		praseIndex = 0;
		indexCountDown = indexCountDownDuration;
		finishedCountDown = finishedDuration;

		phrase = Obscurify(newString);

		//line.gameObject.SetActive(true);

	}


	void SpellString() {

		indexCountDown -= Time.deltaTime;

		if (indexCountDown < 0) {

			indexCountDown = indexCountDownDuration;
			praseIndex++;

			string drawText = phrase.Substring(0,praseIndex);
			drawText = Colorize(drawText);
			textMesh.text = drawText;

			float sat = daylight.TimeSaturation;

			// black or white? contrast with background
			if (sat > 0.55) { // FIXME: this seems backwards
				transform.renderer.material.color = Color.black;
				//lineRenderer.material.color = Color.black;
			} else {
				transform.renderer.material.color = Color.white;
				//lineRenderer.material.color = Color.white;
			}

		} // if (indexCountDown < 0)

	} // SpellString()


	void EraseString() {

		textMesh.text = "";
		//line.gameObject.SetActive(false);

	}


	string Obscurify(string inputString) {

		StringBuilder outputString = new StringBuilder(inputString);

		int wordCount = Regex.Matches(inputString, @"[\S]+").Count;
		int randomWordIndex = (int)Random.Range(0,wordCount);

		int wordIndex = 0;

		for(int i=0; i<inputString.Length; i++) {

			char c = outputString[i];

			// keep digits as is
			if (char.IsDigit(c)) continue;

			// count words
			if (char.IsSeparator(c)) {
				wordIndex++;
				continue;
			}

			// randomly let one word through
			if (randomWordIndex == wordIndex) continue;

			switch((int)Random.Range(0.0f,10.0f)) {
				case 0 :	outputString[i] = '#';	break;
				case 1 :	outputString[i] = '#';	break;
				case 2 :	outputString[i] = '.';	break;
				case 3 :	outputString[i] = ';';	break;
				case 4 :	outputString[i] = ':';	break;
				case 5 :	outputString[i] = '%';	break;
				case 6 :	outputString[i] = '*';	break;
				case 7 :	outputString[i] = '$';	break;
				case 8 :	outputString[i] = '*';	break;
				case 9 :	outputString[i] = '&';	break;
			}
		}

		return outputString.ToString();

	}



	string Colorize(string inputString) {

		string outputString = "";

		foreach (char str in inputString) {
	    	outputString += "<color=#FF0000>" + str + "</color>";
		}

		return outputString;

	}

}
