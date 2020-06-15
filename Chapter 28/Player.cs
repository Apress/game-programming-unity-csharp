using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    public Transform trans;

    [Header("X Bounds")]
    public float minimumX = -70;
    public float maximumX = 70;

    [Header("Y Bounds")]
    public float minimumY = 18;
    public float maximumY = 80;

    [Header("Z Bounds")]
    public float minimumZ = -130;
    public float maximumZ = 70;

    [Header("Movement")]
    [Tooltip("Distance traveled per second with the arrow keys.")]
    public float arrowKeySpeed = 80;

    [Tooltip("Multiplier for mouse drag movement.  A higher value will result in the camera moving a greater distance when the mouse is moved.")]
    public float mouseDragSensitivity = 2.8f;

    [Tooltip("Amount of smoothing applied to camera movement.  Should be a value between 0 and 1.")]
    [Range(0,.99f)]
    public float movementSmoothing = .75f;

    private Vector3 targetPosition;

    [Header("Scrolling")]
    [Tooltip("Amount of Y distance the camera moves per mouse scroll increment.")]
    public float scrollSensitivity = 1.6f;


    void ArrowKeyMovement()
    {
        //If up arrow is held,
        if (Input.GetKey(KeyCode.UpArrow))
        {
            //...add to target Z position:
            targetPosition.z += arrowKeySpeed * Time.deltaTime;
        }
        //Otherwise, if down arrow is held,
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //...subtract from target Z position:
            targetPosition.z -= arrowKeySpeed * Time.deltaTime;
        }

        //If right arrow is held,
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //..add to target X position:
            targetPosition.x += arrowKeySpeed * Time.deltaTime;
        }
        //Otherwise, if left arrow is held,
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            //...subtract from target X position:
            targetPosition.x -= arrowKeySpeed * Time.deltaTime;
        }
    }

    void MouseDragMovement()
    {
        //If the right mouse button is held,
        if (Input.GetMouseButton(1))
        {
            //Get the movement amount this frame:
            Vector3 movement = new Vector3(-Input.GetAxis("Mouse X"),0,-Input.GetAxis("Mouse Y")) * mouseDragSensitivity;

            //If there is any movement,
            if (movement != Vector3.zero)
            {
                //...apply it to the targetPosition:
                targetPosition += movement;
            }
        }
    }

    void Zooming()
    {
        //Get the scroll delta Y value and flip it:
        float scrollDelta = -Input.mouseScrollDelta.y;

        //If there was any delta,
        if (scrollDelta != 0)
        {
            //...apply it to the Y position:
            targetPosition.y += scrollDelta * scrollSensitivity;
        }
    }

    void MoveTowardsTarget()
    {
        //Clamp the target position to the bounds variables:
        targetPosition.x = Mathf.Clamp(targetPosition.x,minimumX,maximumX);
        targetPosition.y = Mathf.Clamp(targetPosition.y,minimumY,maximumY);
        targetPosition.z = Mathf.Clamp(targetPosition.z,minimumZ,maximumZ);

        //Move if we aren't already at the target position:
        if (trans.position != targetPosition)
        {
            trans.position = Vector3.Lerp(trans.position,targetPosition,1 - movementSmoothing);
        }
    }

    //Events:
    void Start()
    {
        targetPosition = trans.position;
    }


    void Update()
    {
        ArrowKeyMovement();

        MouseDragMovement();

        Zooming();

        MoveTowardsTarget();
    }
}
