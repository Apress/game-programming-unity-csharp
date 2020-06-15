using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8) //Make sure the touching collider is in the Player layer
            SceneManager.LoadScene("main"); //Load the main scene
    }
}
