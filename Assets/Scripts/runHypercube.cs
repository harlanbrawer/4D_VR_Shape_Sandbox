using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//class that runs everything on the highest level
public class runHypercube : MonoBehaviour
{
    Mesh4 hc;
    movementController mcon;
    RigidBody4D rb4;
    public const float ROTATION_SPEED = 1.0f;
    public const float PROJECTION_DISTANCE = 2.0f;
    float rotPosition = 0;
    // Start is called before the first frame update
    void Start()
    {
        //input coordinates of verticies of 4 shape
        float[] vertexCoords = 
        {
            -1.0f, -1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f, -1.0f,
            -1.0f, 1.0f, -1.0f, -1.0f,
            1.0f, 1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f, 1.0f, -1.0f,
            1.0f, -1.0f, 1.0f, -1.0f,
            -1.0f, 1.0f, 1.0f, -1.0f,
            1.0f, 1.0f, 1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f, 1.0f,
            1.0f, -1.0f, -1.0f, 1.0f,
            -1.0f, 1.0f, -1.0f, 1.0f,
            1.0f, 1.0f, -1.0f, 1.0f,
            -1.0f, -1.0f, 1.0f, 1.0f,
            1.0f, -1.0f, 1.0f, 1.0f,
            -1.0f, 1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f, 1.0f
        };

        //every pair shows indexes in vertexCoords that are connected by line.
        int[] edgeIndices = {
            0, 1,
            0, 2,
            0, 4,
            0, 8,
            1, 3,
            1, 5,
            1, 9,
            2, 3,
            2, 6,
            2, 10,
            3, 7,
            3, 11,
            4, 5,
            4, 6,
            4, 12,
            5, 7,
            5, 13,
            6, 7,
            6, 14,
            7, 15,
            8, 9,
            8, 10,
            8, 12,
            9, 11,
            9, 13,
            10, 11,
            10, 14,
            11, 15,
            12, 13,
            12, 14,
            13, 15,
            14, 15
        };

        //IN PROGRESS
        //used to darw faces for rendering
        int[] faceIndices = {
            0,2,1,
            1,2,3


        };


        //creating game objects and their components
        //hc = new Mesh4(vertexCoords, edgeIndices);
        GameObject mesh4_Object = new GameObject();
        mesh4_Object.transform.parent = transform; 
        hc = mesh4_Object.AddComponent<Mesh4>();
        rb4 = mesh4_Object.AddComponent<RigidBody4D>(); //Deek?
        mcon = mesh4_Object.AddComponent<movementController>();

        //calling each object or components initialization function
        hc.initialize(vertexCoords, edgeIndices);
        hc.triangles = faceIndices;
        rb4.initialize();
        mcon.initialize();


    }

    // Update is called once per frame
    //call each component/objects update function
    void Update()
    {
        //using rigidbody component to update the mesh4 position
        mcon.controllerUpdate();

        //NOT FUNCTIONAL
        //update rotation
        rotPosition = rotPosition + ROTATION_SPEED * Time.deltaTime;

        //update rotation matrix
        hc.updateRotMat(0, 0, 0, 0, 0, rotPosition); //probably should clean up this function to just take a rotation class

        //update 3d mesh with rotation and projection
        hc.stereoProjectRotationTo3D(PROJECTION_DISTANCE);

        //update lines
        hc.drawLines();
    }
}
