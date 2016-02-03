﻿using UnityEngine;
using System.Collections;

public class Persona : MonoBehaviour {

	GameObject currentPlayer = null;

	void OnMouseDown() {

		// if we're still intersecting with another person
		if (currentPlayer != null) {

			Player playerDialogue = currentPlayer.GetComponent<Player>();

			if (playerDialogue != null) {
				playerDialogue.Click();
			}

		} else { // currentPlayer == null

			// get the ground object
			GameObject ground = GameObject.FindGameObjectWithTag("Ground");
			// tell the player to come here
			ground.GetComponent<Ground>().TouchedObject(this.gameObject);

		}

	}


	void OnTriggerEnter(Collider other) {

		// only register intersections with the player
		if (other.gameObject.tag != "Player") {
			return;
		}

		// make sure we're not already talking to this player
		if (currentPlayer == other.gameObject) {
			return;
		}

		// make sure we're not already talking with someone else
		if (currentPlayer != null) {
			return;
		}

		// ok, register this as valid other
		currentPlayer = other.gameObject;

	}


	void OnTriggerExit(Collider other) {

		// only register intersections with the player
		if (other.gameObject.tag != "Player") {
			return;
		}

		// make sure this is the actual person we were interacting with
		if (other.gameObject == currentPlayer) {
			currentPlayer = null;
		}
      
	}
 

}
