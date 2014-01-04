using UnityEngine;
using System.Collections;

public class ClickTouch : MonoBehaviour {

	public GameObject exploder;

	Walk walk;
	GameObject playerObject;
	Player playerScript;

	public float playerRadius = 1.0f;
	public float tumbleweedRadius = 1.0f;

	// if we're touching a phylactère, maintain a connection to the code component
	Phylactere phylactere = null;
	Vector3 lastTouchPosition = Vector3.zero;



	///////////////// Init

	void Start () {

		Input.simulateMouseWithTouches = false;

		playerObject = GameObject.FindWithTag("Player");
		playerScript = playerObject.GetComponent<Player>();
		walk = GameObject.FindWithTag("Player").GetComponent<Walk>();

	}
	


	///////////////// Loop

	void Update () {

		if (Application.platform == RuntimePlatform.IPhonePlayer) {

			// if there is touch activity
			if (Input.touchCount > 0) {

				if (Input.GetTouch(0).phase == TouchPhase.Began) {	// touch screen to change position
					checkHit(Input.GetTouch(0).position);
				} else if (phylactere != null && Input.GetTouch(0).phase == TouchPhase.Moved) {
					phylactere.touchMoved(Input.GetTouch(0).position);
				} else if (phylactere != null && Input.GetTouch(0).phase == TouchPhase.Ended) {
					phylactere.touchUp(Input.GetTouch(0).position);
					phylactere = null;
				}
 
				// tell the hero that there's been some activity
				playerScript.ResetImpatience();

			}

		} else {

			if (Input.GetMouseButtonDown(0)) {		// click to change position
				checkHit(Input.mousePosition);
				lastTouchPosition = Input.mousePosition;

				// tell the hero that there's been some activity
				playerScript.ResetImpatience();

			} else if (phylactere != null && Input.GetMouseButton(0)) { 	// if we're interacting with a phylactere
				if (Vector3.Distance(lastTouchPosition,Input.mousePosition) > 0) phylactere.touchMoved(Input.mousePosition);
				lastTouchPosition = Input.mousePosition;

				// tell the hero that there's been some activity
				playerScript.ResetImpatience();
			
			} else if (phylactere != null && Input.GetMouseButtonUp(0)) {	// if we're interacting with a phylactere
				phylactere.touchUp(Input.mousePosition);
				lastTouchPosition = Vector3.zero;
				phylactere = null;

				// tell the hero that there's been some activity
				playerScript.ResetImpatience();
				
			}

		}

		// TODO: add double-click for run-to-position
		
	}


	///////////////// Touch

	void checkHit(Vector2 loc) {

		// cast ray down into the world from the screen touch point
		Ray ray = Camera.main.ScreenPointToRay(loc);
		// show in debugger
		//Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
		
		// detect collisions with that point
		RaycastHit[] hits = Physics.RaycastAll(ray);

		// we should have hit at least something, like the ground
        if(hits.Length == 0) return;

        // interate through all the intersections with that ray
        foreach(RaycastHit hit in hits) {

        	// first check to see if we're interacting a speech balloon
        	if (hit.transform.gameObject.tag == "Phylactere") {

        		// ok, we clicked on a phylactere, that's top priority
        		TouchedPhylactere(loc, hit.point, hit.transform.gameObject);
        		// ignore rest of click/touch
        		return;
        	}

        }

        // interate through all the intersections with that ray
        foreach(RaycastHit hit in hits) {
			
        	// check to see if this is another Persona that we're perhaps already talking to
        	if (hit.transform.gameObject.tag == "Persona") {

        		// see if we're already talking to them
        		if (walk.isCollidingWith(hit.transform.gameObject)) {
        			// indicate that we clicked on a Persona
        			TouchedPersona(loc, hit.point, hit.transform.gameObject);
        			// ignore rest of click/touch
        			return;
        		} else {
        			// otherwise, we've clicked on a player we're not already talking to, set target to here
        			TouchedGround(hit.transform.position);
        			// ignore rest of click/touch
        			return;
        		}

        	}

        }

        // ok, after all that see if we're clicking on player
        foreach(RaycastHit hit in hits) {

        	// ok, now check for click on player
        	if (hit.transform.gameObject.tag == "Player") {
        		// indicate that we clicked on the player
        		TouchedPlayer(loc, hit.point);
        		// ignore rest of click/touch
        		return;
        	}

        	// finally check to see if this is a point near enough to the player
        	if (Vector3.Distance(hit.point,playerObject.transform.position) < playerRadius) {
        		// indicate that we clicked on the player
        		TouchedPlayer(loc, hit.point);
        		// ignore rest of click/touch
        		return;
        	}

        }

        // ok, after all that see if we're clicking on a tumbleweed

        // get a list of all the tumbleweeds
        GameObject[] tumbleweeds = GameObject.FindGameObjectsWithTag("Tumbleweed");

        // check in each hit
        foreach(RaycastHit hit in hits) {

        	// check for click directly on tumbleweed
        	if (hit.transform.gameObject.tag == "Tumbleweed") {
        		// indicate that we clicked on the tumbleweed
        		TouchedTumbleweed(loc, hit.point, hit.transform.gameObject);
        		// ignore rest of click/touch
        		return;
        	}

        	foreach(GameObject tumbleweed in tumbleweeds) {
				// check for proximity
	        	if (Vector3.Distance(hit.point,tumbleweed.transform.position) < tumbleweedRadius) {
        			// indicate that we clicked on the tumbleweed
        			TouchedTumbleweed(loc, hit.point, tumbleweed);
	        		// ignore rest of click/touch
	        		return;
	        	}
        	}

        }

        // ok, that's all no, now see if we're touching the Ground
        foreach(RaycastHit hit in hits) {

        	// ignore ground click/touch
        	if (hit.transform.name != "Ground") continue;

			// get the point on the plane where we clicked and go there
			TouchedGround(hit.point);
			// ok, all done
			return;

		}

 	}



