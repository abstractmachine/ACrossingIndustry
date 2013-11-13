using UnityEngine;
using System.Collections;

public class TextDialog : MonoBehaviour {

	public GameObject phylactere;

	int index = 0;

	Color materialColor;

	// Use this for initialization
	void Start () {

		materialColor = transform.Find("Persona").renderer.material.color;
	
	}

	
	// Update is called once per frame
	void Update () {
	


	}


	public void startDialog() {

		index = 0;

		// remove last char 
		string message = gameObject.GetComponent<TextTree>().getString(index);

		if (message != "") speak(message, 0);

	}


	public void replyTo(GameObject other, int newIndex) {

		index = newIndex + 1;

		// remove last char 
		string message = gameObject.GetComponent<TextTree>().getString(index);

		if (message != "") speak(message, index);

	}


	public void speak(string message, int newIndex) {

		index = newIndex;

		// instantiate new GameObject from prefab
		GameObject obj = (GameObject)Instantiate(phylactere);
		// attach this object to us
		obj.transform.parent = transform;
		// position the phylactere object
		obj.transform.localPosition = new Vector3(0, 20, 0);
		// color the phylactere object
		obj.GetComponent<TextColor>().setColor(materialColor);
		// get this phylactere object it's text
		obj.GetComponent<TextSpeaker>().speak(message, index);
		
	}


}
