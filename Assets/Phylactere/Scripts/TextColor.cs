using UnityEngine;
using System.Collections;

public class TextColor : MonoBehaviour {

	// Use this for initialization
	void Awake () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setColor(Color c) {

		// get pointer to rectangle
		GameObject rectangle = transform.Find("Bulle").Find("PlaneScaler").Find("Rectangle").gameObject;
		Material rectangleMaterial = rectangle.renderer.material;

		GameObject triangle = transform.Find("Bulle").Find("Triangle").gameObject;
		Material triangleMaterial = triangle.renderer.material;

		rectangleMaterial.color = c;
		triangleMaterial.color = c;

	}

}
