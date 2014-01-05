using UnityEngine;
using System.Collections;

abstract public class Actor : MonoBehaviour {

    // the action components
    protected Walk walk;
    protected Talk talk;

    // are we Player or Persona?
    abstract public string Type { get; }

	public bool IsWalking { get { return walk.IsWalking; } }
	public bool IsPaused  { get { return walk.IsPaused; } }
    public bool IsTalking { get { return talk.IsTalking; } }

    // the time it takes to get bored and randomly do something
    public float impatienceDelay = 60.0f; // two minutes
    protected float impatienceCountdown = 0f;


    ///////////////// Init

    protected virtual void Start() {

		walk = gameObject.GetComponent<Walk>();
		talk = gameObject.GetComponent<Talk>();

        ResetImpatience();

    }


    ///////////////// Loop

    protected virtual void Update() {

		UpdateImpatience();

    }



	///////////////// Targeting

	public virtual void SetTargetPosition(Vector3 newTarget, bool record=true) {

		if (walk == null) {
			return;
		}

        walk.SetTargetPosition(newTarget);

        // reset the "I'm bored" countdown
        ResetImpatience();

	}



	///////////////// Walking


	public void PauseWalking() {

		walk.PauseWalking();

	}


	public void ResumeWalking() {

		walk.ResumeWalking();

	}


	public void AbortWalking() {

		walk.AbortWalking();

	}


    public virtual void DidAbortWalking() {
 		// we get a confirmation when walking was aborted
    }


	public virtual void ClearTarget() {

	}



    //////////////////// Talking

    public virtual void StartTalking(GameObject other) {

    }


    public virtual void StopTalking(GameObject other) {

    }


    public virtual void AbortDialog() {

    }



	///////////////// Impatience

    protected virtual void DoSomethingImpatient() {



    }



    void UpdateImpatience() {

		if (walk == null) return;

		// if we're active, don't get impatient
		if (walk.IsWalking) ResetImpatience();
		//if (talk.IsTalking()) ResetImpatience();

        impatienceCountdown -= Time.deltaTime;

        // if we run out of patience
        if (impatienceCountdown <= 0.0f) {
            // randomly click somewhere for us
            DoSomethingImpatient();
        }

    }



    public void ResetImpatience() {

        impatienceCountdown = impatienceDelay;

    }

}
