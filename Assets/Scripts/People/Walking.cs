using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List<>
using Pathfinding; // Pathfinding include


// class declaration
public class Walking : MonoBehaviour {

    // MARK: variable declarations

    // pointer to the character controller of this persona
    CharacterController controller;
    Dialog dialog;

    //The calculated path
    public Path path;

    public GameObject xSpot;

    //The point to move to
    public Vector3 targetPosition;
    Vector3 lastPosition;
    List<float> latestDistances = new List<float>();
    List<Vector3> targetHistory = new List<Vector3>();
    public int targetHistoryMax = 100;
    
    //The AI's speed per second
    public float speed = 100;
    public float turnSpeed = 180.0f;

    // the camera zooming levels
    Follow follow;
    
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextPointRadius = 1f;
    public float snapToPointRadius = 0.1f;

    public float waitForSeekDelay = 0.1f;
    // pointer to the Seeker algorithm
    Seeker seeker;
    float waitingForSeekTimeout = 0.0f;
    bool isWaitingForSeeker = false;

    // the time it takes to determine we're stuck and stop walking
    float stuckTimeDelay = 0.1f;
    float stuckTimeout = 0.0f;

    // the time it takes to get bored and randomly click somewhere
    public float impatienceDelay = 120.0f; // two minutes
    float impatienceCountdown = 0f;

    // where is the endPoint in the vectorPath?
    int endPointIndex = -1;
    //The waypoint we are currently moving towards
    int currentPointIndex = 0;

    float currentDistanceToEndPoint = 0f;
    float currentDistanceToNextPoint = 0f;

    // others I'm currently in collision with
    List<GameObject> collisions = new List<GameObject>();

    // the cheat system
    Cheat cheat;

    // state accessor
    public bool IsWalking() { return path != null; }


    // MARK: Init
 
    public void Start () {

        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        dialog = GetComponent<Dialog>();
        follow = Camera.main.GetComponent<Follow>();

        cheat = GetComponent<Cheat>();

        resetWaitingForPathDelay();

        ResetImpatience();

        setTargetPosition(transform.position);

    }

    public void OnDisable () {

        seeker.pathCallback -= OnPathComplete;

    }


    // MARK: Updates


