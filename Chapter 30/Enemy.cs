using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Transform trans;
    public Transform projectileSeekPoint;

    [Header("Stats")]
    public float maxHealth;
    
    public float healthGainPerLevel;

    [HideInInspector] public float health;

    [HideInInspector] public bool alive = true;

    //Methods:
    public void TakeDamage(float amount)
    {
        //Only proceed if damage taken is more than 0:
        if (amount > 0)
        {
            //Reduce health by 'amount' but don't go under 0:
            health = Mathf.Max(health - amount,0);

            //If all health is lost,
            if (health == 0)
            {
                //...call Die:
                Die();
            }
        }
    }

    public void Die()
    {
        if (alive)
        {
            alive = false;
            Destroy(gameObject);
        }
    }

    public void Leak()
    {
        Player.remainingLives -= 1;
        Destroy(gameObject);
    }

    //Unity events:
    protected virtual void Start()
    {
        maxHealth = maxHealth + (healthGainPerLevel * (Player.level - 1));
        health = maxHealth;
    }
}
