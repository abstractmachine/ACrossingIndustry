using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Barriere : MonoBehaviour {
		GameObject barbelee;
		
		void Start() {
		
			barbelee = this.transform.Find("Barbelee").gameObject;
		
		}


		void OnTriggerEnter(Collider other) {
	
				if (other.gameObject.name == "Ouvrier") {

					barbelee.SetActive(true);

				}
		}

		void OnTriggerExit(Collider other) {
				if (other.gameObject.name == "Ouvrier") {
				
					barbelee.SetActive(false);

				}
		}

}