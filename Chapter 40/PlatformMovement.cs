using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
	private enum State
	{
		Stationary,
		MovingToTarget,
		MovingToInitial
	}

	[Header("References")]
	[Tooltip("The Transform of the platform.")]
	public Transform trans;
	
	[Tooltip("The Rigidbody of the platform.")]
	public Rigidbody rb;
	
	[Header("Stats")]
	[Tooltip("World-space position the platform should move to.")]
	public Vector3 targetPosition;
	
	[Tooltip("Amount of time taken to move from one position to the other.")]
	public float timeToChangePosition = 3;

	[Tooltip("Time to wait after moving to a new position, before beginning to move to the next position.")]
	public float stationaryTime = 1f;
	
	//Returns the units to travel per second when moving.
	private float TravelSpeed
	{
		get
		{
			//Distance between the two positions, divided by number of seconds taken to change position:
			return Vector3.Distance(initialPosition,targetPosition) / timeToChangePosition;
		}
	}

	//Gets the current position we're moving towards based on state.
	private Vector3 CurrentDestination
	{
		get
		{
			if (state == State.MovingToInitial)
				return initialPosition;
			else
				return targetPosition;
		}
	}

	//Gets the current distance from our position to the current destination.
	private float DistanceToDestination
	{
		get
		{
			return Vector3.Distance(trans.position,CurrentDestination);
		}
	}

	//World position of platform on Start.
	private Vector3 initialPosition;

	//Current state of the platform.
	private State state = State.Stationary;

	//State for the platform to use next - either MovingToTarget or MovingToInitial.
	private State nextState = State.MovingToTarget;

    //Transitions 'state' to the 'nextState'.
    void GoToNextState()
    {
        state = nextState;
    }

    //Unity events:
    void Start()
    {
        //Mark the position of the platform at start:
        initialPosition = trans.position;
        
        //Invoke the first transition in state after 'stationaryTime' seconds:
        Invoke("GoToNextState",stationaryTime);
    }

    void FixedUpdate()
    {
        if (state != State.Stationary)
        {
            //Set velocity to travel from our position towards the current destination by 'TravelSpeed' per second:
            rb.velocity = (CurrentDestination - trans.position).normalized * TravelSpeed;

            //Calculate how much distance our velocity is going to move us this frame:
            float distanceMovedThisFrame = (rb.velocity * Time.deltaTime).magnitude;

            //If the distance we'll move this Update is enough to reach the destination:
            if (distanceMovedThisFrame >= DistanceToDestination)
            {
                //Reset velocity to zero and snap us to the position so we don't overshoot it:
                rb.velocity = Vector3.zero;
                trans.position = CurrentDestination;

                //Based on our current state, determine what the next state will be:
                if (state == State.MovingToInitial)
                    nextState = State.MovingToTarget;
                else
                    nextState = State.MovingToInitial;

                //Become stationary and invoke the transition to the next state in 'stationaryTime' seconds:
                state = State.Stationary;
                Invoke("GoToNextState",stationaryTime);
            }	
        }
        else //If we are stationary
            //Maintain velocity at 0 to prevent unwanted movement:
            rb.velocity = Vector3.zero;
    }
}