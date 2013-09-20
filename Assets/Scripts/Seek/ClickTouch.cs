using UnityEngine;
using System.Collections;

public class ClickTouch : MonoBehaviour {

	public GameObject exploder;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		// click to change position
		if (Input.GetMouseButtonDown(0)) {
			checkHit(Input.mousePosition);
		}

		// TODO: add double-click for run-to-position

	}


	void checkHit(Vector2 loc) {

		RaycastHit hit;

		Ray ray = Camera.main.ScreenPointToRay(loc);
		// show in debugger
		Debug.DrawRay (ray.origin, ray.direction * 10, Color.yellow);

		if (Physics.Raycast (ray, out hit)) {  
			// get the point on the plane where we clicked
			Vector3 hitLocation = hit.point;
			// we're only interested in the y coordinate
			hitLocation.y = 0.0f;

			Instantiate(exploder, hitLocation, Quaternion.identity);

			GameObject.FindWithTag("Player").GetComponent<Walking>().setTargetPosition(new Vector2(hitLocation.x, hitLocation.z));

		}
	}

}
