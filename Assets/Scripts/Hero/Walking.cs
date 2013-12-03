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
    public float turnSpeed = 180.0f;

    public float zoomOutValue = 30.0f;
    public float zoomInValue = 5.0f;
    public float zoom = 5.0f;
    
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextPointRequiredDistance = 2.5f;
    public float endPointRequiredDistance = 2f;

    int endPointIndex = 0;
    float currentDistanceToEndPoint = 0f;
 
    //The waypoint we are currently moving towards
    private int currentPointIndex = 0;
 
    public void Start () {

        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();

    }
    
    public void OnPathComplete (Path p) {

        if (!p.error) {
            // set the path to this incoming path
            path = p;
            // get the index of the endpoint
            endPointIndex = (int)Mathf.Max(0,path.vectorPath.Count-1);
            //Reset the waypoint counter
            currentPointIndex = 0;
            // start by turning in the right direction
            turnToTarget();
        }

    }

    public void OnDisable () {

        seeker.pathCallback -= OnPathComplete;

    } 


    public void setTargetPosition(Vector3 newTarget) {

        // remove previous path
        clearPath();

        targetPosition = newTarget;
        
        // stop walking animation (in case we were previously walking)
        animation.Play("idle");

        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        seeker.StartPath(transform.position, targetPosition, OnPathComplete);

    }


    void clearPath() {

        path = null;
        endPointIndex = -1;

    }


 
    public void FixedUpdate () {

        // we have no path to move after yet
        if (path == null) return;

        calculatePosition();
        turnTowardsTarget();
        
        //Check to see how close we are to the next waypoint
        float distance = Vector3.Distance(transform.position,path.vectorPath[currentPointIndex]);

        // if we're not close enough
        if (distance > nextPointRequiredDistance) {
            // just keep walking
            walkAStep();

        } else { // ok, we're getting close

            // if we aren't at the endpoint, proceed on to the next waypoint
            if (currentPointIndex != endPointIndex) {
                
                animation.Play("walk");
                currentPointIndex++;

            } else { // ok, we're at the endpoint, start sliding

                turnTowardsTarget();
                slideToTarget();

                // are we at target?
                if (isNearTarget()) {

                    if (isOnTarget()) {
                        snapToTarget();
                        stopAtEndpoint();
                    }
                }

            }

        }

    }


    void calculatePosition() {

        // get position of feet on floor
        Vector3 feetPosition = transform.position + new Vector3(0f,-0.5f,0f);

        // calculate current position compared to endpoint
        currentDistanceToEndPoint = Vector3.Distance(feetPosition, path.vectorPath[endPointIndex]);
        // calculate a zoom value
        float newZoom = Mathf.Min(zoomOutValue,Mathf.Max(zoomInValue,currentDistanceToEndPoint*1.0f));
        zoom += (newZoom - zoom) * 0.025f;
        Camera.main.GetComponent<Follow>().setZoom(zoom);

    }


    bool isNearTarget() {

        if (currentPointIndex >= endPointIndex && currentDistanceToEndPoint <= endPointRequiredDistance) return true;
        else return false;

    }


    bool isOnTarget() {

        float distance = Vector3.Distance(path.vectorPath[endPointIndex], transform.position + new Vector3(0f,-0.5f,0f));

        print("distance = " + distance);

        // don't forget to take into account that we're 0.5 units above ground
        if (distance <= 0.1f) return true;
        else return false;

    }


    void walkAStep() {

        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentPointIndex]-transform.position).normalized;
        dir *= speed * Time.fixedDeltaTime;
        controller.SimpleMove(dir);

    }


    void slideToTarget() {

        // use a Zenon to progressively move towards the target
        Vector3 newPoint = Vector3.Lerp(transform.position + new Vector3(0f,-0.5f,0f), path.vectorPath[endPointIndex], 0.05f);
        // get the delta vector of the target
        Vector3 delta = newPoint-transform.position;
        // go directly to the final waypoint
        CollisionFlags flags = controller.Move(delta);
        // check via bitmask flag if a side collision was detected
        if ((flags & CollisionFlags.Sides) == CollisionFlags.Sides) {
            print("Side collision");
        }

        //print((path.vectorPath[endPointIndex]-transform.position).magnitude);

    }


    void snapToTarget() {

        // get the delta vector of the target
        Vector3 delta = path.vectorPath[endPointIndex]-(transform.position + new Vector3(0f,-0.5f,0f));
        // go directly to the final waypoint
        CollisionFlags flags = controller.Move(delta);
        // check via bitmask flag if a side collision was detected
        if ((flags & CollisionFlags.Sides) == CollisionFlags.Sides) {
            print("Side collision");
        }

    }


    void stopAtEndpoint() {

        animation.Play("idle");
        // set zoom target to minimum
        zoom = zoomInValue;
        Camera.main.GetComponent<Follow>().setZoom(zoom);
        
        Debug.Log ("Reached endpoint. Clearing path");
        clearPath();

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

        Quaternion lookTargetRotation = getTargetRotation(path.vectorPath[currentPointIndex]);
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