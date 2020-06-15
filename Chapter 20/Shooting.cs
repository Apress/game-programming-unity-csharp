using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("References")]
    public Transform spawnPoint;
    public GameObject projectilePrefab;

    [Header("Stats")]
    [Tooltip("Time, in seconds, between the firing of each projectile.")]
    public float fireRate = 1;

    private float lastFireTime = 0;

    //Unity events:
    void Update()
    {
        //If the current game time has surpassed the time we last fired, plus the rate of fire:
        if (Time.time >= lastFireTime + fireRate)
        {
            //Spawn a projectile and mark the time we did it:
            lastFireTime = Time.time;
            Instantiate(projectilePrefab,spawnPoint.position,spawnPoint.rotation);
        }
    }
}
