using UnityEngine;
using System.Collections;

public class Daylight : MonoBehaviour {

	public Light lightPtr;
	public GameObject surfacePtr;

	// time
	public float daylightSpeed = 0.025f;
	static float timePercent = 0.0f;
	static float timeSaturation = 0.0f;
	static int timeHour = 1;


	// accessor methods

	public int TimeHour { get { return timeHour; } }
	public float TimeSaturation { get { return timeSaturation; } }
	public float TimePercent { get { return timePercent; } }


	// Use this for initialization
	void Start () {

		Application.targetFrameRate = 60;

	}
	

	// Update is called once per frame
	void Update () {

		if (Input.GetKey("escape")) {
            Application.Quit();
    	}

		// set the time of the day
		UpdateTime();

		// 
		UpdateDaylight();

	}


	void UpdateDaylight() {

		//float timeSaturation = GameState.Instance.getTimeSaturation();
		float timeSaturation = TimeSaturation;

		Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);

		Camera.main.backgroundColor = c;

		float lightAngle = 180.0f + (timeSaturation * 180.0f);
		lightPtr.transform.rotation = Quaternion.Euler(60.0f, lightAngle, 120.0f);

		surfacePtr.renderer.material.color = c;

	}


	// local methods

	void UpdateTime() {

		float t = Mathf.Repeat((Time.time*daylightSpeed), Mathf.PI*2);

		// set saturation
		timeSaturation = (1.0f + Mathf.Sin(t)) * 0.5f;
		// the percentage 0.0f -> 1.0f
		timePercent = Mathf.Repeat((Time.time*daylightSpeed), 1.0f);

		timeHour = (int)Map(t,0.0f, Mathf.PI*2, 0.0f, 24.0f);
		timeHour = (int)Mathf.Repeat(timeHour+6.0f, 24.0f);

	}


	// Code tools

	float Map(float val, float low1, float high1, float low2, float high2) {

		return low2 + (val - low1) * (high2 - low2) / (high1 - low1);

	}

}
