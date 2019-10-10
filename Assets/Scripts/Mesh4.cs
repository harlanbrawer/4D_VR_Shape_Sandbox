using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

public class Mesh4 : MonoBehaviour
{
    // Vertices: array of Vec4s that defines the points of the mesh
    // Don't ever update this unless you are changing the entire mesh!
    public Vector4[] vertices4D;

    // Projected Vertices
    // Change these whenever! Varies with the projection
    public Vector3[] vertices3D;

    // Array of ints. Each set of 3 indices corresponds to a single triangle, defined clockwise
    public int[] triangles;
    public int[] tets;

    // Array of ints. Each set of 2 indicies corresponds to a single edge. Order does not matter.
    public int[] edges;
    LineRenderer[] lineRenderer;
    GameObject[] lines;

    public float PROJECTION_DISTANCE = 2f;
    float stereoFactor = 1;

    //inerta
    public Matrix<float> tenInertia;
    public Matrix<float> pointApproxTenInertia;
    int POINT_MASS = 1;
    Dictionary<int, char> intTOxyzw;
    public float mass;

    //center of mass
    Vector4 centerOfMass;

    //position of origin (with respect to lines drawn)
    public Vector4 position4;

    //rotation
    public Mesh mesh3d;
    Matrix<float> fullRotMat;
    Matrix<float>[] vec4Mats;
    Matrix<float> rotatedVec4Mat;
    float[,] fullRotArr;
    float cos_ang_xy;
    float cos_ang_xz;
    float cos_ang_xw;
    float cos_ang_yz;
    float cos_ang_yw;
    float cos_ang_zw;

    float sin_ang_xy;
    float sin_ang_xz;
    float sin_ang_xw;
    float sin_ang_yz;
    float sin_ang_yw;
    float sin_ang_zw;

    




    // Normals: array of Vec4s that defines the normal direction of each triangle, in 4D

    // Note:
    // Potentially uneccessary because when projected they are possibly no longer normal, 
    // and they can be calculated in the 3D projection for rendering.
    // Their use is only for rendering for shaders, so since we won't render in 4D
    // they don't need to be calculated here? 
    // Will experiment more later

    //public Vector4[] normals;

    // UVs: used for texturing, gives each vector a texture coordinate. Might not use until later?

    //public Vector2[] uv;



    ////default does nothing
    //public Mesh4()
    //{

    //}



    //takes vector4 arrray
     public void initialize(Vector4[] verts, int[] ed)
     {   
        //initialize vertices
         vertices4D = new Vector4[verts.Length];
         for (int i = 0; i < verts.Length; i++)
         {
             vertices4D[i] = verts[i];
         }

        //initialize edges
        edges = new int[ed.Length];
        for (int i = 0; i < ed.Length; i++)
        {
            edges[i] = ed[i];
        }

        //identity rotation matrix and vector4 matrices
        fullRotMat = DenseMatrix.OfArray(new float[,] {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        });
        vec4Mats = new Matrix<float>[vertices4D.Length];
        for (int i = 0; i < vertices4D.Length; i++)
        {
            vec4Mats[i] = DenseMatrix.OfArray(new float[,] {
                {vertices4D[i].x},
                {vertices4D[i].y},
                {vertices4D[i].z},
                {vertices4D[i].w}
            });
        }
        //initializing matricies for use in other parts of class
        rotatedVec4Mat = DenseMatrix.OfArray(new float[,] {
            {0},
            {0},
            {0},
            {0}
        });
        fullRotArr = new float[,] {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        };

        //tets
        tets = new int[vertices4D.Length * 3]; //FIX THIS LENGTH, NOT SURE YET
        findTets();

        //tensor of inertia
        tenInertia = DenseMatrix.OfArray(new float[,] {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        });
        pointApproxTenInertia = DenseMatrix.OfArray(new float[,] {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        });

        //create dictionary to 
        intTOxyzw = new Dictionary<int, char>();
        intTOxyzw.Add(0, 'x');
        intTOxyzw.Add(1, 'y');
        intTOxyzw.Add(2, 'z');
        intTOxyzw.Add(3, 'w');

        //position starts at zero origin
        position4 = new Vector4(0, 0, 0, 0);

        //calculate mass
        setMass();

        //setTenInertia();
        setPointApproxTenInertia();

        //center of mass
        setPointApproxCenterOfMass();

        //set up mesh3d
        mesh3d = new Mesh();
        vertices3D = new Vector3[vertices4D.Length];

        //set up lines
        renderMesh4();
     }

