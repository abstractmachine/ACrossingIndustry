﻿using UnityEngine;
using System.Collections;

public class Sunlight : MonoBehaviour {

	public float speed = 0.5f;

	void Update() {

		transform.Rotate(speed, 0.0f, 0.0f);

	}

}
