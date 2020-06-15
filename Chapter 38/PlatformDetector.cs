using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    [Tooltip("Transform to move with the platform.")]
    public Transform trans;
    
    //The Transform of the platform we are currently standing on, if any:
    [HideInInspector] public Transform platform = null;

    //Position of the platform on the last Update.
    private Vector3 platformPreviousPosition;

    //True if we have set the position of the platform at least once since it was first set.
    //False if we have not yet set the position of the current platform.
    private bool firstPositionLogged = false;

    //Unity events:
    void FixedUpdate()
    {
        //If we are standing on a platform
        if (platform != null)
        {
            //If we have already logged the platform position at least once and it is not the same as its current position
            if (firstPositionLogged && platformPreviousPosition != platform.position)
            {
                //Add the change in platform position to our trans.position:
                trans.position += platform.position - platformPreviousPosition;
            }
            
            //Log the platform position this frame:
            platformPreviousPosition = platform.position;
            firstPositionLogged = true; //Mark that we have logged the position at least one
        }
        else //If we are not standing on a platform
        {
            //We'll mark that we have not set the platform's position yet.
            //When a new platform is assigned, we won't move the transform until this is set to 'true'.
            firstPositionLogged = false;
        }
    }
}