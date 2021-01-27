using UnityEngine;

public class objectRotation : MonoBehaviour
{
    public float speed = 1.0f; // Speed of the ball
    public float radius = 1.0f; // Relative radius of the ball
    public GameObject movingObjectType;

    private GameObject cubeObject;
    private GameObject movingObject;
    private bool created = false;

    void Start () 
    {
        
    }
 
    void Update()
    {
        // Checks for a created cube - gives centre for moving object
        cubeObject = GameObject.FindWithTag("mainCube"); 

        // Rotation code. Only runs after central cube has been created
        if (cubeObject != null){
            // Scale and location used in calculation
            var cubeScale = cubeObject.transform.localScale; 
            var cubeLocation = cubeObject.transform.position;
            // When cube is first found, create the moving object from given public variable
            if (!created)
            {
                movingObject = Instantiate(movingObjectType, cubeLocation, Quaternion.Euler(0, 0, 0));
                created = true;
            }
            // Moves the object around cube location over time using sin/cos functions
            movingObject.transform.position = cubeLocation + new Vector3(Mathf.Sin(Time.time*speed)*(cubeScale.x*radius),
                                        -(cubeScale.y/2),
                                        Mathf.Cos(Time.time*speed)*(cubeScale.x*radius));

            // Rotation implemented for car and other non-sphere objects where rotation is noticeable
            movingObject.transform.rotation = Quaternion.Euler(0, 90 + (((Time.time * speed)/(Mathf.PI*2))*360), 0);



        }
    }
}
