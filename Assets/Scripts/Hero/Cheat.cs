using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List<>

public class Cheat : MonoBehaviour {

        GameObject coordinates;
        GameObject coordinatesTextObject;
        TextMesh coordinatesMesh;

        Vector3 targetPosition = new Vector3(-1000.0f,0.0f,-1000.0f);

	int index = 0;

	bool on = false;
	public bool IsOn{ get { return on; } }

        void Awake() {
                coordinates = GameObject.Find("Coordinates");
                coordinatesTextObject = coordinates.transform.Find("Text").gameObject;
                coordinatesMesh = coordinatesTextObject.GetComponent<TextMesh>();
        }

	public void KeyDown() {

                // otherwise check to see if we match keycode pattern

                switch(index) {

                	case 0:
                	case 1:
                		if (Input.GetKeyDown(KeyCode.UpArrow)) index++;
                		else ResetIndex();
                		break;

                	case 2:
                	case 3:
                		if (index == 2 && Input.GetKeyDown(KeyCode.UpArrow)) index = 2;
                		else if (Input.GetKeyDown(KeyCode.UpArrow)) index = 1;
                		else if (Input.GetKeyDown(KeyCode.DownArrow)) index++;
                		else ResetIndex();
                		break;

                	case 4:
                	case 6:
                		if (Input.GetKeyDown(KeyCode.UpArrow)) index = 1;
                		else if (Input.GetKeyDown(KeyCode.LeftArrow)) index++;
                		else ResetIndex();
                		break;

                	case 5:
                		if (Input.GetKeyDown(KeyCode.UpArrow)) index = 1;
                		else if (Input.GetKeyDown(KeyCode.RightArrow)) index++;
                		else ResetIndex();
                		break;

                	case 7:
                		if (Input.GetKeyDown(KeyCode.UpArrow)) index = 1;
                		else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                			index++;
                			TurnOn();
                		} else {
                			ResetIndex();
                		}
                		break;

                	default:
                		if (Input.GetKeyDown(KeyCode.UpArrow)) {
                			TurnOff();
                			index = 1;
                		} else {
                			TurnOff();
                			ResetIndex();
                		}
                		break;
                	
                }

	}

	public void TurnOff() {
                hideCoordinates();
		on = false;
	}

	public void TurnOn() {
                showCoordinates();
		on = true;
	}

	public void ResetIndex() {
		index = 0;
	}

        public void showCoordinates() {
                coordinatesTextObject.renderer.enabled = true;
        }

        public void hideCoordinates() {
                coordinatesTextObject.renderer.enabled = false;
        }

        public void setCoordinates(Vector3 newPosition) {

                targetPosition = newPosition;
                coordinates.transform.position = new Vector3(targetPosition.x, 0, targetPosition.z);
                coordinatesMesh.text = targetPosition.x.ToString("0.00") + " , " + targetPosition.z.ToString("0.00");

        }

}
