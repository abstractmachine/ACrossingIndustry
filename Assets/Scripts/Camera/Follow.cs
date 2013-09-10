using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// adapted from: http://forum.unity3d.com/threads/93034-Isometric-Follow-Player-Camera
// shader adapted from: http://chrisrolfs.com/game-dev/unity3d/hide-objects-blocking-player-view

public class Follow : MonoBehaviour {

	public GameObject target;			                // the gameObject I want to follow
    public float zoom = 7.4f;                           // this is the zoom level
    public float[] zoomLevels = {3.0f, 7.4f, 20.0f};

 	// previously hidden items
    private Dictionary<MeshRenderer, Material> hiddenMaterials;
    //This is the material with the Transparent/Diffuse With Shadow shader
    public Material HiderMaterial;

    



	// Use this for initialization
	void Start () {

        Camera.main.isOrthoGraphic = true;
        Camera.main.transform.rotation = Quaternion.Euler(30, 45, 0);

        // set empty dictionary <object,material>
        hiddenMaterials = new Dictionary<MeshRenderer, Material>();

	}
	


	// Update is called once per frame
	void Update () {

        followTarget();

        resetHiddenMaterials();
        checkForOcclusion();

        updateZoom();

	}



    void setZoom(float distance) {

        zoom = distance;

    }



    void setZoom(int index) {

        zoom = zoomLevels[index];

    }



    void updateZoom() {

        if (hiddenMaterials.Count > 0) {
            setZoom(0);
        } else {
            setZoom(1);
        }

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize,zoom, Time.deltaTime * 5.0f);

    }



    void followTarget() {

        // the camera constantly follows the persona
        float cameraDistance = 30;  // this x/y/z distance of the camera to our persona
        transform.position = Vector3.Lerp(transform.position, target.transform.position + new Vector3(-cameraDistance, cameraDistance, -cameraDistance), 0.5f * Time.deltaTime);
        Camera.main.transform.LookAt(target.transform);

    }



	void checkForOcclusion() {

		//Cast a ray from this object's transform the the watch target's transform.
        RaycastHit[] hits = Physics.RaycastAll(
            Camera.main.transform.position,
            target.transform.position - Camera.main.transform.position,
            Vector3.Distance(target.transform.position, Camera.main.transform.position)
        );

		//Loop through all overlapping objects and disable their mesh renderer
        if(hits.Length == 0) return;

        foreach(RaycastHit hit in hits){

            // make sure we're not trying to hide the wrong object
        	if (hit.collider.name == "Persona" || hit.collider.name == "Ground") continue;

            // make sure this isn't the target
            if(hit.collider.gameObject.transform != target && hit.collider.transform.root != target) {

                // get pointers to all the child objects
                MeshRenderer[] meshRenderers = hit.collider.gameObject.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer meshRenderer in meshRenderers) {
                    // if we're already transparent, move on
                    if (meshRenderer.material.name == "SemiTransparent (Instance)") continue;
                    // don't affect the fonts
                    if (meshRenderer.material.name == "Font Material (Instance)") continue;
                    // remember this material (to turn it back on)
        			hiddenMaterials.Add(meshRenderer, meshRenderer.material);
        			meshRenderer.material = HiderMaterial;
    			} // foreach MeshRenderer

            } // if (hit.collider.gameObject.transform)
        } // foreach

	}



	void resetHiddenMaterials() {

		//reset and clear all the previous objects
        if(hiddenMaterials.Count > 0){
            foreach(MeshRenderer meshRenderer in hiddenMaterials.Keys){
            	meshRenderer.material = hiddenMaterials[meshRenderer];
            }
            hiddenMaterials.Clear();
        }

	}

}
