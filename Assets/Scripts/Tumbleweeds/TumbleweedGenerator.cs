using UnityEngine;
using System.Collections;

public class TumbleweedGenerator : MonoBehaviour {

	public GameObject tumbleweedPrefab;

	float startTimer = 1.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		// we need a stupid-ass timer because it takes a few frames for renderer to wake up
		if (startTimer > 0.0f) {
			startTimer -= Time.deltaTime;
			return;
		}

		// no children, and we're not visible?
		if (0 == transform.childCount && !renderer.isVisible) {
			// great! Create a new tumbleweed
			GameObject obj = Instantiate(tumbleweedPrefab, transform.position, Quaternion.identity) as GameObject;
			// place object as parent
			obj.transform.parent = transform;
		}

	}
}
