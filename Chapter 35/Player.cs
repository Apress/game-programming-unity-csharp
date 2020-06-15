using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Variables
    [Header("References")]
    [Tooltip("Reference to the root Transform, on which the Player script is attached.")]
    public Transform trans;

    [Tooltip("Reference to the Model Holder Transform.  Movement will be local to the facing of this Transform.")]
    public Transform modelHolder;

    [Tooltip("Reference to the CharacterController component.")]
    public CharacterController charController;

    [Header("Gravity")]
    [Tooltip("Maximum downward momentum the player can have due to gravity.")]
    public float maxGravity = 92;

    [Tooltip("Time taken for downward velocity to go from 0 to the maxGravity.")]
    public float timeToMaxGravity = .6f;
    
    //Property that gets the downward momentum per second to apply as gravity:
    public float GravityPerSecond
    {
        get
        {
            return maxGravity / timeToMaxGravity;
        }
    }

    //Y velocity is stored in a separate float, apart from the velocity vector:
    private float yVelocity = 0;

    [Header("Movement")]
    [Tooltip("Maximum ground speed per second with normal movement.")]
    public float movespeed = 42;
    
    [Tooltip("Time taken, in seconds, to reach maximum speed from a stand-still.")]
    public float timeToMaxSpeed = .3f;

    [Tooltip("Time taken, in seconds, to go from moving at full speed to a stand-still.")]
    public float timeToLoseMaxSpeed = .2f;

    [Tooltip("Multiplier for additional velocity gain when moving against ongoing momentum.  For example, 0 means no additional velocity, .5 means 50% extra, etc.")]
    public float reverseMomentumMulitplier = .6f;

    [Tooltip("Multiplier for velocity influence when moving while midair.  For example, .5 means 50% speed.  A value greater than 1 will make you move faster while midair.")]
    public float midairMovementMultiplier = .4f;

    [Tooltip("Multiplier for how much velocity is retained after bouncing off of a wall.  For example, 1 is full velocity, .2 is 20%.")]
    [Range(0,1)]
    public float bounciness = .2f;

    //Movement direction, local to the model holder facing:
    private Vector3 localMovementDirection = Vector3.zero;
    
    //Current world-space velocity; only the X and Z axes are used:
    private Vector3 worldVelocity = Vector3.zero;
    
    //True if we are currently on the ground, false if we are midair:
    private bool grounded = false;

    //Velocity gained per second.  Applies midairMovementMultiplier when we are not grounded:
    public float VelocityGainPerSecond
    {
        get
        {
            if (grounded)
                return movespeed / timeToMaxSpeed;
            
            //Only use the midairMovementMultiplier if we are not grounded:
            else
                return (movespeed / timeToMaxSpeed) * midairMovementMultiplier;
        }
    }

    //Velocity lost per second based on movespeed and timeToLoseMaxSpeed:
    public float VelocityLossPerSecond
    {
        get
        {
            return movespeed / timeToLoseMaxSpeed;
        }
    }
    
    [Header("Jumping")]
    [Tooltip("Upward velocity provided on jump.")]
    public float jumpPower = 76;

    //Update Logic:
    void Movement()
    {
        //Every frame, we'll reset local movement direction to zero and set its X and Z based on WASD keys:
        localMovementDirection = Vector3.zero;

        //Right and left (D and A):
        if (Input.GetKey(KeyCode.D))
            localMovementDirection.x = 1;

        else if (Input.GetKey(KeyCode.A))
            localMovementDirection.x = -1;

        //Forward and back (W and S):
        if (Input.GetKey(KeyCode.W))
            localMovementDirection.z = 1;

        else if (Input.GetKey(KeyCode.S))
            localMovementDirection.z = -1;

        //If any of the movement keys are held this frame:
        if (localMovementDirection != Vector3.zero)
        {
            //Convert local movement direction to world direction, relative to the model holder:
            Vector3 worldMovementDirection = modelHolder.TransformDirection(localMovementDirection.normalized);
            
            //We'll calculate a multiplier to add the reverse momentum multiplier based on the direction we're trying to move.
            float multiplier = 1;

            //Dot product will be 1 if moving directly towards existing velocity,
            // 0 if moving perpendicular to existing velocity,
            // and -1 if moving directly away from existing velocity.
            float dot = Vector3.Dot(worldMovementDirection.normalized,worldVelocity.normalized);

            //If we're moving away from the velocity by any amount,
            if (dot < 0)
                //Now, flipping the 'dot' with a '-' makes it between 0 and 1.
                //Exactly 1 means moving directly away from existing momentum.
                //Thus, we'll get the full 'reverseMomentumMultiplier' only when it's 1.
                multiplier += -dot * reverseMomentumMulitplier;

            //Calculate the new velocity by adding movement velocity to the current velocity:
            Vector3 newVelocity = worldVelocity + worldMovementDirection * VelocityGainPerSecond * multiplier * Time.deltaTime;

            //If world velocity is already moving more than 'movespeed' per second:
            if (worldVelocity.magnitude > movespeed)
                //Clamp the magnitude at that of our world velocity:
                worldVelocity = Vector3.ClampMagnitude(newVelocity,worldVelocity.magnitude);
            
            //If we aren't moving over 'movespeed' units per second yet,
            else
                //Clamp the magnitude at a maximum of 'movespeed':
                worldVelocity = Vector3.ClampMagnitude(newVelocity,movespeed);
        }
    }

    void VelocityLoss()
    {
        //Lose velocity as long as we are grounded, and we either are not holding movement keys, or are moving faster than 'movespeed':
        if (grounded && (localMovementDirection == Vector3.zero || worldVelocity.magnitude > movespeed))
        {
            //Calculate velocity we'll be losing this frame:
            float velocityLoss = VelocityLossPerSecond * Time.deltaTime;
            
            //If we're losing more velocity than the world velocity magnitude:
            if (velocityLoss > worldVelocity.magnitude)
                //Zero out velocity so we're totally still:
                worldVelocity = Vector3.zero;

            //Otherwise if we're losing less velocity:
            else
                //Apply velocity loss in the opposite direction of the world velocity:
                worldVelocity -= worldVelocity.normalized * velocityLoss;
        }
    }

    void Gravity()
    {
        //While not grounded,
        if (!grounded && yVelocity > -maxGravity)
            //Decrease Y velocity by GravityPerSecond, but don't go under -maxGravity:
            yVelocity = Mathf.Max(yVelocity - GravityPerSecond * Time.deltaTime,-maxGravity);
    }

    void Jumping()
    {
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            //Start traveling 'jumpPower' upwards per second:
            yVelocity = jumpPower;

            //Stop counting ourselves as grounded since we know we just jumped:
            grounded = false;
        }
    }


    void ApplyVelocity()
    {
        //While grounded, apply slight downward velocity to keep our grounded state correct:
        if (grounded)
            yVelocity = -1;
        
        //Calculate the movement we'll receive this frame:
        Vector3 movementThisFrame = (worldVelocity + (Vector3.up * yVelocity)) * Time.deltaTime;
        
        //Calculate where we expect to be after moving if we don't hit anything:
        Vector3 predictedPosition = trans.position + movementThisFrame;
        
        //Only call Move if we have a minimum of .03 velocity:
        if (movementThisFrame.magnitude > .03f)
            charController.Move(movementThisFrame);

        //Checking grounded state:
        if (!grounded && charController.collisionFlags.HasFlag(CollisionFlags.Below))
            grounded = true;
        else if (grounded && !charController.collisionFlags.HasFlag(CollisionFlags.Below))
            grounded = false;

        //Bounce off of walls when we hit our sides while midair:
        if (!grounded && charController.collisionFlags.HasFlag(CollisionFlags.Sides))
            worldVelocity = (trans.position - predictedPosition).normalized * (worldVelocity.magnitude * bounciness);

        //Lose Y velocity if we're going up and collided with something above us:
        if (yVelocity > 0 && charController.collisionFlags.HasFlag(CollisionFlags.Above))
            yVelocity = 0;
    }

    //Unity Events:
    void Update()
    {
        Movement();

        VelocityLoss();

        Gravity();

        Jumping();

        ApplyVelocity();
    }
}