using UnityEngine;
using System.Collections;

public class Drone : MonoBehaviour {

	public float speed = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		 transform.Translate(-Vector3.up * Time.deltaTime * speed);

		 if (Vector3.Distance(Vector3.zero,transform.localPosition) > 1000) {

		 	transform.Translate(new Vector3(0,1900,0), Space.Self);

		 }

	}


	void OnTriggerEnter(Collider other){

		// if not the player
		if (other.gameObject.tag != "Player") return;

		//if(LayerMask.LayerToName(other.gameObject.layer) != "Persona") return;
		Vector3 loc = other.transform.position;

		print("Drone detection!\tgameObject (" + other.name + ") detected at: " + loc);

	}
 	

}
