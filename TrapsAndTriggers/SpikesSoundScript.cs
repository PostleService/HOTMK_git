using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class SpikesSoundScript : MonoBehaviour
{
    [Tooltip("If haven't played aggression or fear, first time has percentage modified by value of")]
    public EventReference SpikesDepressedSound = new EventReference();

    public void PlaySpikesDepressedSound()
    {

        SoundBiteScript sbs = gameObject.AddComponent(typeof(SoundBiteScript)) as SoundBiteScript;
        sbs.DestroyInstance = true;
        sbs.DestroyInstanceTimer = 1f;
        sbs.SoundToPlay = SpikesDepressedSound;
    }




}
