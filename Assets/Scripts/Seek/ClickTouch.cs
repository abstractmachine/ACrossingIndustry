using UnityEngine;
using System.Collections;

public class ClickTouch : MonoBehaviour {

	public GameObject exploder;
	public GameObject xSpot;

	Walking walking;

	// Use this for initialization
	void Start () {

		walking = GameObject.FindWithTag("Player").GetComponent<Walking>();

	}
	
	// Update is called once per frame
	void Update () {
		
		// click to change position
		if (Input.GetMouseButtonDown(0)) {
			checkHit(Input.mousePosition);
		}

		// touch screen to change position
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
			checkHit(Input.GetTouch(0).position);
		}

		// TODO: add double-click for run-to-position

	}


	void checkHit(Vector2 loc) {

		//RaycastHit hit;

		Ray ray = Camera.main.ScreenPointToRay(loc);
		// show in debugger
		//Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
		
		RaycastHit[] hits = Physics.RaycastAll(ray);
		//Loop through all overlapping objects and disable their mesh renderer
        if(hits.Length == 0) return;

        foreach(RaycastHit hit in hits) {

        	// check to see if this is another Persona that we're perhaps already talking to
        	if (hit.transform.gameObject.tag == "Persona") {
        		// see if we're already talking to them
        		if (walking.isCollidingWith(hit.transform.gameObject)) {
        			// abort click/touch
        			return;
        		}
        	}

        	// ignore ground click/touch
        	if (hit.transform.name != "Ground") continue;

			// FIXME: the raycast should only work on the ground
			//if (Physics.Raycast(ray, out hit)) {  

			// get the point on the plane where we clicked
			Vector3 hitLocation = hit.point;
			// just in case
			hitLocation.y = 0;
			// kill all other clicks
			GameObject[] otherClicks;
        	otherClicks = GameObject.FindGameObjectsWithTag("ClickTouch");
        	foreach (GameObject obj in otherClicks) {
        		Destroy(obj);
        	}
			// montrer où on a cliqué
			GameObject clicker = Instantiate(exploder, hitLocation, Quaternion.identity) as GameObject;
			clicker.name = "Clicker";

			// tell the player where to start walking
			walking.setTargetPosition(hitLocation);

		}

	}

}
