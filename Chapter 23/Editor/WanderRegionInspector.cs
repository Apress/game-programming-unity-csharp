using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WanderRegion))]
public class WanderRegionInspector : Editor
{
    //Quick reference to target with a typecast:
    private WanderRegion Target
    {
        get
        {
            return (WanderRegion)target;
        }
    }

    //The height of the box display.
    private const float BoxHeight = 10f;

    void OnSceneGUI()
    {
        //Make the handles white:
        Handles.color = Color.white;
        
        //Draw a wireframe cube resembling the wander region:
        Handles.DrawWireCube(Target.transform.position + (Vector3.up * BoxHeight * .5f),new Vector3(Target.size.x,BoxHeight,Target.size.z)); 
    }
}
