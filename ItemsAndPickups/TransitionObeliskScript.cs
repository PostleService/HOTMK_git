using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TransitionObeliskScript : MonoBehaviour
{
    public Color NewColor;
    public float TransitionSpeed = 1f;
    private float _animLength = 0f;
    private Light2D _lightElem;

    public string AnimationClipName = "Obelisk_TransitionAnim";


    private void Start()
    {
        _lightElem = gameObject.GetComponent<Light2D>();
        AnimationClip[] animClips = gameObject.GetComponent<Animator>().runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in animClips) 
        { if (clip.name == AnimationClipName) _animLength = clip.length; }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TransitionColor();
    }

    private void TransitionColor()
    {
        if (_animLength >= 0)
        {
            _lightElem.color = Color.Lerp(_lightElem.color, NewColor, Time.fixedDeltaTime / _animLength );
            _animLength -= Time.fixedDeltaTime;
        }
    }
}
