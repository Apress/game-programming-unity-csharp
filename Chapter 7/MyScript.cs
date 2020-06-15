using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScript : MonoBehaviour
{
    void LogMyMessage(string message)
    {
        message = "A message is being logged: " + message;

        Debug.Log(message);
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LogMyMessage("Hurray for parameters!");
    }
}
