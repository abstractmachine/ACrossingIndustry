using UnityEngine;
using System.Collections;

public class GameStateLoader : MonoBehaviour {

	// Use this for initialization
	void Start () {

		//Screen.showCursor = false;

		// if not yet instantiated
		if (!GameState.isInstantiated) {
			// don't destroy it
    		DontDestroyOnLoad(GameState.Instance);
    		// setup internal variables
			GameState.Instance.setup();
		}
	
	} // Start()

}
