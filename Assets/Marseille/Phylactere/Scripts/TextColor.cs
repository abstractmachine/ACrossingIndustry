using UnityEngine;
using System.Collections;

public class TextColor : MonoBehaviour {

	void Awake() {

	}

	// Use this for initialization
	void Start() {

		Transform p = transform.parent;
		print(p.name);

		// get the first material
		Renderer childRenderer = p.GetComponentInChildren<Renderer>();
		setColor(childRenderer.material.color);

	}
	
	// Update is called once per frame
	void Update() {
	
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