    //takes int array where every4 is a vector4 to initialize the verticies
     public void initialize(float[] vertCords, int[] ed)
     {
        //initialize vertices
        int numVerts = vertCords.Length / 4;
        vertices4D = new Vector4[numVerts];
        for (int i = 0; i < numVerts; i++)
        {
            vertices4D[i] = new Vector4(vertCords[4 * i], vertCords[4 * i + 1], vertCords[4 * i + 2], vertCords[4 * i + 3]);
        }

        initialize(vertices4D, ed);
     }


    //USELESS OLD CODE FOR REFERENCE, TO BE DELETED
    //  Vector3 projectVec4_Old(Vector4 vec, float distance = 2f)
    //     {
    //         float w = 1 / (distance - vec.w);
    //         float[,] projectionArr = new float[,] {
    //             {w, 0, 0, 0},
    //             {0, w, 0, 0},
    //             {0, 0, w, 0}
    //         };
    //         return new Vector3 (vec.x * w, vec.y * w, vec.z * w);
    //     }

    // void updateVerticies3dByProjectVec4(ref Vector4[] rotatedVec4s, ref Vector3[] vec3s, ref float distance = 2f)
    //     {
    //         stereoFactor = 1 / (distance - vec.w);
    //         // float[,] projectionArr = new float[,] {
    //         //     {w, 0, 0, 0},
    //         //     {0, w, 0, 0},
    //         //     {0, 0, w, 0}
    //         // };
            
    //         for (int i = 0; i < vec4s.Length; i++)
    //         {
                
    //         }

    //         return new Vector3 (vec.x * stereoFactor, vec.y * stereoFactor, vec.z * stereoFactor);
    //     }

    //  public void ProjectTo3D_Old()
    //  {
    //     //stereographic projection
    //     mesh3d.Clear();
    //     vertices3D = new Vector3[vertices4D.Length];
        
    //     for (int i = 0; i < vertices4D.Length; i++)
    //     {
    //         vertices3D[i] = projectVec4(vertices4D[i], PROJECTION_DISTANCE);
    //     }

    //     mesh3d.vertices = vertices3D;

    //     //edge indexes remain the same
    //     //print(mesh3d.vertices[4]);

    //     //not sure how to handle tris..?

    //  }

