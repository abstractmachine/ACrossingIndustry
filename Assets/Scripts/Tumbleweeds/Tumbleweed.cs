using UnityEngine;
using System.Collections;

public class Tumbleweed : MonoBehaviour {

	float countdown = 0.0f;
	float hiddenCountdown = 0.0f;

	public float hiddenTime = 10.0f;
	public float strength = 100.0f;

	// Use this for initialization
	void Start () {
	
		hiddenCountdown = hiddenTime * 5.0f;

		startCountdown();

	}
	
	// Update is called once per frame
	void Update () {
	
		countdown -= Time.deltaTime;

		// if it's time to push the tumbleweed
		if (countdown <= 0.0f) {
			pushTumbleweed();
			startCountdown();
		}

		// check to see if we're out of bounds
		checkBounds();

	}



	void pushTumbleweed() {

		if (Random.Range(0.0f,1.0f) < 0.5f) pushTumbleweedUp();

		rigidbody.AddForce(Random.Range(-strength, strength), 0.0f, Random.Range(-strength,strength));

	}


	void pushTumbleweedUp() {

		rigidbody.AddForce(0.0f, Random.Range(1.0f,strength*2), 0.0f);

	}



	void checkBounds() {

		// est-ce qu'on est visible?
		if (transform.GetChild(0).GetChild(0).renderer.isVisible) {
			// remettre le timer à 10 secondes
			hiddenCountdown = hiddenTime;
		} else {
			// enlever du temps sur le timer
			hiddenCountdown -= Time.deltaTime;
			// si on est à zero
			if (hiddenCountdown <= 0.0f) {
				// faire disparaitre
				Destroy(gameObject);
			}
		}

	}


	void startCountdown() {

		countdown = Random.Range(1.0f,3.0f);

	}


}
