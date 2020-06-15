using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [Tooltip("Should the force field affect the player?")]
    public bool affectsPlayer = true;

    [Tooltip("Should the force field affect Rigidbodies?")]
    public bool affectsRigidbodies = true;

    [Tooltip("Method of applying force.")]
    public ForceMode forceMode;

    [Tooltip("Amount of force applied.")]
    public Vector3 force;

    [Tooltip("Should the force be applied in world space or local space relative to this Transform's facing?")]
    public Space forceSpace = Space.World;

    //Gets the force in world space.
    public Vector3 ForceInWorldSpace
    {
        get
        {
            //If it's world-space we can just return 'force' as-is:
            if (forceSpace == Space.World)
                return force;
            
            //If it's local space, we use our transform to convert 'force' from local to world space:
            else
                return transform.TransformDirection(force);
        }
    }

    //Methods:
    void OnColliderTouched(Collider other)
    {
        //If we affect the player,
        if (affectsPlayer)
        {
            // check for a Player component on the other collider's GameObject:
            var player = other.GetComponent<Player>();
            
            //If we found one, call AddVelocity:
            if (player != null)
            {
                //If the force mode is a constant push mode, use Time.deltaTime to make the force "per second".
                if (forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration)
                    player.AddVelocity(ForceInWorldSpace * Time.deltaTime);
                else //Otherwise, use the force as-is.
                    player.AddVelocity(ForceInWorldSpace);
            }
        }

        //If we affect Rigidbodies,
        if (affectsRigidbodies)
        {
            // check for a Rigidbody component on the other collider's GameObject:
            var rb = other.GetComponent<Rigidbody>();
            
            //If we found one, call AddForce:
            if (rb != null)
                rb.AddForce(ForceInWorldSpace,forceMode);
        }
    }

    //Unity events:
    void OnTriggerEnter(Collider other)
    {
        //Impulse and VelocityChange modes will apply force only when the trigger is first entered.
        if (forceMode == ForceMode.Impulse || forceMode == ForceMode.VelocityChange)
            OnColliderTouched(other);
    }

    void OnTriggerStay(Collider other)
    {
        //Acceleration and Force modes will apply force constantly as long as the collision stays in contact.
        if (forceMode == ForceMode.Acceleration || forceMode == ForceMode.Force)
            OnColliderTouched(other);
    }
}