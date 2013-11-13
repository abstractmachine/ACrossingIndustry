using UnityEngine;
using System.Collections;
using System.Collections.Generic; // for the List<>

public class TextTree : MonoBehaviour {

	public List<string> phrases = new List<string>();

	public string getString(int index) {
		
		if (index >= phrases.Count) return "";

		return phrases[index];

	}

}
