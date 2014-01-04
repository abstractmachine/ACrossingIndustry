using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <List>

public class Persona : Actor {

	// Actor's type as a string
	public override string Type { get { return "Persona"; } }

	// the list of walking coordinates for the Persona
	List<Vector3> coordinates = new List<Vector3>();

    public float personaImpatienceDelay = 30.0f;


	////////////////// Init

	protected override void Start () {

		base.Start();

        impatienceDelay = personaImpatienceDelay;
        impatienceCountdown = Random.Range(1,impatienceDelay);

	}


	public void SetCoordinates(List<Vector3> newCoordinates) {

		// remember this list of coordinates
		coordinates = newCoordinates;
		// set to the first coordinate
		Vector3 targetCoordinate = GetTargetCoordinate(0);

		SetTargetPosition(targetCoordinate);

	}	


	/////////////////// Loop

	protected override void Update () {

		base.Update();

	}



	///////////////////// Initialize Material to show which "clan/camp" this Persona blong to

	public void SetMaterial(Material newMaterial) {

		FindMaterialInChild(transform, newMaterial);

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


	///////////////// Impatience

	protected override void DoSomethingImpatient() { // overrides base class

		SetNextTargetCoordinate();
	
	}


	void SetNextTargetCoordinate() {

		// there have to be at least two coordinates to change position
		if (coordinates.Count <= 1) return;

		int coordinateIndex = (int)Random.Range(0,coordinates.Count);
		Vector3 targetCoordinate = GetTargetCoordinate(coordinateIndex);

		SetTargetPosition(targetCoordinate);

	}


	Vector3 GetTargetCoordinate(int coordinateIndex) {

		Vector3 targetCoordinate = coordinates[coordinateIndex];
		// remove any y axis translations
		targetCoordinate.y = transform.position.y;

		return targetCoordinate;

	}


}
