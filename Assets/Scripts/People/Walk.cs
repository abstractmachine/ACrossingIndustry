using UnityEngine;
using System.Collections;
using System.Collections.Generic; // required for List<>
using Pathfinding; // Pathfinding include

// TODO: Separate from Walking all the Dialog/Hero/Player functions


// class declaration
public class Walk : MonoBehaviour {

    // MARK: variable declarations

    // pointer to the character controller of this persona
    CharacterController controller;
    // the actor (Player or Persona) controlling this Walk
    Actor actor;

    //The path calculated by A* Pathfinder
    public Path path = null;

    //The point to move to
    public Vector3 targetPosition;
    Vector3 lastPosition;
    List<float> latestDistances = new List<float>();
    
    //The AI's speed per second
    public float speed = 100;
    public float turnSpeed = 180.0f;
    
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextPointRadius = 1f;
    public float snapToPointRadius = 0.1f;

    // pointer to the Seeker algorithm
    Seeker seeker;
    float waitingForSeekTimeout = 0.0f;
    bool isWaitingForSeeker = false;
    public float waitForSeekDelay = 0.1f;

    // the time it takes to determine we're stuck and stop walking
    float stuckTimeDelay = 0.1f;
    float stuckTimeout = 0.0f;

    // where is the endPoint in the vectorPath?
    int endPointIndex = -1;
    //The waypoint we are currently moving towards
    int currentPointIndex = 0;

    float currentDistanceToNextPoint = 0f;
    float currentDistanceToEndPoint = 0f;
    public float CurrentDistanceToEndPoint { get { return currentDistanceToEndPoint; } }

    // others I'm currently in collision with
    List<GameObject> collisions = new List<GameObject>();
    bool collisionFlag = false;

    // state accessor
    public bool IsWalking() { return path != null; }


    // MARK: Init
 
    public void Start () {

        // if we are the player, there will be a player component
        actor = gameObject.GetComponent<Player>();
        // no actor, so we must be a Persona
        if (actor == null) {
            actor = gameObject.GetComponent<Persona>();
        }


        seeker = gameObject.GetComponent<Seeker>();
        controller = gameObject.GetComponent<CharacterController>();

        resetWaitingForPathDelay();

    }

    public void OnDisable () {

        seeker.pathCallback -= OnPathComplete;

    }


    // MARK: Updates


    void Update() {

        // Update pathfinding
        UpdateSeeker();

    }

 
    public void FixedUpdate () {

        // if we have no path to move to, nothing to do
        if (path == null) return;

        // calculate our position in relation to path
        UpdatePosition();

        // in case we anulled walk during UpdatePosition
        if (path == null) return;

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
        if (endPointIndex > -1) {
            currentDistanceToEndPoint = Vector3.Distance(feetPosition, path.vectorPath[endPointIndex]);
        }

        // calculate current position compared to next waypoint
        if (currentPointIndex > -1) {
            currentDistanceToNextPoint = Vector3.Distance(feetPosition, path.vectorPath[currentPointIndex]);
        }

        // make sure we aren't stuck walking into a wall
        checkForStuckPattern(feetPosition);


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
        
        // if we're near the endpoint (and we're the Player) get rid of xSpot
        if (actor.Type == "Player" && IsNearEndpoint()) {
            actor.ClearTarget();
        }
        
        // are we sitting on top of the target?
        if (isOnTarget()) {
            // just to the target
            snapToTarget();
            // stop walking animation; clear path
            AbortWalking();
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
                AbortWalking();
            } // if (stuckTimeout)

        } // if(latestDistances.Average()

        lastPosition = position;

    }


    void clearStuckPatternCheck() {

        latestDistances.Clear();

    }


    // MARK: Collision detection

    void OnTriggerEnter(Collider other){
        
        // if I'm the player and they're a Persona
        if ("Player" == gameObject.tag && "Persona" == other.gameObject.tag) {

            // add to the list of people I'm in collision with
            addToCollisionList(other.gameObject);
            // if they are walking
            if (other.gameObject.GetComponent<Actor>().IsWalking()) {
                // stop them
                // TODO: this should just be a pause that starts again OnTriggerExit
                other.gameObject.GetComponent<Actor>().PauseWalking();
            }
        }


    }


    void OnTriggerStay(Collider other){

        // if I'm the player, the other is a Persona
        // and I'm not talking and they're not talking
        // and I'm close enough

        if ("Player" == gameObject.tag && "Persona" == other.gameObject.tag &&
            !actor.IsTalking() && !other.gameObject.GetComponent<Actor>().IsTalking() &&
            isCloseEnoughToTarget(other.gameObject) &&
            !collisionFlag
            ) {

            AbortWalking();
            other.gameObject.GetComponent<Actor>().AbortWalking();

            collisionFlag = true;

            // tell the player (me) to start talking
            actor.StartTalking(other.gameObject);

        }

    }


    void OnTriggerExit(Collider other){
        
        if ("Persona" == other.gameObject.tag) {
            removeFromCollisionList(other.gameObject);
            actor.StopTalking(other.gameObject);
            collisionFlag = false;
        }

    }



    public bool IsNearEndpoint() {
        if (currentDistanceToEndPoint < nextPointRadius/2.0f) return true;
        return false;
    }


    /*
    public bool IsNearWaypoint() {
        if (currentDistanceToEndPoint < nextPointRadius/2.0f) return true;
        return false;
    }*/



    bool isCloseEnoughToTarget(GameObject obj) {

        //if (Vector3.Distance(targetPosition, obj.transform.position) < nextPointRadius) return true;
        if (Vector3.Distance(transform.position, obj.transform.position) < nextPointRadius) return true;
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
            AbortWalking();
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


    // MARK: Path Targeting

    public void SetTargetPosition(Vector3 newTarget) {

        // remove previous path
        ClearPath();

        // stop any current talking
        actor.AbortDialog();

        targetPosition = newTarget;

        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);

        // start a timer in case the path search takes too long
        resetWaitingForPathDelay();

        // flag that we're waiting for a reply from the seeker
        isWaitingForSeeker = true;

    }


    public void JumpToTarget(Vector3 newTarget) {

        targetPosition = newTarget;

        turnToTarget(newTarget);

        // get the position of the feet
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);
        // get the delta vector of the target
        Vector3 delta = newTarget - feetPosition;
        // go directly to the final waypoint
        transform.position = transform.position + delta;

    }


    public void AbortWalking() {

        // stop walk animation
        stopWalkingAnimation();

        actor.DidAbortWalking();

        // ok, we've reached the end, no need to pathfind
        ClearPath();

        setCurrentDistanceToZero();

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


    void ClearPath() {

        path = null;
        endPointIndex = -1;
        currentPointIndex = -1;
        isWaitingForSeeker = false;

        stuckTimeout = stuckTimeDelay;

        clearStuckPatternCheck();
        
        actor.ClearTarget();

    }


    public void setCurrentDistanceToZero() {

        currentDistanceToEndPoint = 0.0f;

    }


    public void setCurrentDistance(float value) {

        currentDistanceToEndPoint = value;

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
