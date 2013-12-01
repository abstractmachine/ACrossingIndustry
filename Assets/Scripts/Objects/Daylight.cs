using UnityEngine;
using System.Collections;

public class Daylight : MonoBehaviour {

	public Light lightPtr;

	// Use this for initialization
	void Start () {

		Application.targetFrameRate = 60;

	}
	
	// Update is called once per frame
	void Update () {

		drawBackground();

	}


	void drawBackground() {

		float timeSaturation = GameState.Instance.getTimeSaturation();
		Camera.main.backgroundColor = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);

		float lightAngle = 180.0f + (timeSaturation * 180.0f);
		lightPtr.transform.rotation = Quaternion.Euler(60.0f, lightAngle, 120.0f);

	}

}