     //updates the rotation matrix to the current rotation angles
     //when implementing torque may have to use different system that uses change instead or something else
     public void updateRotMat(float ang_xy = 0, float ang_yz = 0, float ang_xz = 0, float ang_xw = 0, float ang_yw = 0, float ang_zw = 0)
     {
        //rotation
        cos_ang_xy = Mathf.Cos(ang_xy);
        cos_ang_xz = Mathf.Cos(ang_xz);
        cos_ang_xw = Mathf.Cos(ang_xw);
        cos_ang_yz = Mathf.Cos(ang_yz);
        cos_ang_yw = Mathf.Cos(ang_yw);
        cos_ang_zw = Mathf.Cos(ang_zw);

        sin_ang_xy = Mathf.Sin(ang_xy);
        sin_ang_xz = Mathf.Sin(ang_xz);
        sin_ang_xw = Mathf.Sin(ang_xw);
        sin_ang_yz = Mathf.Sin(ang_yz);
        sin_ang_yw = Mathf.Sin(ang_yw);
        sin_ang_zw = Mathf.Sin(ang_zw);

        //rotates the supplied vecter4 by the given angle pairs. Each pair represnets a "2 coordinate rotation"
        //which changes the two coordinates at the same time as if it were being rotated with respect to an axis
        //that only affects those two coordinates.
        //This rotation was invented by extrapolating a 3d rotation matrix to 4d
        //what is show below in fullRotArr is an array representation of all 6 rotation matricies multiplied together.
        //this was done so that the matrix multiplication would not have to be done every frame, thus speeding up the rotation calculation.
        fullRotArr = new float[4, 4] //NEED TO FIX THIS SO IT DOESNT NEED TO USE THE NEW KEYWORK B/C THAT CREATES GARBGE
        {
            {cos_ang_xw * (cos_ang_xy * cos_ang_xz +sin_ang_xy * sin_ang_xz * sin_ang_yz), -cos_ang_yw * cos_ang_yz * sin_ang_xy -sin_ang_xw
            * sin_ang_yw * (cos_ang_xy * cos_ang_xz + sin_ang_xy * sin_ang_xz * sin_ang_yz), cos_ang_zw * (-cos_ang_xy * sin_ang_xz +
            cos_ang_xz * sin_ang_xy * sin_ang_yz) + (cos_ang_yz * sin_ang_xy * sin_ang_yw - cos_ang_yw * sin_ang_xw * (cos_ang_xy *
            cos_ang_xz +sin_ang_xy * sin_ang_xz * sin_ang_yz)) * sin_ang_zw, cos_ang_zw * (cos_ang_yz * sin_ang_xy * sin_ang_yw -
            cos_ang_yw * sin_ang_xw * (cos_ang_xy * cos_ang_xz +sin_ang_xy * sin_ang_xz * sin_ang_yz)) + (cos_ang_xy * sin_ang_xz -
            cos_ang_xz * sin_ang_xy * sin_ang_yz) * sin_ang_zw},

            {cos_ang_xw * (cos_ang_xz * sin_ang_xy -cos_ang_xy * sin_ang_xz *
            sin_ang_yz), -cos_ang_xz * sin_ang_xw * sin_ang_xy * sin_ang_yw +cos_ang_xy * (cos_ang_yw * cos_ang_yz + sin_ang_xw *
            sin_ang_xz * sin_ang_yw * sin_ang_yz), -cos_ang_zw * (sin_ang_xy * sin_ang_xz + cos_ang_xy * cos_ang_xz * sin_ang_yz) -
            (cos_ang_xz * cos_ang_yw * sin_ang_xw * sin_ang_xy + cos_ang_xy * (cos_ang_yz * sin_ang_yw -cos_ang_yw * sin_ang_xw *
            sin_ang_xz * sin_ang_yz)) * sin_ang_zw, cos_ang_xy * cos_ang_zw * (-cos_ang_yz * sin_ang_yw + cos_ang_yw * sin_ang_xw *
            sin_ang_xz * sin_ang_yz) +sin_ang_xy * sin_ang_xz * sin_ang_zw +cos_ang_xz * (-cos_ang_yw * cos_ang_zw * sin_ang_xw *
            sin_ang_xy + cos_ang_xy * sin_ang_yz * sin_ang_zw)},

            {cos_ang_xw * cos_ang_yz * sin_ang_xz, -cos_ang_yz * sin_ang_xw *
            sin_ang_xz * sin_ang_yw +cos_ang_yw * sin_ang_yz, cos_ang_xz * cos_ang_yz * cos_ang_zw - (cos_ang_yw * cos_ang_yz *
            sin_ang_xw * sin_ang_xz + sin_ang_yw * sin_ang_yz) * sin_ang_zw, -cos_ang_yw * cos_ang_yz * cos_ang_zw * sin_ang_xw *
            sin_ang_xz -cos_ang_zw * sin_ang_yw * sin_ang_yz -cos_ang_xz * cos_ang_yz * sin_ang_zw},
            
            {sin_ang_xw, cos_ang_xw * sin_ang_yw, cos_ang_xw * cos_ang_yw * sin_ang_zw, cos_ang_xw * cos_ang_yw * cos_ang_zw}
        };

        //create matrix from array
        fullRotMat = DenseMatrix.OfArray(fullRotArr);
     }


