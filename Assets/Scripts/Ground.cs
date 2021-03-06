﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Ground : MonoBehaviour, IPointerClickHandler {

	public GameObject player;

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

		// tell the player who to start walking to
		player.GetComponent<Player>().GoToObject(other);

	}

   
	public void TouchedGround(Vector3 position) {

		// reposition to ground
		position.y = 0.01f;
		// tell the player where to start walking
		player.GetComponent<Player>().GoToPosition(position);

	}

}