    void Update() {

        // if no new keyboard inputs
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0)) {
            // turn on/off cheat mode
            cheat.KeyDown();
        }

        // Update pathfinding
        UpdateSeeker();

    }

 
    public void FixedUpdate () {

        // Update impatience timer
        UpdateImpatience();

        // zoom in/out based on our proximity to endPoint
        UpdateCamera();

        // if we have no path to move to, nothing to do
        if (path == null) return;

        // calculate our position in relation to path
        UpdatePosition();

        // in case we anulled walk during UpdatePosition
        if (path == null) return;

        // we must be walking
        ResetImpatience();

        // turn to face nextPoint
        turnTowardsTarget();

        // if we're not close enough
        if (currentDistanceToNextPoint > nextPointRadius) {
            // just keep walking
            walkAStep();
            return;
        }

        // ok, if we're here, we're getting close

        UpdateWaypoint();

    }


    void UpdateSeeker() {

        // are we still waiting for the seeker to reply with a new path?
        if (isWaitingForSeeker) {
            // start countdown
            waitingForSeekTimeout -= Time.deltaTime;
            // if too long, stop to wait
            if (waitingForSeekTimeout < 0) {
                // turn off flag
                isWaitingForSeeker = true;
                // stop walking animation
                stopWalkingAnimation();
            }
        }

    }


    void UpdatePosition() {

        // get position of feet on floor, taking into account that our center (pelvis) is at 0.5f on y axis
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);

        // calculate current position compared to endpoint
        if (endPointIndex > -1) currentDistanceToEndPoint = Vector3.Distance(feetPosition, path.vectorPath[endPointIndex]);
        
        // calculate current position compared to next waypoint
        if (currentPointIndex > -1) currentDistanceToNextPoint = Vector3.Distance(feetPosition, path.vectorPath[currentPointIndex]);

        // make sure we aren't stuck walking into a wall
        checkForStuckPattern(feetPosition);


    }


    void UpdateCamera() {

        //float zoomScale = (Screen.width/1680.0f);
        float zoomScale = 1.0f;
        float zoomDistance = currentDistanceToEndPoint * zoomScale;

        zoomDistance = Mathf.Pow(zoomDistance,1.4f);

        follow.setZoom(zoomDistance);

    }



    void UpdateWaypoint() {

        // if we aren't at the endpoint, proceed on to the next waypoint
        if (currentPointIndex != endPointIndex) {
            
            // make sure we're playing the "walk" animation
            animation.CrossFade("walk", 0.2f);
            // advance nextPointtarget
            currentPointIndex++;
            return;

        }

        // ok, we're at the endpoint, start sliding

        // the endPoint acts as a magnet
        slideToTarget();
        
        // get rid of xSpot
        if (currentDistanceToEndPoint < nextPointRadius/2.0f) clearXSpots();

        // are we sitting on top of the target?
        if (isOnTarget()) {
            // just to the target
            snapToTarget();
            // stop walking animation; clear path
            abortWalking();
        }

    }


    void UpdateImpatience() {

        impatienceCountdown -= Time.deltaTime;

        // if we run out of patience
        if (impatienceCountdown <= 0.0f) {
            // randomly click somewhere for us
            SetRandomTarget();
        }

    }


    // MARK: Stuck Pattern check

    void checkForStuckPattern(Vector3 position) {

        // calculate delta in order to check if we are stuck
        float distance = Vector3.Distance(position, lastPosition);
        // add to distances tableau
        latestDistances.Add(distance);
        // remove first element if list is too big
        if (latestDistances.Count > 10) latestDistances.RemoveAt(0);

        // get the average of all that (previously used .Average(), but this required Linq which is overkill)
        float average = 0.0f;
        foreach(float f in latestDistances) average += f;
        average /= (float)latestDistances.Count;
        
        //float average = latestDistances.Average();

        // is the average movement large enough?
        if (average > 0.001f) {

            // ok, we're not stuck, reset timeout
            stuckTimeout = stuckTimeDelay;

        } else {

            // see if we've been stuck for a while
            stuckTimeout -= Time.deltaTime;
            // ok, we've been stuck for a while
            if (stuckTimeout < 0f) {
                abortWalking();
            } // if (stuckTimeout)

        } // if(latestDistances.Average()

        lastPosition = position;

    }


    void clearStuckPatternCheck() {

        latestDistances.Clear();

    }


    // MARK: Collision detection

    void OnTriggerEnter(Collider other){

        if ("Persona" == other.gameObject.tag && isCloseEnoughToTarget(other.gameObject)) {
            abortWalking();
            addToCollisionList(other.gameObject);
            StartTalking(other.gameObject);
        }
        
    }


    /*void OnTriggerStay(Collider other){
        
    }*/


    void OnTriggerExit(Collider other){
        
        if ("Persona" == other.gameObject.tag) {
            removeFromCollisionList(other.gameObject);
            StopTalking(other.gameObject);
        }

    }



    public void StartTalking(GameObject other) {

        dialog.activateDialog(other);

    }


    public void StopTalking(GameObject other) {

        dialog.abortDialog();

    }



    bool isCloseEnoughToTarget(GameObject obj) {

        if (Vector3.Distance(targetPosition, obj.transform.position) < nextPointRadius) return true;
        else return false;

    }



    void addToCollisionList(GameObject other) {

        // if it's not already listed, add to collision list
        if (!isCollidingWith(other)) collisions.Add(other);

    }


    void removeFromCollisionList(GameObject other) {

        int count = collisions.Count;
        int index = 0;

        // go through all the objects
        while(index < count) {

            // if this object exists
            if (collisions[index] == other) {
                // remove it from our list
                collisions.RemoveAt(index);
                // adjust the number of objects to check
                count = collisions.Count;
                continue;
            }
            // iterate to next in list
            index++;
        }

    }


    public bool isCollidingWith(GameObject other) {

        // go through the list
        foreach(GameObject o in collisions) {
            // if this object is in the list, return TRUE
            if (o == other) return true;
        }
        // otherwise, we didn't find it
        return false;

    }

    /*void OnControllerColliderHit(ControllerColliderHit hit) {

        // don't worry about Ground
        if (hit.gameObject.name == "Ground") return;

        print(hit.gameObject);

        // if we've just collided with a Persona
        if ("Persona" == hit.gameObject.tag) {
            abortWalking();
        }

    }*/




    bool isOnTarget() {

        // if we're close enough to stop
        if (currentDistanceToEndPoint <= snapToPointRadius) return true;
        else return false; // not on top of target

    }


    void stopWalkingAnimation() {
        
        // stop walking animation (in case we were previously walking)
        animation.CrossFade("idle", 0.2f);

    }


    void walkAStep() {

        // direction vector to the next waypoint without taking into account magnitude
        Vector3 dir = (path.vectorPath[currentPointIndex]-transform.position).normalized;
        // walk in that direction
        dir *= speed * Time.fixedDeltaTime;
        // tell controller move many units to move
        controller.SimpleMove(dir);

    }


    void slideToTarget() {

        // get the position of the feet
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);
        // use a Zenon to progressively move towards the target
        Vector3 newPoint = Vector3.Lerp(feetPosition, path.vectorPath[endPointIndex], 0.05f);
        // get the delta vector of the target
        Vector3 delta = newPoint-transform.position;
        // go directly to the final waypoint
        CollisionFlags flags = controller.Move(delta);
        // check via bitmask flag if a side collision was detected
        if ((flags & CollisionFlags.Sides) == CollisionFlags.Sides) {
            ; // print("Side collision");
        }

    }


    void snapToTarget() {

        // get the position of the feet
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);
        // get the delta vector of the target
        Vector3 delta = path.vectorPath[endPointIndex] - feetPosition;
        // go directly to the final waypoint
        CollisionFlags flags = controller.Move(delta);
        // check via bitmask flag if a side collision was detected
        if ((flags & CollisionFlags.Sides) == CollisionFlags.Sides) {
            ; // print("Side collision");
        }

        setCurrentDistanceToZero();

    }


    void jumpToTarget(Vector3 newTarget) {

        targetPosition = newTarget;

        turnToTarget(newTarget);

        // get the position of the feet
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);
        // get the delta vector of the target
        Vector3 delta = newTarget - feetPosition;
        // go directly to the final waypoint
        transform.position = transform.position + delta;

        follow.forceZoomOut();
        setCurrentDistanceToMax();

        cheat.setCoordinates(newTarget);

        if (IsInvoking("setCurrentDistanceToZero")) {
            CancelInvoke("setCurrentDistanceToZero");
        }

        Invoke("setCurrentDistanceToZero", 2.0f);

    }


    void abortWalking() {

        // stop walk animation
        stopWalkingAnimation();
        // set zoomLevel target to minimum
        follow.setZoom(0.0f);
        // ok, we've reached the end, no need to pathfind
        clearPath();

        setCurrentDistanceToZero();

    }


    void setCurrentDistanceToZero() {

        currentDistanceToEndPoint = 0.0f;

    }


    void setCurrentDistanceToMax() {

        currentDistanceToEndPoint = follow.zoomMax;

    }


    void turnToTarget() {

        // make sure there's a first point to point towards
        if (path.vectorPath.Count < 2) return;

        turnToTarget(path.vectorPath[1]);
        /*
        // get that first point
        Quaternion lookTargetRotation = getTargetRotation(path.vectorPath[1]);
        // point towards it without any restrictions
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, 360.0f);
        */
    }


    void turnToTarget(Vector3 target) {

        // get that first point
        Quaternion lookTargetRotation = getTargetRotation(target);
        // point towards it without any restrictions
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, 360.0f);

    }


    void turnTowardsTarget() {

        // get the quaternion that looks at the nextPoint
        Quaternion lookTargetRotation = getTargetRotation(path.vectorPath[currentPointIndex]);
        // use turnSpeed to limit turning
        float step = turnSpeed * Time.deltaTime;
        // turn to the resulting rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, step);

    }


    // where am I going?
    Quaternion getTargetRotation(Vector3 lookTarget) {

        // make sure we remove any eventual y-axis transformations
        lookTarget.y = transform.position.y;

        Vector3 relativePos = lookTarget - transform.position;

        // if no movement
        if (relativePos == Vector3.zero) return Quaternion.identity;

        // otherwise, calculate rotation
        Quaternion lookTargetRotation = Quaternion.LookRotation(relativePos);
        return lookTargetRotation;

    }


    // this is used when the pathfinder takes too long to search (> 0.x seconds)
    void resetWaitingForPathDelay() {

        waitingForSeekTimeout = waitForSeekDelay;

    }


    // MARK: Path Targeting

    public void setTargetPosition(Vector3 newTarget, bool record=true) {

        // remove previous path
        clearPath();

        // stop any current dialogs
        dialog.abortDialog();

        // reset the "I'm bored" countdown
        ResetImpatience();

        // record this position
        if (record) RecordTargetHistory(newTarget);

        // if cheat is on, just go there
        if (cheat.IsOn) {
            jumpToTarget(newTarget);
            return;
        }

        targetPosition = newTarget;

        cheat.setCoordinates(newTarget);

        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);

        // start a timer in case the path search takes too long
        resetWaitingForPathDelay();

        // flag that we're waiting for a reply from the seeker
        isWaitingForSeeker = true;

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
        if (targetHistory.Count == 0) return;

        // if we're walking, wait until we're done
        if (IsWalking()) return;

        // extract a random position from the list
        Vector3 randomPosition = targetHistory[(int)Random.Range(0,targetHistory.Count)];

        // if target is too close to us, abort. We'll chose another on the next cycle
        if (Vector3.Distance(transform.position,randomPosition) < 15) {
            return;
        }

        // set that position
        setTargetPosition(randomPosition,false);

    }


    
    public void OnPathComplete (Path p) {

        if (!p.error) {
            // make sure there's something in there
            if (p.vectorPath == null || p.vectorPath.Count == 0) return;
            // set the path to this incoming path
            path = p;
            // get the index of the endpoint
            endPointIndex = (int)Mathf.Max(0,path.vectorPath.Count-1);
            //Reset the waypoint counter
            currentPointIndex = 0;
            // start by turning in the right direction
            turnToTarget();
            // ok, we're seeking
            isWaitingForSeeker = false;
        }

    }



    public void ResetImpatience() {

        impatienceCountdown = impatienceDelay;

    }


    void clearPath() {

        path = null;
        endPointIndex = -1;
        currentPointIndex = -1;
        isWaitingForSeeker = false;

        stuckTimeout = stuckTimeDelay;

        clearStuckPatternCheck();
        clearXSpots();

    }



    void clearXSpots() {

        GameObject[] objects = GameObject.FindGameObjectsWithTag("xSpot");

        foreach(GameObject obj in objects) {
            Destroy(obj);
        }

    }

    /*
    // MARK: Linq function

    void whoIsAroundMe(float radius) {
        
        var transformArray = GameObject.FindGameObjectsWithTag("Persona")
        .Select(go => go.transform)
        .Where(t => Vector3.Distance(t.position, transform.position) < radius)
        .ToArray();

        foreach(Transform t in transformArray) {
            print(t);
        }

    }*/

} 