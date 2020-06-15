using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotPlate : TargetingTower
{
    public float damagePerSecond = 10;

    void Update()
    {
        //If we have any targets:
        if (targeter.TargetsAreAvailable)
        {
            //Loop through them:
            for (int i = 0; i < targeter.enemies.Count; i++)
            {
                Enemy enemy = targeter.enemies[i];

                //Only burn ground enemies:
                if (enemy is GroundEnemy)
                    enemy.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}