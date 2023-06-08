using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnityResonance;
using FMODUnity;
using FMOD;

public class SFXSoundPauser : MonoBehaviour
{
    public FMOD.Studio.EventInstance evInst;

    private void OnEnable()
    { MenuManagerScript.OnMenuOpen += ReactToMenuOpenClose; }
    private void OnDisable()
    { MenuManagerScript.OnMenuOpen -= ReactToMenuOpenClose; }

    // Start is called before the first frame update
    void Start()
    { evInst = gameObject.GetComponent<StudioEventEmitter>().EventInstance; }
    private void ReactToMenuOpenClose(bool aBool)
    { evInst.setPaused(aBool); }
}
