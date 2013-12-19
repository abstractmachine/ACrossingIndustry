using UnityEngine;
using System.Collections;

public class ClickTouch : MonoBehaviour {

	public GameObject exploder;
	public GameObject xSpot;

	Walking walking;
	GameObject player;

	public float playerRadius = 1.0f;

	// if we're touching a phylactère, maintain a connection to the code component
	Phylactere phylactere = null;
	Vector3 lastTouchPosition = Vector3.zero;

	// Use this for initialization
	void Start () {

		Input.simulateMouseWithTouches = false;

		walking = GameObject.FindWithTag("Player").GetComponent<Walking>();
		player = GameObject.FindWithTag("Player");

	}
	
	// Update is called once per frame
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
				walking.ResetImpatience();

			}

		} else {

			
			if (Input.GetMouseButtonDown(0)) {		// click to change position
				checkHit(Input.mousePosition);
				lastTouchPosition = Input.mousePosition;

				// tell the hero that there's been some activity
				walking.ResetImpatience();

			} else if (phylactere != null && Input.GetMouseButton(0)) { 	// if we're interacting with a phylactere
				if (Vector3.Distance(lastTouchPosition,Input.mousePosition) > 0) phylactere.touchMoved(Input.mousePosition);
				lastTouchPosition = Input.mousePosition;

				// tell the hero that there's been some activity
				walking.ResetImpatience();
			
			} else if (phylactere != null && Input.GetMouseButtonUp(0)) {	// if we're interacting with a phylactere
				phylactere.touchUp(Input.mousePosition);
				lastTouchPosition = Vector3.zero;
				phylactere = null;

				// tell the hero that there's been some activity
				walking.ResetImpatience();
				
			}

		}

		// TODO: add double-click for run-to-position
		
	}


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
        		touchedPhylactere(loc, hit.point, hit.transform.gameObject);
        		// ignore rest of click/touch
        		return;
        	}
			
        	// otherwise, check to see if this is another Persona that we're perhaps already talking to
        	if (hit.transform.gameObject.tag == "Persona") {

        		// see if we're already talking to them
        		if (walking.isCollidingWith(hit.transform.gameObject)) {
        			// indicate that we clicked on a Persona
        			touchedPersona(loc, hit.point, hit.transform.gameObject);
        			// ignore rest of click/touch
        			return;
        		} else {
        			// otherwise, we've clicked on a player we're not already talking to, set target to here
        			setClickTarget(hit.transform.position);
        			// ignore rest of click/touch
        			return;
        		}

        	}

        	// ok, now check for click on player
        	if (hit.transform.gameObject.tag == "Player") {
        		// indicate that we clicked on the player
        		touchedPlayer(loc, hit.point);
        		// ignore rest of click/touch
        		return;
        	}

        	// finally check to see if this is a point near enough to the player
        	if (Vector3.Distance(hit.point,player.transform.position) < playerRadius) {
        		// indicate that we clicked on the player
        		touchedPlayer(loc, hit.point);
        		// ignore rest of click/touch
        		return;
        	}

        }

        // ok, that's fine, now try to find the Ground
        foreach(RaycastHit hit in hits) {

        	// ignore ground click/touch
        	if (hit.transform.name != "Ground") continue;

			// get the point on the plane where we clicked and go there
			setClickTarget(hit.point);

		}

 	}


	void setClickTarget(Vector3 loc) {

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
		walking.setTargetPosition(loc);

	}


	void touchedPlayer(Vector2 touchPoint, Vector3 hitPoint) {

		Dialog dialog = player.GetComponent<Dialog>();

		// is someone talking?
		if (dialog.IsTalking()) {
			dialog.ClickAccelerate();
		}

	}


	void touchedPersona(Vector2 touchPoint, Vector3 hitPoint, GameObject persona) {

		Dialog dialog = persona.GetComponent<Dialog>();

		// is someone talking?
		if (dialog.IsTalking()) {
			dialog.ClickAccelerate();
		} else {

			print("StartTalking " + persona);
			walking.StartTalking(persona);
		}

	}


	// get the parent of the phylactere we clicked on
	void touchedPhylactere(Vector2 touchPoint, Vector3 hitPoint, GameObject touchedObject) {
    
    	// climb up parent chain until we hit the parent containing the Dialog engine
		Transform topParent = touchedObject.transform.parent;
		// recursive-ish loop
		while(topParent.GetComponent<Dialog>() == null) {
			topParent = topParent.parent;
		}
    	// ok, we're at the top
    	GameObject obj = topParent.gameObject;
    	// is it the player?
    	if (obj.tag == "Player") {
    		phylactere = player.GetComponentInChildren<Phylactere>();
    		// tell the phylactère that we just clicked on it
    		phylactere.touchDown(touchPoint, hitPoint);
    		return;
    	}
    	// or is it a computer-controller persona?
    	if (obj.tag == "Persona") {
    		// tell the phylactère to accelerate the dialogue
    		obj.GetComponent<Dialog>().ClickAccelerate();
    		//phylactere = obj.GetComponentInChildren<Phylactere>();
    		// tell the phylactère that we just clicked on it
    		//phylactere.ClickAccelerate();
    	}
	
	}

}
