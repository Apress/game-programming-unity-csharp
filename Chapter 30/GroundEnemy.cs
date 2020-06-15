using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : Enemy
{
    public static NavMeshPath path;

    public float movespeed = 22;

    private int currentCornerIndex = 0;
    private Vector3 currentCorner;

    private bool CurrentCornerIsFinal
    {
        get
        {
            return currentCornerIndex == (path.corners.Length - 1);
        }
    }
    
    protected override void Start()
    {
        base.Start();
        currentCorner = path.corners[0];
    }

    private void GetNextCorner()
    {
        //Increment the corner index:
        currentCornerIndex += 1;

        //Set currentCorner to corner with the updated index:
        currentCorner = path.corners[currentCornerIndex];
    }

    void Update()
    {
        //If this is not the first corner,
        if (currentCornerIndex != 0)
            //Point from our position to the current corner position:
            trans.forward = (currentCorner - trans.position).normalized;

        //Move towards the current corner:
        trans.position = Vector3.MoveTowards(trans.position,currentCorner,movespeed * Time.deltaTime);
        
        //Whenever we reach a corner,
        if (trans.position == currentCorner)
        {
            //If it's the last corner (positioned at the path goal)
            if (CurrentCornerIsFinal)
                Leak();
            else
                GetNextCorner();
        }
    }
}
