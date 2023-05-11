using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class EnemySoundScript : MonoBehaviour
{
    public EventReference LoopingSound = new EventReference();
    public EventReference AggressionSound = new EventReference();
    public EventReference FearSound = new EventReference();

    public void PlayAggroSound()
    {
        SoundBiteScript sbs = gameObject.AddComponent(typeof(SoundBiteScript)) as SoundBiteScript;
        sbs.DestroyInstance = true;
        sbs.DestroyInstanceTimer = 5f;
        sbs.SoundToPlay = AggressionSound;
    }

    public void PlayFearSound()
    {
        SoundBiteScript sbs = gameObject.AddComponent(typeof(SoundBiteScript)) as SoundBiteScript;
        sbs.DestroyInstance = true;
        sbs.DestroyInstanceTimer = 5f;
        sbs.SoundToPlay = FearSound;
    }
}
