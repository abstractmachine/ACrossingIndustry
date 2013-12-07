using UnityEngine;
using System.Collections;
using System; // for the Evironment.EndLine constant

public class Phylactere : MonoBehaviour {

	// the material used to color the phylacteres
	Material mat;

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
		textMesh = transform.Find("Speech").GetComponent<TextMesh>();
		
		// the cadre of the text
		speechFrame = transform.Find("Bulle").Find("PlaneScaler").gameObject;

	}

	// Use this for initialization
	void Start () {
	
	}


	void OnApplicationQuit() {
		
		if (mat != null) DestroyImmediate(mat);

		speechText = "";
		// get rid of phylactere
		DestroyImmediate(gameObject);

	}


	void OnDestroy() {

		// if this is the OnApplicationQuit, don't respawn a new phylactere
		if (speechText == "") return;

		// tell the parent gameObject that we've finished speaking
		finishedSpeaking();
		
		if (mat != null) DestroyImmediate(mat);

	}

	
	// Update is called once per frame
	void Update () {

		// look at the camera
		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0,180,0);
		
		// if we still need to write out speech 
		if (speechIndex < speechText.Length) speakNextCharacter();

	}


	// Speech Engine

	public void speak(string phrase, float duration) {

		// create the color material for this phylactère
		mat = new Material(Shader.Find("Self-Illumin/Diffuse"));
		// get the color of the Persona
		mat.color = getParentColor();

		transform.Find("Bulle").Find("Triangle").gameObject.renderer.material = mat;
		transform.Find("Bulle").Find("PlaneScaler").Find("Rectangle").gameObject.renderer.material = mat;
		//transform.GetComponentInChildren<TextField>().speak(phrase,duration);

		// reset line player back to 0
		speechIndex = 0;
		speechTimer = 0;
		speechText = phrase;

		resizeFrame();

		// howLong == -1 means, calculate using line length
		if (duration == -1) {
			int lineCount = speechText.Split('\n').Length;
			duration = 0.5f + (3.5f*lineCount);
		}

		// if we've specified a length
		if(duration > 0) {
			Destroy(gameObject, duration);
		}

	}


	Color getParentColor() {

		Material m = transform.parent.GetComponentInChildren<Renderer>().material;
		return m.color;

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

		// make sure the parent (the Persona) is there
		if (transform.parent == null) return;
		// tell the parent object we've finished speaking
		transform.parent.GetComponent<Dialog>().finishedSpeaking();

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
