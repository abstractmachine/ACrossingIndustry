﻿using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	public GameObject targetObject;
	public Vector3 goal;
	NavMeshAgent agent;

	void Start() {

		agent = GetComponent<NavMeshAgent>();

	}


	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag == "Persona") {
			//StopWalking();
		}

	}

	void OnTriggerStay(Collider other) {
		// if we're interacting with another character
		if (other.gameObject.tag == "Persona" && other.gameObject == targetObject) {
			// get our distance to that character
			float distance = CalculateDistance(other.gameObject);
			// if too close
			if (distance < 2.5f) {
				StopWalking();
			}
		}

	}


	float CalculateDistance(GameObject other) {
		// get their position
		Vector3 personaPosition = other.transform.position;
		// annul y
		personaPosition.y = 0f;
		// get our position
		Vector3 playerPosition = this.transform.position;
		// annul y
		playerPosition.y = 0f;
		// get the distance
		return Vector3.Magnitude(playerPosition - personaPosition);      
	}

	void StopWalking() {

		GoToPosition(transform.position);

	}

	public void GoToPosition(Vector3 newPosition) {

		targetObject = null;
		goal = newPosition;
		agent.destination = goal; 

	}


	public void GoToObject(GameObject other) {

		targetObject = other;

		Vector3 position = other.transform.position;
		position.y = 0.01f;
		goal = position;
		agent.destination = goal;

	}

}
