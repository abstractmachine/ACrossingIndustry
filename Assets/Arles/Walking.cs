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
    public float nextWaypointDistance = 3;
 
    //The waypoint we are currently moving towards
    private int currentWaypoint = 0;
 
    public void Start () {

        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
    }
    
    public void OnPathComplete (Path p) {

        //Debug.Log ("Yey, we got a path back. Did it have an error? " + p.error);

        if (!p.error) {
            path = p;
            //Reset the waypoint counter
            currentWaypoint = 0;
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
        
        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath (transform.position, targetPosition, OnPathComplete);

    }
 
    public void FixedUpdate () {

        if (path == null) {
            //We have no path to move after yet
            return;
        }
        
        // are we at target?
        if (currentWaypoint >= path.vectorPath.Count) {
            animation.Play("idle");
            // set zoom target to minimum
            zoom = zoomInValue;
            Camera.main.GetComponent<Follow>().setZoom(zoom);
            //Debug.Log ("End Of Path Reached");
            return;
        } else {
            // calculate current position compared to endpoint
            float distanceToEndPoint = Vector3.Distance(transform.position, path.vectorPath[(int)Mathf.Max(0,path.vectorPath.Count-1)]);
            // calculate a zoom value
            float newZoom = Mathf.Min(zoomOutValue,Mathf.Max(zoomInValue,distanceToEndPoint*2.0f));
            zoom += (newZoom - zoom) * 0.05f;
            Camera.main.GetComponent<Follow>().setZoom(zoom);
        }
        
        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentWaypoint]-transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;
        controller.SimpleMove(dir);

        turnTowardsTarget();
        
        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (Vector3.Distance(transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
            currentWaypoint++;
            animation.Play("walk");
            return;
        }
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

        Quaternion lookTargetRotation = getTargetRotation(path.vectorPath[currentWaypoint]);
        float step = turnSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookTargetRotation, step);

    }


    // where am I going?
    Quaternion getTargetRotation(Vector3 lookTarget) {

        // make sure we remove any eventual y transformations
        lookTarget.y = transform.position.y;

        Vector3 relativePos = lookTarget - transform.position;
        Quaternion lookTargetRotation = Quaternion.LookRotation(relativePos);

        return lookTargetRotation;

    }

} 