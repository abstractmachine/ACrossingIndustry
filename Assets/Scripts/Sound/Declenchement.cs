using UnityEngine;
using System.Collections;

public class Declenchement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// si on est entré dans le zone du son
	void OnTriggerEnter(Collider trigger){

		// si c'est le personnage qui a déclenché
		if (trigger.gameObject.tag == "Player") {

			// chercher la liste des sons associés
			AudioSource[] sources = GetComponents<AudioSource>();
			// combien de sons associés à cet objet?
			int howMany = sources.Length;
			// choisir un des sons au hasard
			int index = (int)Random.Range(0,howMany);
			// déclencher ce son s'il ne joue pas déjà
			if (!sources[index].isPlaying) {
				sources[index].Play();
			}

		}
	
	}


	void OnTriggerExit(Collider trigger){
/*
		// si c'est le personnage qui a déclenché
		if (trigger.gameObject.name == "Capsule" || trigger.gameObject.name == "Hero" || trigger.gameObject.name == "Personnage") {

			print("Exit");

			// chercher la liste des sons associés
			AudioSource[] sources = GetComponents<AudioSource>();

			foreach(AudioSource source in sources) {
				if (source.isPlaying) source.Stop();
			}

		}
		*/
	}


}
