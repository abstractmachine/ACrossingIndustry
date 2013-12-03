using UnityEngine;
using System.Collections;
//Note this line, if it is left out, the script won't know that the class 'Path' exists and it will throw compiler errors
//This line should always be present at the top of scripts which use pathfinding
using Pathfinding;

public class Walking : MonoBehaviour {

    //The point to move to
    public Vector3 targetPosition;

    private Seeker seeker;
    private CharacterController controller;
 
    //The calculated path
    public Path path;
    
    //The AI's speed per second
    public float speed = 100;
    public float turnSpeed = 100.0f;

    public float zoomOutValue = 30.0f;
    public float zoomInValue = 5.0f;
    public float zoom = 5.0f;
    
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextPointRequiredDistance = 3f;
    public float endPointRequiredDistance = 1f;

    float currentDistanceToEndPoint = 0f;
 
    //The waypoint we are currently moving towards
    private int waypointIndex = 0;
 
    public void Start () {

        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();

    }
    
    public void OnPathComplete (Path p) {

        if (!p.error) {
            path = p;
            //Reset the waypoint counter
            waypointIndex = 0;
            turnToTarget();
        }

    }

    public void OnDisable () {

        seeker.pathCallback -= OnPathComplete;

    } 


    public void setTargetPosition(Vector3 newTarget) {

        // remove previous path
        path = null;

        targetPosition = newTarget;
        
        // stop walking animation (in case we were previously walking)
        animation.Play("idle");

        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);

    }
 
    public void FixedUpdate () {

        // we have no path to move after yet
        if (path == null) return;

        calculatePosition();
        turnTowardsTarget();
        walkAStep();
        
        //Check if we are close enough to the next waypoint
        float distance = Vector3.Distance(transform.position,path.vectorPath[waypointIndex]);
        //If we are, proceed to follow the next waypoint
        if (waypointIndex < (path.vectorPath.Count-1) && distance < nextPointRequiredDistance) {
            waypointIndex++;
            animation.Play("walk");
            return;
        }
        
        // are we at target?
        if (isAtTarget()) {

            turnTowardsTarget();
            slideToTarget();

            if (isOnTarget()) {
                //snapToTarget();
                stopAtEndpoint();
            }
        }

    }


    void walkAStep() {

        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[waypointIndex]-transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;
        controller.SimpleMove(dir);

    }


    void slideToTarget() {

        // get the index of the endpoint
        int endPointIndex = (int)Mathf.Max(0,path.vectorPath.Count-1);
        Vector3 delta = path.vectorPath[endPointIndex]-transform.position;
        delta *= 0.01f;
        // go directly to the final waypoint
        CollisionFlags flags = controller.Move(delta);

        //print((path.vectorPath[endPointIndex]-transform.position).magnitude);

    }


    void snapToTarget() {

        // get the index of the endpoint
        int endPointIndex = (int)Mathf.Max(0,path.vectorPath.Count-1);
        Vector3 delta = path.vectorPath[endPointIndex]-transform.position;
        // go directly to the final waypoint
        CollisionFlags flags = controller.Move(delta);

    }


    void calculatePosition() {

        // get the index of the endpoint
        int endPointIndex = (int)Mathf.Max(0,path.vectorPath.Count-1);
        // calculate current position compared to endpoint
        currentDistanceToEndPoint = Vector3.Distance(transform.position, path.vectorPath[endPointIndex]);
        // calculate a zoom value
        float newZoom = Mathf.Min(zoomOutValue,Mathf.Max(zoomInValue,currentDistanceToEndPoint*1.0f));
        zoom += (newZoom - zoom) * 0.025f;
        Camera.main.GetComponent<Follow>().setZoom(zoom);

    }


    bool isAtTarget() {

        if (waypointIndex >= path.vectorPath.Count-1 && currentDistanceToEndPoint <= endPointRequiredDistance) return true;
        else return false;

    }


    bool isOnTarget() {

        // get the index of the endpoint
        int endPointIndex = (int)Mathf.Max(0,path.vectorPath.Count-1);
        Vector3 delta = path.vectorPath[endPointIndex]-transform.position;

        if (delta.magnitude <= 0.75f) return true;
        else return false;

    }


    void stopAtEndpoint() {

        animation.Play("idle");
        // set zoom target to minimum
        zoom = zoomInValue;
        Camera.main.GetComponent<Follow>().setZoom(zoom);
        //Debug.Log ("End Of Path Reached");

        path = null;

    }


    void turnToTarget() {

        // make sure there's a first point to point towards
        if (path.vectorPath.Count < 2) return;
        // get that first point
        Quaternion lookTargetRotation = getTargetRotation(path.vectorPath[1]);
        // point towards it
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, 360.0f);

    }


    void turnTowardsTarget() {

        Quaternion lookTargetRotation = getTargetRotation(path.vectorPath[waypointIndex]);
        float step = turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, step);

    }


    // where am I going?
    Quaternion getTargetRotation(Vector3 lookTarget) {

        // make sure we remove any eventual y transformations
        lookTarget.y = transform.position.y;

        Vector3 relativePos = lookTarget - transform.position;

        // if no movement
        if (relativePos == Vector3.zero) return Quaternion.identity;

        // otherwise, calculate rotation
        Quaternion lookTargetRotation = Quaternion.LookRotation(relativePos);
        return lookTargetRotation;

    }

} 