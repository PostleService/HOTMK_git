using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class SoundBiteScript : MonoBehaviour
{
    public bool PlayedOnCall = false;
    
    [Tooltip("In case this is a local audiosource that is supposed to play once the original object is destroyed")]
    public bool DestroySource = false;
    public float DestroyTimer = -1f;
    public EventReference SoundToPlay = new EventReference();

    // Start is called before the first frame update
    void Start()
    { if (PlayedOnCall == false) FMODUnity.RuntimeManager.PlayOneShotAttached(SoundToPlay, gameObject); }

    private void FixedUpdate()
    { DestroyTimerDecrement();  }

    private void DestroyTimerDecrement()
    {
        if (DestroySource && DestroyTimer != -1)
        {
            if (DestroyTimer >= 0) DestroyTimer -= Time.fixedDeltaTime;
            else Destroy(gameObject);
        }
    }

    public void PlayOnCall()
    { FMODUnity.RuntimeManager.PlayOneShotAttached(SoundToPlay, gameObject); }

}
