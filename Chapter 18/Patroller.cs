using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : MonoBehaviour
{
    //Consts:
    private const float rotationSlerpAmount = .68f;

    [Header("References")]
    public Transform trans;
    public Transform modelHolder;

    [Header("Stats")]
    public float movespeed = 30;

    //Private variables:
    private int currentPointIndex;
    private Transform currentPoint;

    private Transform[] patrolPoints;
    
    //Returns a List containing the Transform of each child with a name that starts with "Patrol Point (".
    private List<Transform> GetUnsortedPatrolPoints()
    {
        //Get the Transform of each child in the Patroller:
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();

        //Declare a local List storing Transforms:
        var points = new List<Transform>();

        //Loop through the child Transforms:
        for (int i = 0; i < children.Length; i++)
        {
            //Check if the child's name starts with "Patrol Point (":
            if (children[i].gameObject.name.StartsWith("Patrol Point ("))
            {
                //If so, add it to the 'points' List:
                points.Add(children[i]);
            }
        }

        //Return the point List:
        return points;
    }

    private void SetCurrentPatrolPoint(int index)
    {
        currentPointIndex = index;
        currentPoint = patrolPoints[index];
    }

    void Start()
    {
        //Get an unsorted list of patrol points:
        List<Transform> points = GetUnsortedPatrolPoints();

        //Only continue if we found at least 1 patrol point:
        if (points.Count > 0)
        {
            //Prepare our array of patrol points:
            patrolPoints = new Transform[points.Count];
            
            //Loop through all patrol points:
            for (int i = 0; i < points.Count; i++)
            {
                //Quick reference to the current point:
                Transform point = points[i];

                //Isolate just the patrol point number within the name:
                int closingParenthesisIndex = point.gameObject.name.IndexOf(')');

                string indexSubstring = point.gameObject.name.Substring(14,closingParenthesisIndex - 14);

                //Convert the number from a string to an integer:
                int index = Convert.ToInt32(indexSubstring);

                //Set a reference in our script patrolPoints array:
                patrolPoints[index] = point;
                
                //Unparent each patrol point so it doesn't move with us:
                point.SetParent(null);

                //Hide patrol points in the Hierarchy:
                point.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }

            //Start patrolling at the first point in the array:
            SetCurrentPatrolPoint(0);
        }
    }

    void Update()
    {
        //Only operate if we have a currentPoint:
        if (currentPoint != null)
        {
            //Move root GameObject towards the current point:
            trans.position = Vector3.MoveTowards(trans.position,currentPoint.position,movespeed * Time.deltaTime);
            
            //If we're on top of the point already, change the current point:
            if (trans.position == currentPoint.position)
            {
                //If we're at the last patrol point...:
                if (currentPointIndex >= patrolPoints.Length - 1)
                {
                    //...we'll set to the first patrol point (double back):
                    SetCurrentPatrolPoint(0);
                }
                else //Else if we're not at the last patrol point
                    SetCurrentPatrolPoint(currentPointIndex + 1); //Go to the index after the current.
            }
            //Else if we're not on the point yet, rotate the model towards it:
            else
            {
                Quaternion lookRotation = Quaternion.LookRotation((currentPoint.position - trans.position).normalized);

                modelHolder.rotation = Quaternion.Slerp(modelHolder.rotation,lookRotation,rotationSlerpAmount);
            }
        }

    }

}
