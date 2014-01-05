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

    bool paused = false;
    public bool IsPaused { get { return paused; } }

    // state accessor
    public bool IsWalking { get { return path != null; } }






    ////////////////// Init
 
    public void Start () {

        // if we are the player, there will be a player component
        actor = gameObject.GetComponent<Player>();
        // no actor, so we must be a Persona
        if (actor == null) {
            actor = gameObject.GetComponent<Persona>();
        }


        seeker = gameObject.GetComponent<Seeker>();
        controller = gameObject.GetComponent<CharacterController>();

        ResetWaitingForPathDelay();

    }

    public void OnDisable () {

        seeker.pathCallback -= OnPathComplete;

    }






    //////////////// Loop

    void Update() {

        // Update pathfinding
        UpdateSeeker();

    }

 
    public void FixedUpdate () {

        // if we have no path to move to, nothing to do
        if (path == null) return;

        // calculate our position in relation to path
        if (!paused) UpdatePosition();

        // in case we anulled walk during UpdatePosition
        if (path == null) return;

        // turn to face nextPoint
        if (!paused) TurnTowardsTarget();

        // if we're not close enough
        if (currentDistanceToNextPoint > nextPointRadius) {
            // just keep walking
            if (!paused) WalkAStep();
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
                StopWalkingAnimation();
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
            
            StartWalkingAnimation();
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




    ////////////////// Collision detection

    void OnTriggerEnter(Collider other){
        
        // if I'm the player and they're a Persona
        if ("Player" == gameObject.tag && "Persona" == other.gameObject.tag) {

            // if they are walking
            if (other.gameObject.GetComponent<Actor>().IsWalking) {
                // this pauses the other actor's walking but starts again OnTriggerExit
                other.gameObject.GetComponent<Actor>().PauseWalking();
            }
        }


    }


    void OnTriggerStay(Collider other){

        // if we haven't already started the dialog
        if (!collisionFlag) {
            // if I'm the player, the other is a Persona
            if ("Player" == gameObject.tag && "Persona" == other.gameObject.tag) {
                // get access to other actor
                Actor otherActor = other.gameObject.GetComponent<Actor>();
                // and I'm not talking and they're not talking
                if (!actor.IsTalking && !otherActor.IsTalking) {

                    // and I'm close enough if they were walking
                    // or my target is close enough if they were standing still
                    if ((otherActor.IsWalking  && IsPositionCloseEnoughTo(other.gameObject))
                    ||  (!otherActor.IsWalking && IsTargetCloseEnoughTo(other.gameObject))
                    ){

                        AbortWalking();

                        // add to the list of people I'm in collision with
                        AddToCollisionList(other.gameObject);

                        //Actor otherActor = other.gameObject.GetComponent<Actor>();
                        //if (otherActor.IsWalking() && !otherActor.IsPaused()) otherActor.AbortWalking();

                        collisionFlag = true;

                        // tell the player (me) to start talking
                        actor.StartTalking(other.gameObject);

                    } //
                } // if (!actor.IsTalking
            } // if ("Player"
        } // if (collisionFlag

    }


    void OnTriggerExit(Collider other){

        if ("Player" == gameObject.tag && "Persona" == other.gameObject.tag) {
    
            // if we interrupted the other actor
            if (other.gameObject.GetComponent<Actor>().IsPaused) {
                // tell them to start walking again
                other.gameObject.GetComponent<Actor>().ResumeWalking();
            }

            RemoveFromCollisionList(other.gameObject);
            actor.StopTalking(other.gameObject);
            collisionFlag = false;

        }

    }



    public bool IsNearEndpoint() {

        if (currentDistanceToEndPoint < nextPointRadius/2.0f) return true;
        return false;

    }


    bool IsPositionCloseEnoughTo(GameObject obj) {
        
        if (Vector3.Distance(transform.position, obj.transform.position) < nextPointRadius) return true;
        else return false;

    }



    bool IsTargetCloseEnoughTo(GameObject obj) {

        if (Vector3.Distance(targetPosition, obj.transform.position) < nextPointRadius) return true;
        else return false;

    }



    void AddToCollisionList(GameObject other) {

        // if it's not already listed, add to collision list
        if (!IsCollidingWith(other)) collisions.Add(other);

    }


    void RemoveFromCollisionList(GameObject other) {

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


    public bool IsCollidingWith(GameObject other) {

        // go through the list
        foreach(GameObject o in collisions) {
            // if this object is in the list, return TRUE
            if (o == other) return true;
        }
        // otherwise, we didn't find it
        return false;

    }



    bool isOnTarget() {

        // if we're close enough to stop
        if (currentDistanceToEndPoint <= snapToPointRadius) return true;
        else return false; // not on top of target

    }

    

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





    ///////////////////// Animation


    void StartWalkingAnimation() {
        
        // make sure we're playing the "walk" animation
        animation.CrossFade("walk", 0.2f);

    }


    void StopWalkingAnimation() {
        
        // stop walking animation (in case we were previously walking)
        animation.CrossFade("idle", 0.2f);

    }


    void WalkAStep() {

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

        SetCurrentDistanceToZero();

    }




    ///////////////// Path Targeting

    public void SetTargetPosition(Vector3 newTarget) {

        // remove previous path
        ClearPath();

        // stop any current talking
        actor.AbortDialog();

        targetPosition = newTarget;

        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);

        // start a timer in case the path search takes too long
        ResetWaitingForPathDelay();

        // flag that we're waiting for a reply from the seeker
        isWaitingForSeeker = true;

    }


    public void JumpToTarget(Vector3 newTarget) {

        targetPosition = newTarget;

        TurnToTarget(newTarget);

        // get the position of the feet
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);
        // get the delta vector of the target
        Vector3 delta = newTarget - feetPosition;
        // go directly to the final waypoint
        transform.position = transform.position + delta;

    }


    public void AbortWalking() {

        // stop walk animation
        StopWalkingAnimation();

        actor.DidAbortWalking();

        // ok, we've reached the end, no need to pathfind
        ClearPath();

        SetCurrentDistanceToZero();

    }



    public void PauseWalking() {

        paused = true;

        if (IsWalking) {
            StopWalkingAnimation();
        }

    }


    public void ResumeWalking() {

        paused = false;

        if (IsWalking) {
            StartWalkingAnimation();
        }

    }


    void TurnToTarget() {

        // make sure there's a first point to point towards
        if (path.vectorPath.Count < 2) return;

        TurnToTarget(path.vectorPath[1]);
        /*
        // get that first point
        Quaternion lookTargetRotation = TargetRotation(path.vectorPath[1]);
        // point towards it without any restrictions
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, 360.0f);
        */
    }


    void TurnToTarget(Vector3 target) {

        // get that first point
        Quaternion lookTargetRotation = TargetRotation(target);
        // point towards it without any restrictions
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, 360.0f);

    }


    void TurnTowardsTarget() {

        // get the quaternion that looks at the nextPoint
        Quaternion lookTargetRotation = TargetRotation(path.vectorPath[currentPointIndex]);
        // use turnSpeed to limit turning
        float step = turnSpeed * Time.deltaTime;
        // turn to the resulting rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, step);

    }


    // where am I going?
    Quaternion TargetRotation(Vector3 lookTarget) {

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
    void ResetWaitingForPathDelay() {

        waitingForSeekTimeout = waitForSeekDelay;

    }


    
    public void OnPathComplete(Path p) { // this is called by component <Phylactere> when it's done

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
            TurnToTarget();
            // ok, we're seeking
            isWaitingForSeeker = false;
        }

    }


    void ClearPath() {

        path = null;
        endPointIndex = -1;
        currentPointIndex = -1;
        isWaitingForSeeker = false;

        paused = false;

        stuckTimeout = stuckTimeDelay;

        clearStuckPatternCheck();
        
        actor.ClearTarget();

    }


    public void SetCurrentDistanceToZero() {

        currentDistanceToEndPoint = 0.0f;

    }


    public void SetCurrentDistance(float value) {

        currentDistanceToEndPoint = value;

    }

} 
