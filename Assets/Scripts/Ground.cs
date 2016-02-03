using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Ground : MonoBehaviour, IPointerClickHandler {

	public GameObject player;
	public GameObject touchPointPrefab;

	public void OnPointerClick(PointerEventData eventData) {

		checkHit(eventData.position);

	}


	void checkHit(Vector2 loc) {

		// cast ray down into the world from the screen touch point
		Ray ray = Camera.main.ScreenPointToRay(loc);
		// show in debugger
		//Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        
		// detect collisions with that point
		RaycastHit[] hits = Physics.RaycastAll(ray);

		// we should have hit at least something, like the ground
		if (hits.Length == 0) {
			Debug.LogWarning("No hit detection");
			return;
		}

		bool didHitGround = false;
		Vector3 groundHitPoint = Vector3.zero;

		foreach (RaycastHit hit in hits) {      
			// make sure it's a ground click/touch
			if (hit.transform.name == "Ground") {
				didHitGround = true;
				groundHitPoint = hit.point;
			}
		}

		if (didHitGround) {
			// get the point on the plane where we clicked and go there
			TouchedGround(groundHitPoint);
		}

	}


	public void TouchedObject(GameObject other) {

		// get the position of the other object
		Vector3 position = other.transform.position;
		// reposition to ground
		position.y = 0.01f;
		// if we were already showing a click exploder
		RemovePreviousClicks();
		// show click
		ShowClick(position);
		// tell the player who to start walking to
		player.GetComponent<Player>().GoToObject(other);

	}

   
	public void TouchedGround(Vector3 position) {

		// reposition to ground
		position.y = 0.01f;
		// if we were already showing a click exploder
		RemovePreviousClicks();
		// show click
		ShowClick(position);
		// tell the player where to start walking
		player.GetComponent<Player>().GoToPosition(position);

	}


	void RemovePreviousClicks() {
		// kill all other clicks
		GameObject[] otherClicks;
		otherClicks = GameObject.FindGameObjectsWithTag("TouchPoint");
		foreach (GameObject obj in otherClicks) {
			Destroy(obj);
		}

	}


	void ShowClick(Vector3 position) {

		// montrer où on a cliqué
		GameObject touchPoint = Instantiate(touchPointPrefab, position, Quaternion.Euler(90, 0, 0)) as GameObject;
		touchPoint.name = "TouchPoint";
		touchPoint.transform.parent = this.transform;
		// blow up in a co-routine
		StartCoroutine(Explode(touchPoint));

	}


	IEnumerator Explode(GameObject touchPoint) {

//		// match background color for the sprite color
//		float timeSaturation = Camera.main.GetComponent<Daylight>().TimeSaturation;
//		timeSaturation = Mathf.Min(1.0f, 1.3f - timeSaturation);
//		Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);
//		GetComponent<SpriteRenderer>().color = c;

		float explosionSpeed = 0.025f;

		while (touchPoint != null && touchPoint.transform.localScale.x < 0.5f) {      
			touchPoint.transform.localScale = touchPoint.transform.localScale + new Vector3(explosionSpeed, explosionSpeed, explosionSpeed);
			yield return new WaitForEndOfFrame();
		}

		Destroy(touchPoint);

	}

}
