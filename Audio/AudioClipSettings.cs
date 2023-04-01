using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AudioClipSettings
{
    public AudioClip Clip;
    // Details for AudioSource to adjust upon receiving AudioClip
    public bool PlayOnAwake;
    public bool Loop;
    public bool CanPitchShift;

}
