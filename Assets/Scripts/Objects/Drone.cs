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

		 if (transform.localPosition.x > 300) {

		 	transform.Translate(Vector3.up * 1000);

		 }

	}


	void OnTriggerEnter(Collider other){
		if(LayerMask.LayerToName(other.gameObject.layer) != "Persona") return;
		Vector3 loc = other.transform.position;

		print("Layer : "+ LayerMask.LayerToName(other.gameObject.layer) + "\tgameObject (" + other.name + ") detected at: " + loc);

	}
 	

}
