using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    [Tooltip("Units moved per second.")]
    public float movespeed;

    private Vector3 targetPosition;

    protected override void Start()
    {
        base.Start();
        
        //Set target position to the last corner in the path:
        targetPosition = GroundEnemy.path.corners[GroundEnemy.path.corners.Length - 1];

        //But make the Y position equal to the one we were given at start:
        targetPosition.y = trans.position.y;
    }

    void Update()
    {
        //Move towards the target position:
        trans.position = Vector3.MoveTowards(trans.position,targetPosition,movespeed * Time.deltaTime);

        //Leak if we've reached the target position:
        if (trans.position == targetPosition)
            Leak();
    }
}