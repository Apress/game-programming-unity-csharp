using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	//References
	[Header("References")]
	public Transform trans;
	public Transform modelTrans;
	public CharacterController characterController;
	public GameObject cam;

	//Movement
	[Header("Movement")]
	[Tooltip("Units moved per second at maximum speed.")]
	public float movespeed = 24;

	[Tooltip("Time, in seconds, to reach maximum speed.")]
	public float timeToMaxSpeed = .26f;

	private float VelocityGainPerSecond { get { return movespeed / timeToMaxSpeed; } }
	
	[Tooltip("Time, in seconds, to go from maximum speed to stationary.")]
	public float timeToLoseMaxSpeed = .2f;

	private float VelocityLossPerSecond { get { return movespeed / timeToLoseMaxSpeed; } }

	[Tooltip("Multiplier for momentum when attempting to move in a direction opposite the current traveling direction (e.g. trying to move right when already moving left).")]
	public float reverseMomentumMultiplier = 2.2f;

	private Vector3 movementVelocity = Vector3.zero;

	//Death and Respawning
	[Header("Death and Respawning")]
	[Tooltip("How long after the player's death, in seconds, before they are respawned?")]
	public float respawnWaitTime = 2f;

	private bool dead = false;

	private Vector3 spawnPoint;

	private Quaternion spawnRotation;

	//Dashing
	[Header("Dashing")]
	[Tooltip("Total number of units traveled when performing a dash.")]
	public float dashDistance = 17;

	[Tooltip("Time taken to perform a dash, in seconds.")]
	public float dashTime = .26f;

	[Tooltip("Time after dashing finishes before it can be performed again.")]
	public float dashCooldown = 1.8f;

	private bool CanDashNow
	{
		get
		{
			return (Time.time > dashBeginTime + dashTime + dashCooldown);
		}
	}

	private bool IsDashing
	{
		get
		{
			return (Time.time < dashBeginTime + dashTime);
		}
	}

	private Vector3 dashDirection;
	private float dashBeginTime = Mathf.NegativeInfinity;

	//Methods:
	private void Movement()
	{
		//Only move if we aren't dashing:
		if (!IsDashing)
		{
			//FORWARD AND BACKWARD MOVEMENT

			//If W or the up arrow key is held:
			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
			{
				if (movementVelocity.z >= 0) //If we're already moving forward
					//Increase Z velocity by VelocityGainPerSecond, but don't go higher than 'movespeed':
					movementVelocity.z = Mathf.Min(movespeed,movementVelocity.z + VelocityGainPerSecond * Time.deltaTime);

				else //Else if we're moving back
					//Increase Z velocity by VelocityGainPerSecond, using the reverseMomentumMultiplier, but don't raise higher than 0:
					movementVelocity.z = Mathf.Min(0,movementVelocity.z + VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);
			}
			//If S or the down arrow key is held:
			else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
			{
				if (movementVelocity.z > 0) //If we're already moving forward
					movementVelocity.z = Mathf.Max(0,movementVelocity.z - VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);

				else //If we're moving back or not moving at all
					movementVelocity.z = Mathf.Max(-movespeed,movementVelocity.z - VelocityGainPerSecond * Time.deltaTime);
			}
			else //If neither forward nor back are being held
			{
				//We must bring the Z velocity back to 0 over time.
				if (movementVelocity.z > 0) //If we're moving up,
					//Decrease Z velocity by VelocityLossPerSecond, but don't go any lower than 0:
					movementVelocity.z = Mathf.Max(0,movementVelocity.z - VelocityLossPerSecond * Time.deltaTime);
				
				else //If we're moving down,
					//Increase Z velocity (back towards 0) by VelocityLossPerSecond, but don't go any higher than 0:
					movementVelocity.z = Mathf.Min(0,movementVelocity.z + VelocityLossPerSecond * Time.deltaTime);
			}

			//RIGHT AND LEFT MOVEMENT:

			//If D or the right arrow key is held:
			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
			{
				if (movementVelocity.x >= 0) //If we're already moving right
					//Increase X velocity by VelocityGainPerSecond, but don't go higher than 'movespeed':
					movementVelocity.x = Mathf.Min(movespeed,movementVelocity.x + VelocityGainPerSecond * Time.deltaTime);

				else //If we're moving left
					//Increase x velocity by VelocityGainPerSecond, using the reverseMomentumMultiplier, but don't raise higher than 0:
					movementVelocity.x = Mathf.Min(0,movementVelocity.x + VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);
			}

			else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
			{
				if (movementVelocity.x > 0) //If we're already moving right
					movementVelocity.x = Mathf.Max(0,movementVelocity.x - VelocityGainPerSecond * reverseMomentumMultiplier * Time.deltaTime);

				else //If we're moving left or not moving at all
					movementVelocity.x = Mathf.Max(-movespeed,movementVelocity.x - VelocityGainPerSecond * Time.deltaTime);
			}

			else //If neither right nor left are being held
			{
				//We must bring the X velocity back to 0 over time.
				
				if (movementVelocity.x > 0) //If we're moving right,
					//Decrease X velocity by VelocityLossPerSecond, but don't go any lower than 0:
					movementVelocity.x = Mathf.Max(0,movementVelocity.x - VelocityLossPerSecond * Time.deltaTime);
				
				else //If we're moving left,
					//Increase X velocity (back towards 0) by VelocityLossPerSecond, but don't go any higher than 0:
					movementVelocity.x = Mathf.Min(0,movementVelocity.x + VelocityLossPerSecond * Time.deltaTime);
			}

			//If the player is moving in either direction (left/right or up/down):
			if (movementVelocity.x != 0 || movementVelocity.z != 0)
			{
				//Applying the movement velocity:
				characterController.Move(movementVelocity * Time.deltaTime);
				
				//Keeping the model holder rotated towards the last movement direction:
				modelTrans.rotation = Quaternion.Slerp(modelTrans.rotation,Quaternion.LookRotation(movementVelocity),.18F);
			}
		}
	}

	private void Dashing()
	{
		//If we aren't dashing right now:
		if (!IsDashing)
		{
			//If dash is not on cooldown, and the space key is pressed:
			if (CanDashNow && Input.GetKey(KeyCode.Space))
			{
				//Find the direction we're holding with the movement keys:
				Vector3 movementDir = Vector3.zero;

				//If holding W or up arrow, set z to 1:
				if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
					movementDir.z = 1;
				
				//Else if holding S or down arrow, set z to -1:
				else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
					movementDir.z = -1;
				
				//If holding D or right arrow, set x to 1:
				if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
					movementDir.x = 1;
				
				//Else if holding A or left arrow, set x to -1:
				else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
					movementDir.x = -1;

				//If at least one movement key was held:
				if (movementDir.x != 0 || movementDir.z != 0)
				{
					//Start dashing:
					dashDirection = movementDir;
					dashBeginTime = Time.time;
					movementVelocity = dashDirection * movespeed;
					modelTrans.forward = dashDirection;
				}
			}
		}
		else //If we are dashing
		{
			characterController.Move(dashDirection * (dashDistance / dashTime) * Time.deltaTime);
		}
	}

	public void Die()
	{
		if (!dead)
		{
			dead = true;
			Invoke("Respawn",respawnWaitTime);
			movementVelocity = Vector3.zero;
			enabled = false;
			characterController.enabled = false;  
			modelTrans.gameObject.SetActive(false);
			dashBeginTime = Mathf.NegativeInfinity;
		}
	}

	public void Respawn()
	{
		dead = false;
		trans.position = spawnPoint;
		modelTrans.rotation = spawnRotation;
		enabled = true;
		characterController.enabled = true;
		modelTrans.gameObject.SetActive(true);
	
	}
	
	//Unity events:
	void Start()
	{
		spawnPoint = trans.position;
		spawnRotation = modelTrans.rotation;
	}

	void Update()
	{
		Movement();

		Dashing();
	}
}

