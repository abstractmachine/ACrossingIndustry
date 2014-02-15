using UnityEngine;
//using System; // required for the Evironment.EndLine constant
using System.Collections;
using System.Collections.Generic; // required for List

public class Phylactere : MonoBehaviour {

	// how slowly/quickly the text writes out
	public float speechDelay = 0.01f;

	// the speech variables
	class Speech {
		public GameObject frame; 						// a pointer to the frame around the text (since we need to resize it)
		public List<string> texts = new List<string>();	// the text alternatives
		public int index = 0;							// which text are we displaying
		public string text = "";						// the actual text content
		public int charIndex = 0;						// the current index of the character
		public float charTimer = 0;						// the time remaining before next character
		public bool shouldReply = true; 				// this can be de-activated when conversations end
	}

	Speech speech = new Speech();

	class Spinner {
		public GameObject gameObject;			// a pointer to the spinner for rotating text
		public bool spinning = false;			// are we spinning or not
		public int targetAngle = 0;				// where we want to rotate to
		public float currentAngle = 0.0f;		// where we currently are
		public int index = 0;					// which rotation we're at
		public int lastRotation = -1;			// tracks whether we're front/back (i.e. if we've changed orientation)
		public float startTime = 0;				// when we started spinning
	}

	Spinner spinner = new Spinner();

	// the material used to color the phylacteres
	Material mat;

	// a pointer to the text mesh component
	TextMesh textMesh;
	GameObject textObject;
	TextMesh textIndexMesh;
	GameObject textIndexObject;

	Vector2 touchStart = new Vector2(-1,-1);
	Vector2 touchStop = new Vector2(-1,-1);
	int lastTouchRotations = 0;
	

	// we need to inialize objects before Start()
	void Awake() {

		// the object we use to spin the phylactere around
		spinner.gameObject = transform.Find("Spinner").gameObject;
		spinner.startTime = Time.time;

		// set text mesh color to the background color
		//float timeSaturation = Camera.main.GetComponent<Daylight>().getTimeSaturation();
		//Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);

		// the text we write into
		textObject = spinner.gameObject.transform.Find("Speech").gameObject;
		textMesh = textObject.GetComponent<TextMesh>();
		textMesh.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//textMesh.color = c;

		// when there are multiple-choice possibilities, we display the current index
		textIndexObject = spinner.gameObject.transform.Find("Index").gameObject;
		textIndexMesh = textIndexObject.GetComponent<TextMesh>();
		textIndexMesh.color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//textIndexMesh.color = c;
		
		// the cadre of the text
		speech.frame = spinner.gameObject.transform.Find("Bulle").Find("PlaneScaler").gameObject;

		// start by pointing directly at the camera (so as to avoid opening jerky movements)
		LookAtCamera();


	}

	// Use this for initialization
	void Start () {
	
	}


	void OnApplicationQuit() {
		
		if (mat != null) DestroyImmediate(mat);

		speech.shouldReply = false;
		// get rid of phylactere
		DestroyImmediate(gameObject);

	}


	void OnDestroy() {

		// if we need to reply to this phrase
		// tell the parent gameObject that we've finished Speaking
		if (speech.shouldReply) FinishedSpeaking();
		
		if (mat != null) DestroyImmediate(mat);

	}


	public void AbortReply() {

		speech.shouldReply = false;

	}


	////


	void StartChoice() {

		float deltaTime = Time.time - (spinner.startTime);
		deltaTime += 10.0f;
		float sineTime = Mathf.Abs(Mathf.Sin(deltaTime * 0.1f));
		sineTime *= 4.0f;
		sineTime += 1.0f;

		Invoke("SwitchChoice", sineTime);

	}


	void PauseChoice() {

		CancelInvoke("SwitchChoice");

	}


	void ResumeChoice() {

		// give some time to click
		Invoke("SwitchChoice", 30.0f);

	}


	void SwitchChoice() {

		// loop through to the next text
		spinner.index = (spinner.index+1) % speech.texts.Count;
		// if we've just looped back to 0
		if (spinner.index == 0) {
			spinner.currentAngle = -1 * 180;
		}

		SpeechForcePhrase(spinner.index);

		// start the timer on the next switch
		StartChoice();

	}


	////


	void StartDestroy() {

		// calculate using line length
		int lineCount = speech.text.Split('\n').Length;
		float duration = 1.0f + (4.0f*lineCount);

		Invoke("DestroyPhylactere", duration);

	}


	void PauseDestroy() {

		CancelInvoke("DestroyPhylactere");

	}


	void ResumeDestroy() {

		StartDestroy();

	}


	void DestroyPhylactere() {

		Destroy(gameObject);

	}



	////////////////////////
	


	void Update () {

		LookAtCamera();
		SpinPhylactere();
		
		// if we still need to write out speech 
		if (speech.charIndex < speech.text.Length) SpeakNextCharacter();

	}


