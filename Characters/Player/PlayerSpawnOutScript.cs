using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;

public class PlayerSpawnOutScript : MonoBehaviour
{
    public Color NewColor;
    public float FadeOutSpeedVsAnim = 3f;
    public string[] AnimationClipNames = { };

    private float _animLength = 0f;
    private Light2D _lightElem;

    private void Start()
    {
        _lightElem = gameObject.GetComponent<Light2D>();
        AnimationClip[] animClips = gameObject.GetComponent<Animator>().runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in animClips)
        { if (AnimationClipNames.Contains<string>(clip.name)) _animLength += clip.length; }
    }

    // Update is called once per frame
    void FixedUpdate()
    { TransitionColor(); }

    private void TransitionColor()
    {
        if (_animLength >= 0)
        {
            _lightElem.color = Color.Lerp(_lightElem.color, NewColor, Time.fixedDeltaTime / _animLength );
            _animLength -= Time.fixedDeltaTime;
        }
    }
}
