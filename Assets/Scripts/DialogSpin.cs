﻿using UnityEngine;
using System.Collections;

public class DialogSpin : MonoBehaviour {


	void Start() {

		StartCoroutine(SpinAround());

	}


	IEnumerator SpinAround() {

		transform.FindChild("Text").gameObject.SetActive(false);

		// depending on which one this is
		int spinIndex = (transform.parent.childCount - transform.GetSiblingIndex());
		// spin at a earlier/later time
		yield return new WaitForSeconds(spinIndex * 0.1f);
		// text begins hidden
		bool flipped = false;
		// loop the angles
		for (float angle = 180.0f; angle >= 0.0f; angle -= 20.0f) {
			// if we haven't flipped around yet
			if (!flipped && angle <= 90.0f) {
				// flipped on
				flipped = true;
				// turn on text
				transform.FindChild("Text").gameObject.SetActive(true);
			}
			// apply a rotation for this angle locally
			GetComponent<RectTransform>().localRotation = Quaternion.Euler(0.0f, angle, 0.0f);
			// show frame
			yield return new WaitForEndOfFrame();         
		}      
		// apply a rotation for this angle locally
		GetComponent<RectTransform>().localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		// all done
		yield return null;      
	}
}
