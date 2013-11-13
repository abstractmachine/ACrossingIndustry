using UnityEngine;
using System.Collections;
using System; // for the Evironment.EndLine constant


public class TextField : MonoBehaviour {
	
	// how slowly/quickly the text writes out
	public float speechDelay = 0.01f;

	// a pointer to the text mesh component
	TextMesh textMesh;

	// a pointer to the frame around the text (since we need to resize it)
	GameObject speechFrame;

	// the actual speech variables
	string speechText = "";
	int speechIndex = 0;
	float speechTimer = 0;
	

	// we need to inialize objects before Start()
	void Awake() {

		// the text we write into
		textMesh = GetComponent<TextMesh>();
		
		// the cadre of the text
		speechFrame = transform.parent.Find("Bulle").Find("PlaneScaler").gameObject;

	}

	
	// Update is called once per frame
	void Update () {
		
		// if we still need to write out speech 
		if (speechIndex < speechText.Length) speakNextCharacter();
	
	}


	void OnDestroy() {

		if (speechText == "") return;

		finishedSpeaking();

	}
	

	public void speak(string newText, float howLong){

		// reset line player back to 0
		speechIndex = 0;
		speechTimer = 0;
		speechText = newText;

		resizeFrame();

		// howLong == -1 means, calculate using line length
		if (howLong == -1) {
			int lineCount = speechText.Split('\n').Length;
			howLong = 0.5f + (3.5f*lineCount);
		}

		// if we've specified a length
		if(howLong > 0) {
			Destroy(gameObject.transform.parent.gameObject, howLong);
		}

	}


	void speakNextCharacter() {

		// count down
		speechTimer -= Time.deltaTime;
		if (speechTimer > 0) return;

		// reset timer for next time
		speechTimer = speechDelay;
		// increment index
		speechIndex++;
		// write out text
		textMesh.text = parseString(speechText.Substring(0,speechIndex));
		// resize since we've drawn text
		resizeFrame();

		// are we at the end of the speech?
		//if (speechIndex >= speechText.Length) finishedSpeaking(speechText);

	}


	void finishedSpeaking() {

		// make sure the parent is there
		GameObject parentObject = transform.parent.gameObject;
		// if not, leave
		if (parentObject == null) return;
		// tell the parent object we've finished speaking
		parentObject.GetComponent<TextSpeaker>().finishedSpeaking();

	}


	string parseString(string inputString) {

		string finalString = "";
		int lineLength = 0;

		for(int i=0; i<inputString.Length; i++) {

			finalString += inputString[i];
			lineLength++;

			// if we're not the last letter, and if we're not at the end of the text
			if (lineLength >= 18 && i < inputString.Length-1) {
				// make sure we're not adding spaces at the beginning of the line
				if (!(inputString[i] == ' ' && lineLength == 0)) {
					// break the text into a new line
					finalString += Environment.NewLine;
				}
				// start counting the next line at zero
				lineLength = 0;
			}

		}

		return finalString;

	}


	void resizeFrame() {

		int lineCount = textMesh.text.Split('\n').Length;

		float scaleY = 0.01f * (20 + (29 * lineCount));
		
		Vector3 newScale = speechFrame.transform.localScale;
		newScale.y = scaleY;
		speechFrame.transform.localScale = newScale;

	}


}
