using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class createAtTouchLocation : MonoBehaviour
{
    public GameObject marker1;
    public GameObject marker2;
    public GameObject boxPlane;
    public GameObject occlusionBox;
    public bool useOcclusionBox;
    public ARRaycastManager raycastManager;

    private int objectTracker;

    private Pose placementPose;
    private Pose savedPose1;
    private Pose savedPose2;
    private GameObject savedObject1;
    private GameObject savedObject2;
    private GameObject planeObject;
    private GameObject occlusionObject;
    private float planeScale;
    private float savedYPos;
    private Vector2 startPos;
    private Vector2 direction;


    // Start is called before the first frame update
    void Start()
    {
        objectTracker = 1;
    }

    void Update()
    {
        Touch touch = Input.GetTouch(0);
        // Code run when screen tapped
        if (Input.touchCount > 0)
        {
            // Raycasting to look for planes at touch location
            var hits = new List<ARRaycastHit>();
            raycastManager.Raycast(touch.position, hits, TrackableType.Planes);
            // If tap is on a plane (decected by raycast)
            if (hits.Count > 0)
            {
                placementPose = hits[0].pose; //Chose first hit
                // Determining the rotation based on the direction from the camera for placement markers, not particularly necessary as they are spheres but needs a value to be given and useful in case marker shape changed
                var cameraForward = Camera.main.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            
                // Runs code for marker placement and eventual box creation. Places at end of tap so code isn't running during tap for clarity
                if (objectTracker < 3) {
                    if (touch.phase == TouchPhase.Ended){ 
                        // Placing first marker
                        if (objectTracker == 1)
                        {
                            savedObject1 = Instantiate(marker1, placementPose.position, placementPose.rotation);
                            savedPose1 = placementPose;
                            objectTracker = 2;
                        }
                        // Second marker and box creation
                        else if (objectTracker == 2)
                        {
                            savedObject2 = Instantiate(marker2, placementPose.position, placementPose.rotation);
                            savedPose2 = placementPose;
                            


                            // Takes vector between two points and inverses to get new rotation vector where opposing corners point towards spheres
                            var pointDifferenceAngle = new Vector3(-(savedObject1.transform.position.x- savedObject2.transform.position.x)
                                ,0,-(savedObject1.transform.position.z- savedObject2.transform.position.z));
                            placementPose.rotation = Quaternion.LookRotation(pointDifferenceAngle, Vector3.up);
                            placementPose.rotation *= Quaternion.Euler(Vector3.up * 45);
                            placementPose.position = (savedObject1.transform.position + savedObject2.transform.position) /2;
                            
                            // Creates box object and scales to correct size where markers are at the coreners
                            planeObject = Instantiate(boxPlane, placementPose.position, placementPose.rotation);
                            var longSide = Mathf.Sqrt(Mathf.Pow(savedObject1.transform.position.x - savedObject2.transform.position.x, 2)
                                + Mathf.Pow(savedObject1.transform.position.z - savedObject2.transform.position.z, 2));
                            planeScale = Mathf.Sqrt(0.5f*Mathf.Pow(longSide,2));
                            planeObject.transform.localScale = new Vector3(planeScale, 0.01f, planeScale);
                            savedYPos = placementPose.position.y;


                            planeObject.tag = "mainCube";



                            if (useOcclusionBox){
                                occlusionObject = Instantiate(occlusionBox, placementPose.position, placementPose.rotation);

                                updateOcclusionBox();  //Makes selection box occlude
                                
                            } else // Make box invisible
                            {
                                planeObject.GetComponent<Renderer>().enabled = false;
                            }

                            // Removes placement markers
                            savedObject1.SetActive(false);
                            savedObject2.SetActive(false);
                            objectTracker = 3;
                        }
                    }

                // Code to detect swipe
                } else if (objectTracker == 4) {
                    // Get swipe start position as swipe begins
                    if (touch.phase == TouchPhase.Began){
                        startPos = touch.position;
                    }  
                    // Adjust cube height as swipe progresses, relative to start position
                    else if (touch.phase == TouchPhase.Moved) {
                        direction = touch.position - startPos;
                        var newScale = 0.005f*direction.y;
                        if (newScale < 0.005f) {
                            newScale = 0.005f;
                        }
                        planeObject.transform.localScale = new Vector3(planeScale, newScale, planeScale);
                        planeObject.transform.position = new Vector3(planeObject.transform.position.x,
                                                                    savedYPos + (0.5f*newScale),
                                                                    planeObject.transform.position.z);                    
                        
                    }

                }
            }
        }
        updateOcclusionBox();
    }

    // Makes actual occlusion mesh inside occlusion box but very slightly smaller
    private void updateOcclusionBox() {
        occlusionObject.transform.localScale = new Vector3(planeObject.transform.localScale.x-0.0001f,
                                                        planeObject.transform.localScale.y-0.0001f,
                                                        planeObject.transform.localScale.z-0.0001f);
        occlusionObject.transform.position = planeObject.transform.position;
        occlusionObject.transform.rotation = planeObject.transform.rotation;
    }


}