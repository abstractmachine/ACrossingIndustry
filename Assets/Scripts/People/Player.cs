using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List<>

public class Player : Actor {

	// Actor's type as a string
	public override string Type { get { return "Player"; } }

	// shows target of click/touch
    public GameObject xSpot;

    public float playerImpatienceDelay = 10.0f;
    public int targetHistoryMax = 100;
    List<Vector3> targetHistory = new List<Vector3>();

    // the camera zooming levels
    Follow follow;

    // the cheat system
    Cheat cheat;


	///////////////// Init

	protected override void Start () {

		// first call the case class (Actor)
		base.Start();

        cheat = GetComponent<Cheat>();

        // the camera follows the player
        follow = Camera.main.GetComponent<Follow>();

        impatienceDelay = playerImpatienceDelay;

        SetTargetPosition(transform.position);
	
	}
	


	///////////////// Loop

	protected override void Update () {

		base.Update();

        // if no new keyboard inputs
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0)) {
            // turn on/off cheat mode
            cheat.KeyDown();
        }

		UpdateCamera();
	
	}


    void UpdateCamera() {

        //float zoomScale = (Screen.width/1680.0f);
        float zoomScale = 1.0f;
        float zoomDistance = walk.CurrentDistanceToEndPoint * zoomScale;

        zoomDistance = Mathf.Pow(zoomDistance,1.4f);

        follow.setZoom(zoomDistance);

    }



    public override void SetTargetPosition(Vector3 newTarget, bool record=true) {

        // record this position, if necessary
        if (record) RecordTargetHistory(newTarget);

        cheat.setCoordinates(newTarget);

        // if cheat is on, just go there
        if (cheat.IsOn) {

            walk.JumpToTarget(newTarget);

	        follow.forceZoomOut();
	        setCurrentDistanceToMax();

	        // TODO: this is kinda ugly, find something better
	        if (IsInvoking("setCurrentDistanceToZero")) {
	            CancelInvoke("setCurrentDistanceToZero");
	        }

	        Invoke("setCurrentDistanceToZero", 2.0f);
            return;
        }

        // set the coordinates of the new target
        base.SetTargetPosition(newTarget);

        // create an x-spot where we clicked
        Instantiate(xSpot, newTarget, Quaternion.identity);


    }



    void RecordTargetHistory(Vector3 newTarget) {

        // append a new value to the end of our List<Vector3>
        targetHistory.Add(newTarget);

        // if too many elements in list
        if (targetHistory.Count > targetHistoryMax) {
            // remove from beginning of list (i.e. the oldest in the list)
            targetHistory.RemoveAt(0);
        }

    }



    void SetRandomTarget() {

        // if no history, forget it
        if (targetHistory.Count <= 1) return;

        // if we're walking, wait until we're done
        if (walk.IsWalking) return;

        if (cheat.IsOn) return;

        // extract a random position from the list
        Vector3 randomPosition = targetHistory[(int)Random.Range(0,targetHistory.Count)];

        // if target is too close to us, abort. We'll chose another on the next cycle
        if (Vector3.Distance(transform.position,randomPosition) < 15) {
            return;
        }

        // set that position
        SetTargetPosition(randomPosition,false);

    }



    public override void ClearTarget() {

        GameObject[] objects = GameObject.FindGameObjectsWithTag("xSpot");

        foreach(GameObject obj in objects) {
            Destroy(obj);
        }

    }



    ///////////////// Impatience

    protected override void DoSomethingImpatient() { // overrides base class

    	if (cheat.IsOff) SetRandomTarget();

    }



	///////////////// Walking

    public override void DidAbortWalking() {

        // set zoomLevel target to minimum
        follow.setZoom(0.0f);

    }



    //////////////////// Talking

    public override void StartTalking(GameObject other) {

        talk.ActivateDialog(other);

    }


    public override void StopTalking(GameObject other) {

        AbortDialog();

    }


    public override void AbortDialog() {

    	talk.AbortDialog();

    }


    ////////////////////////// Camera Following


    void setCurrentDistanceToZero() {

        walk.SetCurrentDistanceToZero();

    }


    void setCurrentDistanceToMax() {

        walk.SetCurrentDistance(follow.zoomMax);

    }



}
