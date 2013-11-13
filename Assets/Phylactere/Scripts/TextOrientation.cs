using UnityEngine;
using System.Collections;

public class TextOrientation : MonoBehaviour {
	
	// Use this for initialization
	void Start () {

		
	}
	
	// Update is called once per frame
	void Update () {

		// look at the camera
		transform.LookAt(Camera.main.transform, Vector3.up);
		transform.Rotate(0,180,0);

	}
}
