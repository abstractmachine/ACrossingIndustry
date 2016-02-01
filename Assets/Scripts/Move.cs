using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	public Vector3 goal;
	NavMeshAgent agent;

	void Start() {

		agent = GetComponent<NavMeshAgent>();

	}


	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag == "Persona") {
			StopWalking();
		}

	}

	void StopWalking() {

		GoToPosition(transform.position);

	}

	public void GoToPosition(Vector3 newPosition) {

		goal = newPosition;
		agent.destination = goal; 

	}

}
