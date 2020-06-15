using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //Try to find a PlatformDetector on the touching object:
        PlatformDetector detector = other.GetComponent<PlatformDetector>();
        
        //If there is a detector,
        if (detector != null)
            //Set this Transform as its 'platform' variable:
            detector.platform = transform;
    }

    void OnTriggerExit(Collider other)
    {
        //Try to find a PlatformDetector on the touching object:
        PlatformDetector detector = other.GetComponent<PlatformDetector>();
        
        //If there is a detector,
        if (detector != null)
            //Null out its 'platform' variable:
            detector.platform = null;
    }
}