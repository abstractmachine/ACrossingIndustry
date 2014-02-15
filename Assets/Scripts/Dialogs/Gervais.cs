using UnityEngine;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic; // Dictionary,List

public class Gervais : MonoBehaviour {

	TextMesh parentMesh;
	TextMesh gervaisMesh;

	string parentText = "";

	//List<string> masks = new List<string> { " ", "#" };
	//int maskLevel = 0;

	void Start() {

		gervaisMesh = GetComponent<TextMesh>();
		parentMesh = transform.parent.gameObject.GetComponent<TextMesh>();

		gervaisMesh.text = "";

		gervaisMesh.color = parentMesh.color;

	}


	void Update() {

		// if the parent text changed it's contents, do something
		if (parentMesh.text != parentText) ParentTextDidChange();

	}


	void ParentTextDidChange() {

		parentText = parentMesh.text;

		// if it's an empty text
		if (parentMesh.text == "") {
			EmptyText();
			return;
		}

		// otherwise, overlay something
		//Obscurify();

	}


	void EmptyText() {

		gervaisMesh.text = "";

	}


	void Obscurify() {

		string newText = gervaisMesh.text;

		//maskLevel = (int)Random.Range(1,masks.Count);
		//bool maskOn = false;

		for(int i=gervaisMesh.text.Length; i<parentText.Length; i++) {

			char thisChar = parentText[i];
/*
			// if we're at first, or a space/break
			if (i==0 || thisChar==' ' || thisChar=='\n') {
				// choose new maskOn state
				if (Random.Range(0,10) < 5) maskOn = true;
				else maskOn = false;
			} // if (i==0
*/

			switch(thisChar) {

				case ' ' :
					newText += " ";
					break;
				case '\n':
					newText += "\n";
					break;
				default  :
					newText += "#";
					//if (!maskOn) newText += " ";
					//else newText += masks[maskLevel];
					break;

			} // switch(thisChar
			
		} // for(int i

		gervaisMesh.text += newText;

	}


	void Blinkify(TextMesh textMesh) {

	}

}
