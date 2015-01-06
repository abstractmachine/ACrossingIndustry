using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Phylactere : MonoBehaviour {
	
	public float TimePerLine = 2.0f;
	public float speechDelay = 0.01f;
	public float rotationSpeed = 1.2f;	
	public float minimumWidth = 254f;
	public float minimumHeight = 131f;
	public float marginWidth = 100f;
	public float marginHeight = 75f;
	public float indexSize = 33f;

	Transform spinner;
	GameObject textObject;
	Text text;
	GameObject indexObject;
	Text indexText;
	Animator anim;
	GameObject UglyClickZone;
	bool isTouching = false;
	
	
	class Speech {	
		public List<string> texts = new List<string>();	// the text alternatives
		public int index = 0;								// which text are we displaying
		public string text = "";							// the actual text content
		public int charIndex = 0;							// the current index of the character
		public float charTimer = 0;						// the time remaining before next character
		public bool shouldReply = true; 					// this can be de-activated when conversations end
	}

	Speech speech = new Speech();

	void Awake () {
		// Set the camera for CanvasUI
		transform.gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
		// Set the transforms
		transform.rotation = Camera.main.transform.rotation;
		transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		// A pointer to the spinning ImageUI which contains everything
		spinner = transform.Find ("Spinner");
		// texts pointers
		textObject = spinner.Find("Text").gameObject;
		text = textObject.GetComponent<Text>();
		indexObject = spinner.Find ("Arrow").Find ("Index").gameObject;
		indexText = indexObject.GetComponent<Text>();
		// ...
		UglyClickZone = transform.Find ("UglyClickZone").gameObject;
/*		// A better approach might be using a button component on the object
		// but if spinning, can't be touched …
		Button b = transform.GetComponent<Button>();
		b.onClick.AddListener(() => ButtonClicked());	
		// And farther : 
		void ButtonClicked(){doStuff…}	*/
		// A pointer to the animator which triggers the rotation when we've got multiple choices
		anim = transform.GetComponent<Animator>();
		anim.speed = rotationSpeed;
	}

	void Start () {
	
	}

	void Update () {
		// The text isn't fully written?
		if (speech.charIndex < speech.text.Length) SpeakNextCharacter();
		
		// Resize the frame at update since the ContentSizeFitter doesn't seem to always fire before our code does.
		ResizeFrame(); 
		
	}
	
	void OnApplicationQuit() {
		
		speech.shouldReply = false;
		// get rid of phylactere
		DestroyImmediate(transform.gameObject);
		
	}
		
	void OnDestroy() {
		
		// if we need to reply to this phrase
		// tell the parent gameObject that we've finished Speaking
		if (speech.shouldReply) FinishedSpeaking();
				
	}


	public void AbortReply() {

			speech.shouldReply = false;

		}

	public void Speak(List<string> phrases) {
		if (phrases.Count == 0) {
			DestroyImmediate(gameObject);
			return;
		}
		
		SetColor ();
		
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
				
	}

	public void ClickAccelerate(){
		// are we at the end of the text?
		if (speech.charIndex >= speech.text.Length-1) Destroy(gameObject);
		
		else JumpToEndOfText();

	}


	void SetColor() {

		spinner.GetComponent<Image>().color = GetParentColor();
		spinner.Find ("Arrow").GetComponent<Image>().color = GetParentColor();

	}
		
	Color GetParentColor() {
		
		Material m = transform.parent.GetComponentInChildren<Renderer>().material;
		return m.color;
		
	}
	//TODO :make a better object structure (probably without any code thanks to Layout Components).
	void ResizeFrame() {
		// No text inside?
		if(text.text == "Empty Text" || text.text == "") return;
		// First get the size of our text object
		Vector2 textSize = textObject.GetComponent<RectTransform>().sizeDelta;
		// Add margins
		textSize.x += marginWidth;
		textSize.y += marginHeight;
		// Fit to minimum size?
		if(textSize.x <= minimumWidth) textSize.x = minimumWidth; // Default 254
		if(textSize.y <= minimumHeight) textSize.y = minimumHeight; // Default 131
				
		transform.GetComponent<RectTransform>().sizeDelta = textSize;
		// Rescale the click zone to fit to the frame size (height/8 to get the triangle height too)
		UglyClickZone.transform.localScale = new Vector3(textSize.x/10, 0, textSize.y/8);
		
		
	}

	void SpinPhylactere(){

		if(speech.texts.Count < 2) return;

		// The id for our Mecanim trigger
		int hash = Animator.StringToHash("rotate");
		// Let spin this phylactere
		anim.SetTrigger(hash);

	}


	void SpeechSetPhrase(int index) {
		
		speech.text = speech.texts[index];
		speech.index = index;
		speech.charIndex = 0;
	
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
		text.text = ParseStringWithSpaces(speech.text.Substring(0,speech.charIndex));
		
	}
	
	void SpeechForcePhrase(int index) {

		speech.text = speech.texts[index];
		speech.index = index;
		
		JumpToEndOfText();
		
	}

	void JumpToEndOfText() {

		// just to the end of the phrase
		speech.charIndex = speech.text.Length;
		// Be sure we're not calling from the touch events
		if(! isTouching) 	SpinPhylactere();
		
		text.text = ParseStringWithSpaces(speech.text.Substring(0, speech.charIndex));
		
		// if there are multiple choice possibilities
		if (speech.texts.Count > 1) {
			// display
			indexText.text = "<size="+indexSize+">"+(speech.index+1)+"</size>\n\n"+speech.texts.Count.ToString ();
		}
		
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
	
	void FinishedSpeaking() {
		
		// make sure the parent (the Persona) is there
		if (transform.parent == null) return;
		
		// tell the parent object which phrase we chose
		transform.parent.GetComponent<Talk>().FinishedSpeaking(speech.text);
		
	}


	void StartChoice() {
		
		int lineCount = speech.text.Split('\n').Length;
		float duration = (1/rotationSpeed) + (TimePerLine*lineCount);

		Invoke("SwitchChoice", duration);
		
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
		int index = (speech.index+1) % speech.texts.Count;
		
		indexText.text = "<size="+indexSize+">"+index.ToString()+"</size>\n\n"+speech.texts.Count.ToString ();
		
		SpeechForcePhrase(index);
		
		// start the timer on the next switch
		StartChoice();
		
	}

	
	void StartDestroy() {
		
		// calculate using line length
		int lineCount = speech.text.Split('\n').Length;
		float duration = (1/rotationSpeed) + (TimePerLine*lineCount);
		
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
	
	// Click/Touch
	public void TouchDown(){ 
		
		isTouching = true;
		JumpToEndOfText();
		
		if (speech.texts.Count == 1) PauseDestroy();
		else PauseChoice();
		
	}
	//Not Implemented
	public void TouchMoved(Vector2 touchPoint) {
		
	/*	// delta
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
		}*/
		
	}
	
	public void TouchUp() {
		
		 // if we're spinning, then we haven't selected this one yet
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("spinning") ) {
			
			// if there's are multiple choices, go back to spinning the choices
			if (speech.texts.Count > 1) ResumeChoice();
			// if there aren't multiple choices, go back to delay
			if (speech.texts.Count == 1) ResumeDestroy();
			
		} else { // no we're not spinning
			
			// this phylactere is done, advance to next dialog point
			DestroyPhylactere();
			
		}
		isTouching = false;
		
	}

	// Animation functions
	public void Invert(){
	
		textObject.transform.localRotation = Quaternion.Euler(0,180,0);
	
	}
	
	public void Revert(){
	
		textObject.transform.localRotation = Quaternion.Euler(0,0,0);
	
	}
	
}
