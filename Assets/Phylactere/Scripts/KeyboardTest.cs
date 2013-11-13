using UnityEngine;
using System.Collections;

public class KeyboardTest : MonoBehaviour {

	//string buffer = "";

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyDown("@") || Input.GetKeyDown(KeyCode.Space)) {

			gameObject.GetComponent<TextDialog>().startDialog();

		}

		// did we type something?
		/*if (Input.GetKeyDown(KeyCode.Return)) {

			// get the input string
			string s = Input.inputString;
			char c = '\0';
			// if there is a first character, get it (it could be an empty control character)
			if (s.Length > 0) c = s[0];

			// is it the return key?
			if (Input.GetKeyDown(KeyCode.Return)) {

				// if yes, speak that text
				gameObject.GetComponent<TextDialog>().speak(buffer);
				// empty the buffer
				buffer = "";

			} else if (Input.inputString.Length > 0 && (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSeparator(c))) { // otherwise, check to see if it's a valid character
				// add to the buffer
				buffer += Input.inputString;

			}

		}*/

	}

}
