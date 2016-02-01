using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour {

	public GameObject player;
	public GameObject touchPointPrefab;

	void OnMouseDown() {

		checkHit(Input.mousePosition);

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

		foreach (RaycastHit hit in hits) {      
			// ignore ground click/touch
			if (hit.transform.name != "Ground") {
				continue;
			}         
			// get the point on the plane where we clicked and go there
			TouchedGround(hit.point);
			// ok, all done
			return;         
		}

	}

   
	public void TouchedGround(Vector3 position) {
		
		// reposition to ground
		position.y = 0.01f;
		// kill all other clicks
		GameObject[] otherClicks;
		otherClicks = GameObject.FindGameObjectsWithTag("TouchPoint");
		foreach (GameObject obj in otherClicks) {
			Destroy(obj);
		}
		// montrer où on a cliqué
		GameObject touchPoint = Instantiate(touchPointPrefab, position, Quaternion.Euler(90, 0, 0)) as GameObject;
		touchPoint.name = "TouchPoint";
		touchPoint.transform.parent = this.transform;
		// blow up in a co-routine
		StartCoroutine(Explode(touchPoint));

		// tell the player where to start walking
		player.GetComponent<Move>().GoToPosition(position);

	}


	IEnumerator Explode(GameObject touchPoint) {
		
//		float timeSaturation = Camera.main.GetComponent<Daylight>().TimeSaturation;
//		timeSaturation = Mathf.Min(1.0f, 1.3f - timeSaturation);
//		Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);
		//renderer.material.color = c;
//		GetComponent<Renderer>().material.SetColor("_Emission", c);

		float explosionSpeed = 0.025f;

		while (touchPoint != null && touchPoint.transform.localScale.x < 0.5f) {      
			touchPoint.transform.localScale = touchPoint.transform.localScale + new Vector3(explosionSpeed, explosionSpeed, explosionSpeed);
			yield return new WaitForEndOfFrame();
		}

		Destroy(touchPoint);

	}

}
