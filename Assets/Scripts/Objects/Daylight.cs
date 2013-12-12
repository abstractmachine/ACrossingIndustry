using UnityEngine;
using System.Collections;

public class Daylight : MonoBehaviour {

	public Light lightPtr;
	public GameObject surfacePtr;

	// Use this for initialization
	void Start () {

		Application.targetFrameRate = 60;

	}
	
	// Update is called once per frame
	void Update () {

		float timeSaturation = GameState.Instance.getTimeSaturation();

		Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);

		Camera.main.backgroundColor = c;

		float lightAngle = 180.0f + (timeSaturation * 180.0f);
		lightPtr.transform.rotation = Quaternion.Euler(60.0f, lightAngle, 120.0f);

		surfacePtr.renderer.material.color = c;

	}

}