     //applies rotation to the 4vectors that describe the 4vertices
     //then uses stereographic projection to project 4vector vertices into 3space
     public void stereoProjectRotationTo3D(float distance = 2f)
     {
        //stereographic projection
        //mesh3d.Clear(); //DO WE NEED THIS?????


        //rotate tensor of inertia
        tenInertia = fullRotMat.Multiply(tenInertia);
        
        for (int i = 0; i < vertices4D.Length; i++)
        {
            //rotatedVec4 is buffer
            rotatedVec4Mat = fullRotMat.Multiply(vec4Mats[i]);

            //stereographic projection factor
            stereoFactor = 1 / (distance - rotatedVec4Mat[3,0]);

            vertices3D[i].x = rotatedVec4Mat[0,0] * stereoFactor + position4.x;
            vertices3D[i].y = rotatedVec4Mat[1,0] * stereoFactor + position4.y;
            vertices3D[i].z = rotatedVec4Mat[2,0] * stereoFactor + position4.z;
        }

        mesh3d.vertices = vertices3D;

        //edge indexes remain the same
        //print(mesh3d.vertices[4]);

        //not sure how to handle tris...

     }


    //call this in start funtion
    //initializes components of how the mesh4 lines are rendered
    public void renderMesh4(float lineWidth = 0.01f)
    {
        int numLines = edges.Length / 2;

        //initialize lines and renderers
        lineRenderer = new LineRenderer[numLines];
        lines = new GameObject[numLines];

        //create line gameobjects
        for (int i = 0; i < numLines; i++)
        {
            lines[i] = new GameObject();
            lines[i].transform.parent = transform;
        }

        //initialize lineRenderer properties
        for (int i = 0; i < numLines; i++)
        {
            lineRenderer[i] = lines[i].AddComponent<LineRenderer>();
            lineRenderer[i].material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer[i].widthMultiplier = lineWidth;
            lineRenderer[i].positionCount = 2;
            //lineRenderer[i].startColor = Color.red;
            //lineRenderer[i].endColor = Color.blue;
        }
    }

    //call this in update
    //actually draws the lines between 3vertices to the screen
    public void drawLines()
    {
        //draw lines between vertices in 3d
        for (int i = 0; i < edges.Length / 2; i++)
        {
            lineRenderer[i].SetPosition(0, mesh3d.vertices[edges[i * 2]]);
            lineRenderer[i].SetPosition(1, mesh3d.vertices[edges[(i * 2) + 1]]);
        }
    }

    //OLD UNUSED. 
    //example of how could use helper class but this was included in the stereographic projection function
    public void rotateBaseVec4(float ang_xy = 0, float ang_yz = 0, float ang_xz = 0, float ang_xw = 0, float ang_yw = 0, float ang_zw = 0)
    {
        //rotate each vertex
        for (int i = 0; i < vertices4D.Length; i++)
        {
            Helpers.rotate(ref vertices4D[i], ang_xy, ang_yz, ang_xz, ang_xw, ang_yw, ang_zw);
        }
    }

    //DOESNT WORK
    //attemping to find a way to get an array of the outer facing tetrahedrons by using the sides and verticies
    //this might not be possible but it would be nice for the user to not have to input the outer faces
    public void findTets() //DOESNT WORK
    {
        int numTetVerts = 0;

        //find tets
        for (int i = 0; i < vertices4D.Length; i++)
        {
            for (int j = i; j < vertices4D.Length; j++)
            {
                for (int k = j; k < vertices4D.Length; k++)
                {
                    for (int l = k; l < vertices4D.Length; l++)
                    {
                        int connectionCount = 0;

                        for (int n = 0; n < vertices4D.Length; n++)
                        {
                            if ( (edges[2 * n] == i || edges[2 * n] == j || edges[2 * n] == k || edges[2 * n] == l) && 
                                 (edges[2 * n + 1] == i || edges[2 * n + 1] == j || edges[2 * n + 1] == k || edges[2 * n + 1] == l) )
                            {
                                connectionCount++;
                            }
                        }

                        if (connectionCount == 6)
                        {
                            tets[numTetVerts] = i;
                            tets[numTetVerts + 1] = j;
                            tets[numTetVerts + 2] = k;
                            tets[numTetVerts + 3] = l;
                            numTetVerts += 4;
                        }
                    }
                }
            }
        }
    }

