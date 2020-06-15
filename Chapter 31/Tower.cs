using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public int goldCost = 5;

    [Range(0f,1f)]
    public float refundFactor = .5f;
}
