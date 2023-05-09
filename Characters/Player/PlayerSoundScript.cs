using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class PlayerSoundScript : MonoBehaviour
{
    public EventReference DamageSound = new EventReference();
    public EventReference StepSound;
    public bool PlayStepSounds = false;

    private FloorMaterialInfo _floorMaterialInfo;

    public void Awake()
    {
        _floorMaterialInfo = GameObject.Find("FloorMaterialInfo").GetComponent<FloorMaterialInfo>();
    }

    public void PlayDamageSound()
    { FMODUnity.RuntimeManager.PlayOneShotAttached(DamageSound, gameObject); }

    public void PlayStepSound()
    {
        if (PlayStepSounds == true)
        {
            string Material = _floorMaterialInfo.RequestMaterialAtLocation(gameObject.transform.position);
            FMOD.Studio.EventInstance evInst = FMODUnity.RuntimeManager.CreateInstance(StepSound);
            evInst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));

            evInst.setParameterByNameWithLabel("Material", Material);
            evInst.start();
            evInst.release();
        }
    }
}
