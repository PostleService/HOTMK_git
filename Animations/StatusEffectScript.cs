using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class StatusEffectScript : MonoBehaviour
{
    public RuntimeAnimatorController SlowedStatus;
    public RuntimeAnimatorController StunnedStatus;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Awake()
    { 
        _spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        _animator = this.gameObject.GetComponent<Animator>();
    }

    public void RemoveStatusEffect()
    {
        _spriteRenderer.sprite = null;
        _animator.runtimeAnimatorController = null;
    }

    public void SetSlowed()
    {
        _spriteRenderer.sortingOrder = -1;
        _animator.runtimeAnimatorController = SlowedStatus;
    }

    public void SetStunned()
    {
        _spriteRenderer.sortingOrder = 1;
        _animator.runtimeAnimatorController = StunnedStatus;
    }
}
