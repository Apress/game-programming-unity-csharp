using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [Tooltip("The Collider component of the Targeter.  Can be a box or sphere collider.")]
    public Collider col;

    //List of all enemies within the targeter:
    [HideInInspector] public List<Enemy> enemies = new List<Enemy>();

    //Return true if there are any targets:
    public bool TargetsAreAvailable
    {
        get
        {
            return enemies.Count > 0;
        }
    }

    //Methods:
    public void SetRange(int range)
    {
        if (col is BoxCollider)
        {
            //We multiply range by 2 to make sure the targeter covers a space 'range' units in any direction.
            (col as BoxCollider).size = new Vector3(range * 2,30,range * 2);
            
            //Shift the Y position of the center up by half the height:
            (col as BoxCollider).center = new Vector3(0,15,0);

        }
        else if (col is SphereCollider)
        {
            //Sphere collider radius is the distance from the center to the edge.
            (col as SphereCollider).radius = range;
        }
    }

    public Enemy GetClosestEnemy(Vector3 point)
    {
        //Lowest distance we've found so far:
        float lowestDistance = Mathf.Infinity;
        
        //Enemy that had the lowest distance found so far:
        Enemy enemyWithLowestDistance = null;

        //Loop through enemies:
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i]; //Quick reference to current enemy

            //If the enemy has been destroyed or is already dead
            if (enemy == null || !enemy.alive)
            {
                //Remove it and continue the loop at the same index:
                enemies.RemoveAt(i);
                i -= 1;
            }
            else
            {
                //Get distance from the enemy to the given point:
                float dist = Vector3.Distance(point,enemy.trans.position);

                if (dist < lowestDistance)
                {
                    lowestDistance = dist;
                    enemyWithLowestDistance = enemy;
                }
            }
        }

        return enemyWithLowestDistance;
    }

    //Unity events:
    void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<Enemy>();

        if (enemy != null)
            enemies.Add(enemy);
    }

    void OnTriggerExit(Collider other)
    {
        var enemy = other.gameObject.GetComponent<Enemy>();

        if (enemy != null)
            enemies.Remove(enemy);
    }
}
