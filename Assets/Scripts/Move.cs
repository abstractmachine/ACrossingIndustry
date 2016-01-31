using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	float speed = 0.2f;

	void Update() {

		UpdateJoystick();
		UpdateKeys();

	}

	void UpdateJoystick() {
         
	}

	void UpdateKeys() {

		if (Input.GetKey(KeyCode.LeftArrow)) {
			transform.Translate(0, 0, speed);
		}
		if (Input.GetKey(KeyCode.RightArrow)) {
			transform.Translate(0, 0, -speed);
		}      
		if (Input.GetKey(KeyCode.UpArrow)) {
			transform.Translate(speed, 0, 0);
		} 
		if (Input.GetKey(KeyCode.DownArrow)) {
			transform.Translate(-speed, 0, 0);
		}

	}
}
