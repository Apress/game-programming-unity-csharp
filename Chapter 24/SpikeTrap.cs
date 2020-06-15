using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private enum State
    {
        Lowered,
        Lowering,
        Raising,
        Raised
    }

    private State state = State.Lowered;

    private const float SpikeHeight = 3.6f;

    private const float LoweredSpikeHeight = .08f;

    [Header("References")]
    [Tooltip("Reference to the parent of all the spikes.")]
    public Transform spikeHolder;
    public GameObject hitboxGameObject;
    public GameObject colliderGameObject;


    [Header("Stats")]
    [Tooltip("Time in seconds after lowering the spikes before raising them again.")]
    public float interval = 2f;

    [Tooltip("Time in seconds after raising the spikes before they start lowering again.")]
    public float raiseWaitTime = .3f;

    [Tooltip("Time in seconds taken to fully lower the spikes.")]
    public float lowerTime = .6f;

    [Tooltip("Time in seconds taken to fully raise the spikes.")]
    public float raiseTime = .08f;

    private float lastSwitchTime = Mathf.NegativeInfinity;

    //Methods:
    void StartRaising()
    {
        lastSwitchTime = Time.time;
        state = State.Raising;
        hitboxGameObject.SetActive(true);
    }

    void StartLowering()
    {
        lastSwitchTime = Time.time;
        state = State.Lowering;
    }

    //Unity events:
    void Start()
    {
        //Spikes will be lowered by default.
        //We'll start raising them 'interval' seconds after Start.
        Invoke("StartRaising",interval);
    }

    void Update()
    {
        if (state == State.Lowering)
        {
            //Get the spike holder local scale:
            Vector3 scale = spikeHolder.localScale;
            
            //Update the Y scale by lerping from max height to min height:
            scale.y = Mathf.Lerp(SpikeHeight,LoweredSpikeHeight,(Time.time - lastSwitchTime) / lowerTime);
            
            //Apply the updated scale to the spike holder:
            spikeHolder.localScale = scale;

            //If the spikes have finished lowering:
            if (scale.y == LoweredSpikeHeight)
            {
                //Update the state and Invoke the next raising in 'interval' seconds:
                Invoke("StartRaising",interval);
                state = State.Lowered;
                colliderGameObject.SetActive(false);
            }
        }
        else if (state == State.Raising)
        {
            //Get the spike holder local scale:
            Vector3 scale = spikeHolder.localScale;
            
            //Update the Y scale by lerping from min height to max height:
            scale.y = Mathf.Lerp(LoweredSpikeHeight,SpikeHeight,(Time.time - lastSwitchTime) / raiseTime);
            
            //Apply the updated scale to the spike holder:
            spikeHolder.localScale = scale;

            //If the spikes have finished raising:
            if (scale.y == SpikeHeight)
            {
                //Update the state and Invoke the next lowering in 'raiseWaitTime' seconds:
                Invoke("StartLowering",raiseWaitTime);
                state = State.Raised;
                
                //Activate the collider to block the player:
                colliderGameObject.SetActive(true);

                //Deactivate the hitbox so it no longer kills the player:
                hitboxGameObject.SetActive(false);
            }
        }
    }
}