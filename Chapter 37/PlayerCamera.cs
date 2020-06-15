using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //References:
    [Header("References")]
    [Tooltip("The base Transform of the player, which should never rotate.")]
    public Transform playerBaseTrans;

    [Tooltip("Set this to the Transform that has the Camera component (which should also have the PlayerCamera component).")]
    public Transform trans;

    [Tooltip("Reference to the Camera X Target Transform.")]
    public Transform cameraXTarget;

    [Tooltip("Reference to the Camera Y Target Transform.")]
    public Transform cameraYTarget; 

    [Tooltip("The Transform holding the model.  This is what will rotate on the Y axis to turn left/right as the camera turns.")]
    public Transform modelHolder;
    
    //Movement and Positioning:
    [Header("Movement and Positioning")]
    [Tooltip("How quickly the camera turns.  This is a multiplier for how much of the mouse input applies to the rotation (in degrees).")]
    public float rotationSpeed = 2.5f;

    [Tooltip("The amount of smoothing applied to the third-person camera.  A higher value will cause the camera to more gradually turn when the mouse is moved.")]
    [Range(0,.99f)]
    public float thirdPersonSmoothing = .25f;

    [Tooltip("The third-person camera will be kept this many units off of walls it touches.  Setting this higher can help prevent the camera from clipping with bumpy walls.")]
    public float wallMargin = .5f;

    [Tooltip("The amount of smoothing applied to the first-person camera.  A higher value will cause the camera to more gradually turn when the mouse is moved.")]
    [Range(0,.99f)]
    public float firstPersonSmoothing = .8f;

    [Tooltip("Position, local to the Model Holder, for the camera to use when in first-person mode.")]
    public Vector3 firstPersonLocalPosition = new Vector3(0,5.4f,0);

    [Tooltip("Position, local to the Model Holder, for the camera to orbit around when in third-person mode.")]
    public Vector3 thirdPersonLocalOrbitPosition = new Vector3(0,5.4f,0);

    //Bounds:
    [Header("Bounds")]
    [Tooltip("Minimum distance from the for the third person camera to have from its orbit point.")]
    public float minThirdPersonDistance = 5;
    
    [Tooltip("Maximum distance from the for the third person camera to have from its orbit point.")]
    public float maxThirdPersonDistance = 42;

    [Tooltip("Resembles the current third person distance.  Set this to whatever you want the initial distance value to be.")]
    public float thirdPersonDistance = 28;

    [Tooltip("Multiplier for scroll wheel movement.  A higher value will result in a greater change in third-person distance when scrolling the mouse wheel.")]
    public float scrollSensitivity = 8;

    [Tooltip("X euler angles for the camera when it is looking as far down as it can.")]
    public int xLookingDown = 65;

    [Tooltip("Y euler angles for the camera when it is looking as far up as it can.")]
    public int xLookingUp = 310;

    [Header("Misc")]
    [Tooltip("The layer mask for what the third-person camera will be obstructed by, and what it will ignore and pass through.  You'll probably want this to include environmental objects, but not smaller entities.")]
    public LayerMask thirdPersonRayLayermask;

    [Tooltip("The key to press to change from first-person to third-person, or vice versa.")]
    public KeyCode modeToggleHotkey = KeyCode.C;

    [Tooltip("The key to hold down to hold the camera still, unlock the mouse cursor, and allow mouse movement.")]
    public KeyCode mouseCursorShowHotkey = KeyCode.V;

    [Tooltip("Is the camera currently in first-person mode (true) or third-person mode (false)?  This can be set to determine the default mode when the game starts.")]
    public bool firstPerson = true;

    //Is the mouse cursor currently showing?  Toggled on by holding the mouseCursorShowHotkey.
    private bool showingMouseCursor = false;

    //Target position for the third-person camera.
    private Vector3 thirdPersonTargetPosition;

    //Gets the third-person camera orbit point in world space.
    private Vector3 OrbitPoint
    {
        get
        {
            return modelHolder.TransformPoint(thirdPersonLocalOrbitPosition);
        }
    }

    //Gets the rotation of both the X and Y Camera Targets together.
    private Quaternion TargetRotation
    {
        get
        {
            //Construct a new rotation out of Euler angles, using the rotation of the X target and Y target together:
            return Quaternion.Euler(cameraXTarget.eulerAngles.x,cameraYTarget.eulerAngles.y,0);
        }
    }

    //Gets a direction pointing forward along the TargetRotation.
    private Vector3 TargetForwardDirection
    {
        get
        {
            //Return the forward axis of the TargetRotation:
            return TargetRotation * Vector3.forward;
        }
    } 

    //Methods:
    void SetMouseShowing(bool value)
    {
        //Enable or disable the cursor visibility:
        Cursor.visible = value;
        showingMouseCursor = value;

        //Set the cursor lock state based on 'value':
        if (value)
            Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;
    }

    void Hotkeys()
    {
        //Toggling first and third-person mode:
        if (Input.GetKeyDown(modeToggleHotkey))
        {
            firstPerson = !firstPerson;
        }

        //Toggling mouse mode:
        if (Input.GetKeyDown(mouseCursorShowHotkey)) //Whenever the mouse cursor hotkey is pressed
            SetMouseShowing(true); //Show the mouse
        
        if (Input.GetKeyUp(mouseCursorShowHotkey)) //Whenever the mouse cursor hotkey is let go of
            SetMouseShowing(false); //Don't show the mouse

        //Scroll wheel for third-person distance:
        if (!firstPerson) //Only check for it while we're in third-person mode
        {
            //Get scroll wheel delta this frame:
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            
            //Subtract delta from thirdPersonDistance, multiplying it by the scroll sensitivity:
            thirdPersonDistance = Mathf.Clamp(thirdPersonDistance - scrollDelta * scrollSensitivity,minThirdPersonDistance,maxThirdPersonDistance);
        }
    }

    void UpdateTargetRotation()
    {
        //The X rotation we should be receiving uses the mouse Y because rotating along the X axis makes us look up/down.
        float xRotation = (Input.GetAxis("Mouse Y") * -rotationSpeed);

        //The Y rotation we should be receiving uses the mouse X because the rotating along the Y axis makes us look left/right.
        float yRotation = (Input.GetAxis("Mouse X") * rotationSpeed);
        
        //Apply the rotation to the camera:
        cameraXTarget.Rotate(xRotation,0,0);
        cameraYTarget.Rotate(0,yRotation,0);

        //We'll keep the camera target's X eulerAngles between the xLookingUp and xLookingDown values.
        //This prevents the camera from looking too far up or down.
        if (cameraXTarget.localEulerAngles.x >= 180) //If the X rotation is anywhere between 180 and 360
        {
            if (cameraXTarget.localEulerAngles.x < xLookingUp) //and it's less than the looking up value,
                // set it to the xLookingUp:
                cameraXTarget.localEulerAngles = new Vector3(xLookingUp,0,0);
        }
        else //If the X rotation is anywhere between 0 and 180
        {
            if (cameraXTarget.localEulerAngles.x > xLookingDown) //and it's past the x looking down value,
                // set it to the xLookingDown:
                cameraXTarget.localEulerAngles = new Vector3(xLookingDown,0,0);
        }
    }

    void FirstPerson()
    {
        //If the camera isn't in its first-person location, move it there:
        Vector3 targetWorldPosition = modelHolder.TransformPoint(firstPersonLocalPosition);

        if (trans.position != targetWorldPosition) //If the camera isn't at the first person camera location yet
        {
            //Lerp the camera transform towards the first person camera location:
            trans.position = Vector3.Lerp(trans.position,targetWorldPosition,.2f);
        }

        //Get the rotation of the camera, slerped towards the target rotation:
        Quaternion targetRotation = Quaternion.Slerp(trans.rotation, TargetRotation,1.0f - firstPersonSmoothing);

        //Apply just the X and Y axes to the camera:
        trans.eulerAngles = new Vector3(targetRotation.eulerAngles.x,targetRotation.eulerAngles.y,0);

        //Make the model face the same direction way as the camera, but with the Y axis removed from the direction:
        modelHolder.forward = new Vector3(trans.forward.x,0,trans.forward.z);
    }

    void ThirdPerson()
    {
        //We'll calculate the third-person target position by casting a ray backwards from the orbit point.
        //Make a new ray at the position of the orbit point, pointing directly backwards from the camera target:
        Ray ray = new Ray(OrbitPoint, -TargetForwardDirection);
        RaycastHit hit;

        //Cast the ray using thirdPersonDistance plus the wall margin to account for walls just outside the distance:
        if (Physics.Raycast(ray,out hit,thirdPersonDistance + wallMargin,thirdPersonRayLayermask.value))
        {
            //If the ray hits something, set the target position to the hit point:
            thirdPersonTargetPosition = hit.point;
            
            //We'll offset it back towards the cameraTarget by 'wallMargin' distance:
            thirdPersonTargetPosition += (TargetForwardDirection * wallMargin);
        }
        else //If the ray didn't hit anything
        {
            //Set the target position to 'distance' units directly behind the camera target
            thirdPersonTargetPosition = OrbitPoint - (TargetForwardDirection * thirdPersonDistance);
        }

        //Lerp the camera towards the target position using our smoothing settings:
        trans.position = Vector3.Lerp(trans.position,thirdPersonTargetPosition,1.0f - thirdPersonSmoothing);
        
        //Now that the camera has been moved properly, make it look at the orbit point:
        trans.forward = (OrbitPoint - trans.position).normalized;

        //Make the model face the same direction as the camera, with no Y axis position influence:
        modelHolder.forward = new Vector3(trans.forward.x,0,trans.forward.z);
    }

    //Unity events:
    void Start()
    {
        //By default, don't show the mouse:
        SetMouseShowing(false); 
    }

    void Update()
    {
        //Process hotkeys:
        Hotkeys();
    }

    //LateUpdate occurs after all Update calls for this script and all others.
    void LateUpdate()
    {
        //Update camera target rotation, so long as we're not showing the mouse cursor:
        if (!showingMouseCursor)
            UpdateTargetRotation();
        
        //Perform positioning logic based on the mode we're in:
        if (firstPerson)
            FirstPerson();
        else
            ThirdPerson();
    }
}