using UnityEngine;
using System.Collections;

public class OpenClose : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		
		if (other.tag == "Player") {
			transform.parent.animation.Play("open");
		}

	}

	void OnTriggerExit(Collider other){

		if (other.tag == "Player") {
			transform.parent.animation.Play("close");
		}

	}

}
