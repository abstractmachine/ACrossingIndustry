using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List<>

public class Cheat : MonoBehaviour {

	int index = 0;
	bool on = false;
	public bool IsOn{ get { return on; } }

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
		on = false;
	}

	public void TurnOn() {
		on = true;
	}

	public void ResetIndex() {
		index = 0;
	}

}
