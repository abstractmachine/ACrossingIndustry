using UnityEngine;
using System.Collections;

public class Ripple : MonoBehaviour {

	// Use this for initialization
	void Start () {

		float timeSaturation = Camera.main.GetComponent<Daylight>().getTimeSaturation();
		timeSaturation = Mathf.Min(1.0f, 1.3f - timeSaturation);
		Color c = new Color(timeSaturation, timeSaturation, timeSaturation, 1.0f);
		//renderer.material.color = c;
		renderer.material.SetColor("_Emission", c);

	}
	
	// Update is called once per frame
	void Update () {
	
		transform.localScale = transform.localScale + new Vector3(0.25f, 0.0f, 0.25f);

		if (transform.localScale.x > 5.0f) Destroy(gameObject, 0.01f);

	}
}
