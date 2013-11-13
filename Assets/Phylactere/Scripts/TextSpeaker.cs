using UnityEngine;
using System.Collections;
using System.Collections.Generic; // for the List<>

public class TextSpeaker : MonoBehaviour {

	// pointer to our text field
	TextField textField;

	int index = 0;

 	// whom are we speaking to?
    List<GameObject> neighbors = new List<GameObject>();

	// Use this for initialization
	void Awake () {

		// get pointer to text field
		textField = transform.Find("Speech").gameObject.GetComponent<TextField>();

	}
	

	// receive new speech, define delay (-1==line length dependant, 0==infinite, n=delay in seconds)
	public void speak(string newSpeech, int newIndex) {

		index = newIndex;
		textField.speak(newSpeech, -1);

	}


	public void finishedSpeaking() {

		// see who is around us
		foreach(GameObject neighbor in neighbors) {

			Transform neighborParent = neighbor.transform.parent;
			// if no deal
			if (neighborParent == null) continue;
			// see if we can get another speaker (i.e. intelligent conversationalist) out of this object
			GameObject otherSpeaker = neighborParent.gameObject;
			// if no deal
			if (otherSpeaker == null) continue;
			// ask for a reply from that neighbor
			TextDialog textDialog = otherSpeaker.GetComponent<TextDialog>();
			// if there is no intelligent conversationalist in our neighbor, move on to next
			if (textDialog == null) continue;
			// demand a response from the neighbor
			textDialog.replyTo(gameObject, index);

		}

	}



	// collision detection helps us to remember who our neighbors are

	void OnTriggerEnter(Collider other) {

		// make sure it's not us
		if (transform.parent == other.transform.parent) return;

		// note a new neighbor has arrived
        neighbors.Add(other.gameObject);
    
    }


    void OnTriggerExit(Collider other){
    	
    	// remove from our list of neighbors
    	foreach(GameObject neighbor in neighbors) {
    		// not them?
    		if (neighbor != other.gameObject) continue;
    		// found them, remove from list
    		neighbors.Remove(neighbor);
    		// leave foreach
    		break;
    	} // foreach

    }

}
