using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderRegion : MonoBehaviour
{
    [Tooltip("Size of the box.")]
    public Vector3 size;

    public Vector3 GetRandomPointWithin()
    {
        float x = transform.position.x + Random.Range(size.x * -.5f,size.x * .5f);
        float z = transform.position.z + Random.Range(size.z * -.5f,size.z * .5f);

        return new Vector3(x,transform.position.y,z);
    }

    void Awake()
    {
        //Get all of our Wanderer children:
        var wanderers = gameObject.GetComponentsInChildren<Wanderer>();

        //Loop through the children:
        for (int i = 0; i < wanderers.Length; i++)
        {
            //Set their .region reference to 'this' script instance:
            wanderers[i].region = this;
        }
    }

}