using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour {

	// Singleton instance reference variable
	private static GameState instance;

	// the internal states

	// time
	float daylightSpeed = 0.025f;

	static float timePercent = 0.0f;
	static float timeSaturation = 0.0f;
	static int timeHour = 1;


	public void setup() {

	}

	void Update() {

		updateDaylight();

	}


	// local methods

	void updateDaylight() {

		float t = Mathf.Repeat((Time.time*daylightSpeed), Mathf.PI*2);

		// set saturation
		timeSaturation = (1.0f + Mathf.Sin(t)) * 0.5f;
		// the percentage 0.0f -> 1.0f
		timePercent = Mathf.Repeat((Time.time*daylightSpeed), 1.0f);

		timeHour = (int)map(t,0.0f, Mathf.PI*2, 0.0f, 24.0f);
		timeHour = (int)Mathf.Repeat(timeHour+6.0f, 24.0f);

	}

	// accessor methods

	public int getTimeHour() {
		return timeHour;
	}


	public float getTimeSaturation() {
		return timeSaturation;
	}


	public float getTimePercent() {
		return timePercent;
	}




	// Singleton instance
	// use the upper case to get that (private) static instance
	public static GameState Instance {
		get {

			// if we haven't yet instantiated this object
			if (null == instance) {
				// instantiate it
				instance = new GameObject("GameState").AddComponent<GameState>();
			}
			// get the pointer to the instance
			return instance;

		} // get
	} // Instance


	// check for instantiation
	public static bool isInstantiated {
		get {
			return instance != null;
		}
	}


	// Clean-up
	// Sets the instance to null when the application quits
	public void OnApplicationQuit() {
		instance = null;
	}



	// Code tools

	float map(float val, float low1, float high1, float low2, float high2) {

		return low2 + (val - low1) * (high2 - low2) / (high1 - low1);

	}

}