 	///////////////// Actions

	void TouchedGround(Vector3 loc) {

		// just in case
		loc.y = 0.01f;
		// kill all other clicks
		GameObject[] otherClicks;
    	otherClicks = GameObject.FindGameObjectsWithTag("ClickTouch");
    	foreach (GameObject obj in otherClicks) {
    		Destroy(obj);
    	}
		// montrer où on a cliqué
		GameObject clicker = Instantiate(exploder, loc, Quaternion.identity) as GameObject;
		clicker.name = "Clicker";

		// tell the player where to start walking
		playerScript.SetTargetPosition(loc);

	}


	void TouchedPlayer(Vector2 touchPoint, Vector3 hitPoint) {

		Talk talk = playerObject.GetComponent<Talk>();

		// is someone talking?
		if (talk.IsTalking()) {
			talk.ClickAccelerate();
		}

	}


	void TouchedPersona(Vector2 touchPoint, Vector3 hitPoint, GameObject persona) {

		Talk talk = persona.GetComponent<Talk>();

		// is someone talking?
		if (talk.IsTalking()) {
			talk.ClickAccelerate();
		} else {
			playerScript.StartTalking(persona);
		}

	}


	// get the parent of the phylactere we clicked on
	void TouchedPhylactere(Vector2 touchPoint, Vector3 hitPoint, GameObject touchedObject) {
    
    	// climb up parent chain until we hit the parent containing the Talk engine
		Transform topParent = touchedObject.transform.parent;
		// recursive-ish loop
		while(topParent.GetComponent<Talk>() == null) {
			topParent = topParent.parent;
		}
    	// ok, we're at the top
    	GameObject obj = topParent.gameObject;
    	// is it the player?
    	if (obj.tag == "Player") {
    		phylactere = playerObject.GetComponentInChildren<Phylactere>();
    		// tell the phylactère that we just clicked on it
    		phylactere.touchDown(touchPoint, hitPoint);
    		return;
    	}
    	// or is it a computer-controller persona?
    	if (obj.tag == "Persona") {
    		// tell the phylactère to accelerate the dialogue
    		obj.GetComponent<Talk>().ClickAccelerate();
    		//phylactere = obj.GetComponentInChildren<Phylactere>();
    		// tell the phylactère that we just clicked on it
    		//phylactere.ClickAccelerate();
    	}
	
	}


	void TouchedTumbleweed(Vector2 touchPoint, Vector3 hitPoint, GameObject obj) {

		// calculate direction
		Vector3 direction = obj.transform.position - hitPoint;
		direction.Normalize();

		obj.GetComponent<Tumbleweed>().Jump(direction);

	}

}
