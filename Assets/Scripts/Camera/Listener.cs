using UnityEngine;
using System.Collections;

public class Listener : MonoBehaviour {

	public GameObject target;

	
	void Start () {
	
	}
	
	// Suivre le target de vue pour mettre des oreilles sur la caméra
	
	void Update () {

		transform.position = target.transform.position;
		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0.0f,180.0f,0.0f);

	}
}
