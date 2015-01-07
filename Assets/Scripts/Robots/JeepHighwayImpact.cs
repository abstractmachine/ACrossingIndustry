using UnityEngine;
using System.Collections;

public class JeepHighwayImpact : MonoBehaviour {
	
	
	public float force = 1000; // adjust the impact force
	public float yforce = 10; // adjust the impact force
	
	void OnTriggerEnter(Collider other) {
		
		Vector3 dir = other.transform.position - transform.position;
		dir.y = yforce; // keep the force horizontal
		if (other.rigidbody){ // use AddForce for rigidbodies:
			other.rigidbody.AddForce(dir.normalized * force);
		} else { // use a special script for character controllers:
			// try to get the enemy's script ImpactReceiver:
			ImpactReceiver script = other.GetComponent<ImpactReceiver>(); 
			// if it has such script, add the impact force: 
			if (script){
				script.AddImpact(dir.normalized * force);
			}
		} 
	}
	
	void OnTriggerExit(Collider other) {
		if (other.gameObject.name == "Ouvrier") {
			
			print ("uh?! Actually you survived…");
			
		}
	}
	
}