	// Turn towards camera permanently


	void LookAtCamera() {

		// look at the camera
		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0,180,0);

	}


	void SpinPhylactere() {

		// loop around from negative to highest value positive
		if (spinner.index < 0) {
			spinner.currentAngle = speech.texts.Count * 180;
			spinner.index = speech.texts.Count + spinner.index;
		}

		// loop around from highest value positive to zero
		if (spinner.index >= speech.texts.Count) {
			spinner.currentAngle = -1 * 180;
			spinner.index = spinner.index - speech.texts.Count;
		}

		spinner.targetAngle = spinner.index * 180;

		if (Mathf.Abs(spinner.currentAngle-spinner.targetAngle) < 10.0f) spinner.currentAngle = spinner.targetAngle;
		else spinner.currentAngle = Mathf.Lerp(spinner.currentAngle, spinner.targetAngle, 0.1f);

		// calculate actual rotation
		float rotationY = spinner.currentAngle;
		while(rotationY < 0) rotationY += 360.0f;
		rotationY = (float)((int)rotationY % 360);
		spinner.gameObject.transform.localRotation = Quaternion.Euler(0,rotationY,0);

		// if the text box has spun around
		if ((rotationY >= 90 && rotationY < 270)) {
			// flip around text
			textObject.transform.localRotation = Quaternion.Euler(0,180.0f,0);
			textObject.transform.localPosition = new Vector3(1.35f, 0.4f, 0.0f);
			textIndexObject.transform.localRotation = Quaternion.Euler(0,180.0f,0);
			// if the last time we weren't in this rotation
			if (spinner.lastRotation != 180) TextDisplayDidRotate();
			spinner.lastRotation = 180;
		} else {
			textObject.transform.localRotation = Quaternion.identity;
			textObject.transform.localPosition = new Vector3(-1.35f, 0.4f, 0.0f);
			textIndexObject.transform.localRotation = Quaternion.identity;
			// if the last time we weren't in this rotation
			if (spinner.lastRotation != 0) TextDisplayDidRotate();
			spinner.lastRotation = 0;
		}

	}


	void TextDisplayDidRotate() {

		// if we're already at this text
		if (spinner.index == speech.index) return;

		// show that text
		SpeechForcePhrase(spinner.index);

	}


	// Speech Engine


	// complex version: multiple-choice phrases to Speak

	public void Speak(List<string> phrases) {

		// make sure we're actually supposed to say something
		if (phrases.Count == 0) {
			DestroyImmediate(gameObject);
			return;
		}

		SetColor();

		// if there's only one phrase
		if (phrases.Count == 1) {
			// there is only one phrase to add
			speech.texts.Add(phrases[0]);
			// just use that phrase
			SpeechSetPhrase(0);
			// we destroy the phylactere when time's up
			StartDestroy();
			// forget the rest
			return;
		}

		// otherwise, add all the phrases to our list
		foreach(string phrase in phrases) {
			speech.texts.Add(phrase);
		}

		// start with phrase #0
		SpeechForcePhrase(0);

		StartChoice();

		// whip through all the phrases
		spinner.currentAngle = speech.texts.Count * 180;

	}


	void SpeechSetPhrase(int index) {

		speech.text = speech.texts[index];
		speech.index = index;
		speech.charIndex = 0;

		ResizeFrame();

	}


	void SpeechForcePhrase(int index) {

		speech.text = speech.texts[index];
		speech.index = index;

		JumpToEndOfText();

	}


	void SetColor() {

		// create the color material for this phylactère
		mat = new Material(Shader.Find("Self-Illumin/Diffuse"));
		// get the color of the Persona
		mat.color = GetParentColor();

		spinner.gameObject.transform.Find("Bulle").Find("Triangle").gameObject.renderer.material = mat;
		spinner.gameObject.transform.Find("Bulle").Find("PlaneScaler").Find("Rectangle").gameObject.renderer.material = mat;
		spinner.gameObject.transform.Find("Bulle").Find("PlaneScaler").Find("RectangleRev").gameObject.renderer.material = mat;
		
	}


	Color GetParentColor() {

		Material m = transform.parent.GetComponentInChildren<Renderer>().material;
		return m.color;

	}


	void SpeakNextCharacter() {

		// count down
		speech.charTimer -= Time.deltaTime;
		if (speech.charTimer > 0) return;

		// reset charTimer for next time
		speech.charTimer = speechDelay;
		// increment index
		speech.charIndex++;
		// write out text
		//textMesh.text = ParseString(speech.text.Substring(0,speech.charIndex));
		textMesh.text = ParseStringWithSpaces(speech.text.Substring(0,speech.charIndex));
		// resize since we've drawn text
		ResizeFrame();

		// are we at the end of the speech?
		//if (speech.charIndex >= speech.text.Length) FinishedSpeaking(speech.text);

	}


	public void ClickAccelerate() {

		// are we at the end of the text?
		if (speech.charIndex >= speech.text.Length-1) Destroy(gameObject);
		else JumpToEndOfText();

	}


	void JumpToEndOfText() {

		// just to the end of the phrase
		speech.charIndex = speech.text.Length;
		//
		//textMesh.text = ParseString(speech.text);
		textMesh.text = ParseStringWithSpaces(speech.text);

		// if there are multiple choice possibilities
		if (speech.texts.Count > 1) {
			// display
			textIndexMesh.text = "" + (speech.index+1);
		}
		//
		ResizeFrame();

	}


	void FinishedSpeaking() {

		// make sure the parent (the Persona) is there
		if (transform.parent == null) return;
		// if this is a multiple choice
		//if (speech.texts.Count > 1) transform.parent.GetComponent<Talk>().FinishedSpeaking(speech.index);
		// tell the parent object we've finished Speaking
		//else transform.parent.GetComponent<Talk>().FinishedSpeaking();

		// tell the parent object which phrase we chose
		transform.parent.GetComponent<Talk>().FinishedSpeaking(speech.text);

	}


	string ParseString(string inputString) {

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
					//finalString += Environment.NewLine;
					finalString += '\n';
				}
				// start counting the next line at zero
				lineLength = 0;
			}

		}

		return finalString;

	}


	string ParseStringWithSpaces(string inputString) {

		string finalString = "";
		int lineLength = 0;
		int lineMax = 18;

		string[] words = inputString.Split(' ');
		foreach (string word in words) {
		    
			// if this word's length is over max
			if (word.Length >= lineMax) {
				// put it on it's own line
				finalString += '\n';
				finalString += word;
				// set us to the end of that line
				lineLength = lineMax;
				// next word
				continue;
			}

			// if we add this word's length, does it go over the line length?
			if (lineLength + word.Length + 1 > lineMax) {
				// move on to next line
				finalString += '\n';
				finalString += word;
				// move the length cursor to the end of that word
				lineLength = word.Length;
				// next word
				continue;
			}

			// otherwise, just add the word (with a space)

			// if we're not at the beginning of the line
			if (lineLength > 0) {
				// add a space
				finalString += ' ';
				lineLength += 1;
			}
			// now add the word
			finalString += word;
			lineLength += word.Length;

		}

		return finalString;

	}


	void ResizeFrame() {

		int lineCount = textMesh.text.Split('\n').Length;

		float scaleY = 0.01f * (29 + (29 * lineCount));
		
		Vector3 newScale = speech.frame.transform.localScale;
		newScale.y = scaleY;
		speech.frame.transform.localScale = newScale;

	}


	// Click/Touch

	public void TouchDown(Vector2 touchPoint, Vector3 hitPoint) {

		JumpToEndOfText();

		touchStart = touchPoint;
		touchStop = touchPoint;
		lastTouchRotations = 0;

		if (speech.texts.Count == 1) PauseDestroy();
		else PauseChoice();

	}


	public void TouchMoved(Vector2 touchPoint) {

		// delta
		//Vector2 delta = touchPoint - touchStop;
		// update to new position
		touchStop = touchPoint;

		// calculate delta
		float xDelta = (float)(touchStop.x - touchStart.x);

		if (!spinner.spinning && Mathf.Abs(xDelta) > 30) {
			spinner.spinning = true;
		}

		// calculate positive/negative offset

		int rotations = 0;

		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			rotations = (int)(xDelta / (Screen.width/10));
		} else {
			rotations = (int)(xDelta / (Screen.width/15));
		}

		// if there is only one text, no need to calculate rotations
		if (speech.texts.Count == 1) return;

		// if we've changed direction
		if (lastTouchRotations != rotations) {
			// figure out if we add or subtract
			int deltaIndex = rotations - lastTouchRotations;
			// remember for next time
			lastTouchRotations = rotations;
			// add to index
			spinner.index += deltaIndex;
		}

	}


	public void TouchUp(Vector2 touchPoint) {

		// delta
		//Vector2 delta = touchPoint - touchStop;
		// update to new position
		touchStop = touchPoint;

		// if we're spinning, then we haven't selected this one yet
		if (spinner.spinning) {

			// ok, we're done spinning, the next TouchUp should select this phylactere
		 	spinner.spinning = false;
		 	// if there's are multiple choices, go back to spinning the choices
			if (speech.texts.Count > 1) ResumeChoice();
		 	// if there aren't multiple choices, go back to delay
			if (speech.texts.Count == 1) ResumeDestroy();

		} else { // no we're not spinning

			// this phylactere is done, advance to next dialog point
			DestroyPhylactere();

		}


	}

}