    //algorithm for determining center of mass based on shape
    //sets the center of mass variable
    public void setCenterOfMass()
    {

    }

    //very roughly apporximates center of mass by taking each vertex as a bit of mass
    //sets center of mass variable
    public void setPointApproxCenterOfMass()
    {
        //massdist
        float xMassDist = 0;
        float yMassDist = 0;
        float zMassDist = 0;
        float wMassDist = 0;

        for (int i = 0; i < vertices4D.Length; i++)
        {
            xMassDist += vertices4D[i].x * POINT_MASS;
            yMassDist += vertices4D[i].y * POINT_MASS;
            zMassDist += vertices4D[i].z * POINT_MASS;
            wMassDist += vertices4D[i].w * POINT_MASS;
        }

        //total mass //change this to be for lines instead later
        float totalMass = vertices4D.Length * POINT_MASS;

        //calculation of COM
        centerOfMass = new Vector4(xMassDist / totalMass, yMassDist / totalMass, zMassDist / totalMass, wMassDist / totalMass);

    }


    //ATTEMPS AT FINDING THE VOLUME OF A 4SHAPE FROM THE VERTICES
    // public float SignedVolumeOfTets(Vector4 p1, Vector4 p2, Vector4 p3, Vector4 p4)
    // {
    //     float v3214 = p3.x * p2.y * p1.z * p4.w;
    //     float v2314 = p2.x * p3.y * p1.z * p4.w;
    //     float v3124 = p3.x * p1.y * p2.z * p4.w;
    //     float v1324 = p1.x * p3.y * p2.z * p4.w;
    //     float v2134 = p2.x * p1.y * p3.z * p4.w;
    //     float v1234 = p1.x * p2.y * p3.z * p4.w;

    //     return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    // }

    // public float VolumeOfMesh(Mesh mesh)
    // {
    //     float volume = 0;
    //     Vector3[] vertices = mesh.vertices;
    //     int[] triangles = mesh.triangles;
    //     for (int i = 0; i &lt; mesh.triangles.Length; i += 3)
    //     {
    //         Vector3 p1 = vertices[triangles[i + 0]];
    //         Vector3 p2 = vertices[triangles[i + 1]];
    //         Vector3 p3 = vertices[triangles[i + 2]];
    //         volume += SignedVolumeOfTriangle(p1, p2, p3);
    //     }
    //     return Mathf.Abs(volume);
    // }

    //sets mass based on volume and density
    //APPROXIMATED for now
    public void setMass()
    {
        //just point approximating right now
        mass = POINT_MASS * vertices4D.Length;
    }

    //set the tensor of interia variable using an algorithm
    //NOT SURE HOW TO DO YET
    public void setTenInertia()
    {

    }

    //approximating tensor of inertia treating each vertex as a point mass
    //sets tensor of inertia variable
    public void setPointApproxTenInertia()
    {
        for (int i = 0; i < vertices4D.Length; i++)
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    pointApproxTenInertia[row,col] = POINT_MASS * ( (vertices4D[i].x * vertices4D[i].x + vertices4D[i].y * vertices4D[i].y + 
                        vertices4D[i].z * vertices4D[i].z + vertices4D[i].w * vertices4D[i].w) 
                        - (vertices4D[i][row] * vertices4D[i][col]) );
                }
            }
        }
    }
 
}
