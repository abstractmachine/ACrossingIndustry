using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <List>

public class Persona : MonoBehaviour {

	// the list of walking coordinates
	List<Vector3> coordinates = new List<Vector3>();

	float timeoutLength = 10.0f;
	float timeoutValue;

	void Start () {

		// set the starting length to a random value
		timeoutValue = timeoutLength * Random.Range(1.0f,5.0f);

	}



	public void SetMaterial(Material newMaterial) {

		FindMaterialInChild(transform, newMaterial);

		/*
		// find the child containing the material
		foreach(Transform child in transform) {
			foreach(Transform subchild in child) {

				// not the material layer
	    		if(subchild.gameObject.tag != "Material") continue;
	        		// change material color
				subchild.gameObject.renderer.material = newMaterial;
				print("Set Material");
				break;

			} // foreach(Transfrom subchild
    	} // foreach(Transform child
    		*/

	}


	void FindMaterialInChild(Transform child, Material newMaterial) {

		foreach(Transform grandchild in child) {

			if (grandchild.gameObject.tag != "Material") {
				FindMaterialInChild(grandchild, newMaterial);
				return;
			}

        	// ok, found it change material color
			grandchild.gameObject.renderer.material = newMaterial;
			return;

		}

	}
	

	void Update () {
	
		timeoutValue -= Time.deltaTime;
		if (timeoutValue < 0.0f) SetRandomTargetCoordinate();

	}


	void ResetTimeout() {
	
		timeoutValue = timeoutLength;

	}


	void SetRandomTargetCoordinate() {

		int randomIndex = (int)Random.Range(0,coordinates.Count);
		SetTargetCoordinate(randomIndex);
		
	}


	void SetTargetCoordinate(int coordinateIndex) {

		if (coordinates.Count == 0) return;

		Vector3 targetCoordinate = coordinates[coordinateIndex];
		// remove any y axis translations
		targetCoordinate.y = transform.position.y;
		
		// reset timeout
		ResetTimeout();

		//print("Set new target for " + gameObject.name + " to " + targetCoordinate);

	}


	public void SetCoordinates(List<Vector3> newCoordinates) {

		// remember this list of coordinates
		coordinates = newCoordinates;
		// set to the first coordinate
		SetTargetCoordinate(0);

	}


}
