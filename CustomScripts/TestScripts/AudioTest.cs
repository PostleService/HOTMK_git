using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioTest : MonoBehaviour
{
    public AudioClip audioclip1;
    public AudioClip audioclip2;
    public AudioClip CurrentAudio;
    private AudioClip _currentAudio
    {
        set
        {
            if (CurrentAudio != value)
            {
                CurrentAudio = value;
                PlaySound();
            }
        }
    }

    public bool testbool = false;

    private void Update()
    {
        if (testbool == false)
        {
            _currentAudio = audioclip1;
        }
        else
        {
            _currentAudio = audioclip2;
        }

        Debug.LogWarning(this.gameObject.GetComponent<AudioSource>().clip.ToString());
    }

    private void PlaySound()
    {
        this.gameObject.GetComponent<AudioSource>().clip = CurrentAudio;
        this.gameObject.GetComponent<AudioSource>().loop = true;
        this.gameObject.GetComponent<AudioSource>().Play();
    }
}
