using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class handles physics for the object
public class RigidBody4D : MonoBehaviour
{
    Mesh4 hc;
    GameObject mesh4_gameObject;
    public Vector4 force4;
    public Vector4 velocity4;
    public Vector4 linAccel4;
    public Vector4 position4;

    float[] angles4;
    float[] torques4;
    float[] angAccel4;

    //scaling factors for testing purposes, good feeling movement, and simulating realism
    public float LINEAR_SENSITIVITY = 0.1f;
    public float ROTATIONAL_SENSITIVITY = 0.1f;
    

    //initialize the variables
    public void initialize()
    {
        hc = GetComponent<Mesh4>(); //not sure about syntax


        force4 = new Vector4(0, 0, 0, 0);
        position4 = new Vector4(0, 0, 0, 0);
        velocity4 = new Vector4(0, 0, 0, 0);
        linAccel4 = new Vector4(0, 0, 0, 0);
        position4 = new Vector4(0, 0, 0, 0);
    }

    //teleports object to position
    public void setPosition4(float x, float y, float z, float w)
    {
        position4.x = x;
        position4.y = y;
        position4.z = z;
        position4.w = w;
    }

    //changes obj position by delta
    public void addPosition4(float deltax, float deltay, float deltaz, float deltaw)
    {
        position4.x += deltax;
        position4.y += deltay;
        position4.z += deltaz;
        position4.w += deltaw;
    }

    //sets obj velocity
    public void setVelocity4(float vx, float vy, float vz, float vw)
    {
        velocity4.x = vx;
        velocity4.y = vy;
        velocity4.z = vz;
        velocity4.w = vw;
    }

    //change object velocity by delta
    public void addVelocity(float deltavx, float deltavy, float deltavz, float deltavw)
    {
        velocity4.x += deltavx;
        velocity4.y += deltavy;
        velocity4.z += deltavz;
        velocity4.w += deltavw;
    }

    //set object force
    public void setForce4(float fx, float fy, float fz, float fw)
    {
        force4.x = fx;
        force4.y = fy;
        force4.z = fz;
        force4.w = fw;
    }

    //change obj force by delta
    public void addForce4(float fx, float fy, float fz, float fw)
    {
        force4.x += fx;
        force4.y += fy;
        force4.z += fz;
        force4.w += fw;
    }

    //use force mass and drag a scaling coefficent to calculate acceleration
    public void calcLinAccel4()
    {
        linAccel4.x = force4.x * LINEAR_SENSITIVITY / hc.mass;
        linAccel4.y = force4.y * LINEAR_SENSITIVITY / hc.mass;
        linAccel4.z = force4.z * LINEAR_SENSITIVITY / hc.mass;
        linAccel4.w = force4.w * LINEAR_SENSITIVITY / hc.mass;
    }

    //use acceleration to calculate velocity
    public void calcVelocity4()
    {
        velocity4.x += linAccel4.x * Time.deltaTime;
        velocity4.y += linAccel4.y * Time.deltaTime;
        velocity4.z += linAccel4.z * Time.deltaTime;
        velocity4.w += linAccel4.w * Time.deltaTime;
    }

    //calc position from velocity
    //MAYBE DIDIVE THIS BY TWO...? not sure
    public void calcPosition4()
    {
        position4.x += velocity4.x * Time.deltaTime;
        position4.y += velocity4.y * Time.deltaTime;
        position4.z += velocity4.z * Time.deltaTime;
        position4.w += velocity4.w * Time.deltaTime;
    }

    //UNSURE HOW TO IMPLEMENT ROTATION YET
    //not sure what kind of rot mat to use with the inertia tensor
    public void setAngle4()
    {
        
    }

    public void rotate4()
    {

    }

    public void addTorque4()
    {

    }

    public void calcRotAccel4()
    {

    }

    //updates acceleration, velocity, and position from forces
    public void updateFromForces4()
    {
        calcLinAccel4();
        calcVelocity4();
        calcPosition4();
    }

    public void updateFromTorques4()
    {

    }

    //push rigidbody position to mesh4
    public void updateObjPos()
    {
        hc.position4 = position4;
    }

    //push rotation of rigidbody to mesh4
    public void updateObjAng()
    {

    }

    //call every frame
    public void bodyUpdate()
    {
        updateFromForces4();
        updateFromTorques4();

        updateObjPos();
        updateObjAng();
    }
}
