using UnityEngine;
using System.Collections;

public class Mur : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	void OnTriggerEnter(Collider other){
		
		//print("Persona (" + other.gameObject + ") is approaching wall (" + gameObject +") whose parent is " + transform.parent);

	}

}
