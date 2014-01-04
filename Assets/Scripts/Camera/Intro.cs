using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {
	

	bool shouldGo = false;
	float timeout = 0.2f;

	string message = "<b><color=#FFFFFF>A Crossing Industry</color></b> is currently <i><color=#444444>in development</color></i>.\nThis <b><color=#444444>private</color></b> demonstration explores navigation and basic\ninteraction with other characters. Future demonstrations\nwill explore actions, objects and more complex character\ninteractions.\n\n";
	string click = "<i>> click to continue</i>";

	void Update () {
	
		if (shouldGo) {

			timeout -= Time.deltaTime;
			if (timeout <= 0.0f) Go();
			return;

		} else {

			int percentageLoaded = (int)(Application.GetStreamProgressForLevel("Game") * 100.0f);

			if (percentageLoaded >= 100) {
		 		gameObject.GetComponent<GUIText>().text = message + click;
			} else {
		 		gameObject.GetComponent<GUIText>().text = message + "Loading: " + percentageLoaded.ToString() + "%";
			}

		}

		 if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
		 	if (Application.CanStreamedLevelBeLoaded("Game")) shouldGo = true;
		 }

		 if (Input.GetMouseButtonDown(0)) {
		 	if (Application.CanStreamedLevelBeLoaded("Game")) shouldGo = true;
		 }

		 if (shouldGo) {
		 	gameObject.GetComponent<GUIText>().text = "";
		 }

	}


	void Go() {

		Application.LoadLevel("Game");

	}


}
