using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_WallBreakEffect : MonoBehaviour
{
    public string AnimationName;
    private Animator _thisObjectAnimator;
    private SpriteRenderer _srParent;
    private SpriteRenderer _srThisObj;

    private void Awake()
    { transform.localPosition = Vector3.zero; }

    private void Start()
    {
        if (gameObject.GetComponent<Animator>() != null)
        { _thisObjectAnimator = gameObject.GetComponent<Animator>(); }
        _srParent = gameObject.transform.parent.GetComponent<SpriteRenderer>();
        _srThisObj = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        CheckAnimationEnd();
        if (_srParent.enabled == true) _srThisObj.enabled = true;
        else _srThisObj.enabled = false; 
    }

    public void OnAnimationFinish()
    { Destroy(gameObject); }

    public void CheckAnimationEnd()
    {
        if (_thisObjectAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationName))
        { OnAnimationFinish(); }
    }
}
