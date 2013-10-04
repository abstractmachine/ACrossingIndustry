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

			GameObject.FindWithTag("Player").GetComponent<Walking>().setTargetPosition(hitLocation);

		}

	}

}
