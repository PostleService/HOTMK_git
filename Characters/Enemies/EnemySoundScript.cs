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
    { FMODUnity.RuntimeManager.PlayOneShotAttached(AggressionSound, gameObject); }

    public void PlayFearSound()
    {  FMODUnity.RuntimeManager.PlayOneShotAttached(FearSound, gameObject);  }
}
