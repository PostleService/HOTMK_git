using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_WallBreakEffect : MonoBehaviour
{
    public string AnimationName;
    public float ActivationTimer = 0.5f;
    private bool _spriteRendererActivated = false;
    private Animator _thisObjectAnimator;

    private void Awake()
    { transform.localPosition = Vector3.zero; }

    private void Start()
    {
        if (gameObject.GetComponent<Animator>() != null)
        { _thisObjectAnimator = gameObject.GetComponent<Animator>(); }
    }

    private void FixedUpdate()
    {

        if (ActivationTimer > 0) ActivationTimer -= Time.fixedDeltaTime;
        else if (ActivationTimer <= 0 && _spriteRendererActivated == false)
        {
            _spriteRendererActivated = true;
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<Animator>().enabled = true;
        }
        CheckAnimationEnd();

    }

    public void CheckAnimationEnd()
    {
        if (_thisObjectAnimator.GetCurrentAnimatorStateInfo(0).IsName(AnimationName))
        { OnAnimationFinish(); }
    }

    public void OnAnimationFinish()
    { Destroy(gameObject); }
}