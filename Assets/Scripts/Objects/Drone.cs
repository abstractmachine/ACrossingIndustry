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

		 if (transform.localPosition.x > 200) {

		 	transform.Translate(Vector3.up * 500);

		 }

	}


	void OnTriggerEnter(Collider other){
	
		Vector3 loc = other.transform.position;

		print("Persona (" + other.name + ") detected at: " + loc);

	}
 	

}
