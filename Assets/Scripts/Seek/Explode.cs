using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		transform.localScale = transform.localScale + new Vector3(0.05f, 0.05f, 0.05f);

		if (transform.localScale.x > 5.0f) Destroy(gameObject, 0.01f);

	}
}
