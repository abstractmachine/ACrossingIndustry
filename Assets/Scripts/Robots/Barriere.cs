using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Barriere : MonoBehaviour {

		public Material transparentMaterial;
		public Material opaqueMaterial;
		
		GameObject barbelee;
		
		void Start() {
		
				barbelee = transform.Find("Barbelee").gameObject;
		
		}


		void OnTriggerEnter(Collider other) {
				print(other);
				if (other.gameObject.name == "Ouvrier") {
						//renderer.material = opaqueMaterial;
						barbelee.SetActive(true);
						barbelee.SetActiveRecursively(true);
				}
		}

		void OnTriggerExit(Collider other) {
				if (other.gameObject.name == "Ouvrier") {
						//renderer.material = transparentMaterial;
						barbelee.SetActive(false);
						barbelee.SetActiveRecursively(false);
				}
		}

}