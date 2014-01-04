using UnityEngine;
using System.Collections;

public class Fader : MonoBehaviour {

	public Texture filler;

	float fadeValue = 0.0f;
	float fadeSpeed = 0.4f;

	bool fading = false;
	bool fadeIn = false;


	void Start() {

		fadeIn = true;

	}


	void OnGUI() {
 
 		float v = 192.0f / 255.0f;
 		GUI.color = new Color(v, v, v, 1.0f-fadeValue);
		//GUI.color = new Color(0.0f, 0.0f, 0.0f, 1.0f-fadeValue);
		GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height ), filler );

	}
	

	void Update () {

		// should we start/stop fading?
		if (!Fading() && FadingIn()) StartFadeIn();
		else if (!Fading() && FadingOut()) StartFadeOut();
		else if (Fading() && Faded()) StopFade();

		// are we fading?
		if (fading && FadingIn()) Fade();

	}


	void Fade() {

		if (FadingIn()) FadeIn();
		else FadeOut();

	}


	void FadeIn() {

		fadeValue = Mathf.Clamp01(fadeValue + (Time.deltaTime * fadeSpeed));

	}


	void FadeOut() {

		fadeValue = Mathf.Clamp01(fadeValue - (Time.deltaTime * fadeSpeed));

	}


	void StopFade() {

		fading = false;

	}


	void StartFadeIn() {

		fadeIn = true;
		fading = true;

	}


	void StartFadeOut() {

		fadeIn = false;
		fading = true;

	}


	public bool Fading() { return fading; }
	public bool FadingIn() { return fadeIn == true; }
	public bool FadingOut() { return fadeIn == false; }
	public bool Faded() { if (FadingIn()) return FadedIn(); else return FadedOut(); }
	public bool FadedIn() { return (fadeValue>=1.0f); }
	public bool FadedOut() { return (fadeValue<=0.0f); }

}
