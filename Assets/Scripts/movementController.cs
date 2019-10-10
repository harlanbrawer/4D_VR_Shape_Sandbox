using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class handles input from the player and gives it to the rigidbody4d component
public class movementController : MonoBehaviour
{
    GameObject mesh4_gameObject;
    RigidBody4D rb4;
    public float DRAG_COEFFICENT = 20f;
    public float SPEED = 600f;

    // Start is called before the first frame update
    public void initialize()
    {
        //OLD AND UNUSED
        // mesh4_gameObject = this.gameObject.transform.GetChild(0).gameObject;
        // rb4 = mesh4_gameObject.GetComponent<RigidBody4D>(); //not sure about syntax

        //creates the rigidbody4d component
        rb4 = GetComponent<RigidBody4D>();
    }

    // Update is called once per frame
    public void controllerUpdate()
    {
        //use wasdfc for translational movement
        //getting player input for translational movement
        rb4.setForce4(
            (System.Convert.ToSingle(Input.GetKey("d")) - System.Convert.ToSingle(Input.GetKey("a"))) * SPEED,
            (System.Convert.ToSingle(Input.GetKey("w")) - System.Convert.ToSingle(Input.GetKey("s"))) * SPEED,
            (System.Convert.ToSingle(Input.GetKey("f")) - System.Convert.ToSingle(Input.GetKey("c"))) * SPEED,
            (System.Convert.ToSingle(Input.GetKey("e")) - System.Convert.ToSingle(Input.GetKey("q"))) * SPEED
        );

        //adding drag effects
        rb4.addForce4(
            -rb4.velocity4.x * DRAG_COEFFICENT * DRAG_COEFFICENT, 
            -rb4.velocity4.y * DRAG_COEFFICENT * DRAG_COEFFICENT, 
            -rb4.velocity4.z * DRAG_COEFFICENT * DRAG_COEFFICENT, 
            -rb4.velocity4.w * DRAG_COEFFICENT * DRAG_COEFFICENT
        );

        //pushes changes to the rigidbody4d
        rb4.bodyUpdate();
    }
}
