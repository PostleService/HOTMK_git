using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSounds : MonoBehaviour
{
    public List<AudioClipSettings> AudioClipSettingsList;

    public AudioClipSettings FindClipByName(string aClipName)
    { 
        AudioClipSettings clip = AudioClipSettingsList.Find(elem => elem.Clip.ToString() == aClipName);
        return clip;
    }
}
