using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    public bool IsLast = false;
    private Animator _animator;
    private string _currentAnimatorState;

    private float _animationLength;

    public void SpawnInstructions(string aInstruction)
    {
        _animator = this.gameObject.GetComponent<Animator>();
        if (aInstruction == "start" || aInstruction == "levelup")
        {
            if (aInstruction == "levelup")
            { this.gameObject.GetComponent<SpriteSheetSwapper_Canvas>().ChangeCanvas(); }

            ChangeAnimatorState("HeartSpawn");
        }
        if (IsLast && aInstruction == "heal") { ChangeAnimatorState("HeartHeal"); }
        WaitBeforeIdle(_animationLength);
    }

    public void DespawnInstructions(string aInstruction)
    {
        _animator = this.gameObject.GetComponent<Animator>();
        if (IsLast && aInstruction == "damage" || aInstruction == "death")
        {
            ChangeAnimatorState("HeartDamage");
            Destroy(this.gameObject, _animationLength);
        }
        else { Destroy(this.gameObject); }
    }

    private void ChangeAnimatorState(string aNewState)
    {
        if (_currentAnimatorState == aNewState) return;
        else
        {
            _animator.Play(aNewState);
            _currentAnimatorState = aNewState;

            // Speaking directly to runtime animation controller is necessary because 
            // AnimationController returns length of clip with delay (thus returning length of wrong clip)
            // Can be circumvented with WaitForEndOfFrame coroutine, but returns wrong .Length for some reason
            RuntimeAnimatorController rac = _animator.runtimeAnimatorController;
            for (int i = 0; i < rac.animationClips.Length; i++)
            {
                if (rac.animationClips[i].name == aNewState)
                { _animationLength = rac.animationClips[i].length; }
            }
        }
    }

    private void WaitBeforeIdle(float aSeconds)
    { StartCoroutine(Wait(aSeconds)); }

    private IEnumerator Wait(float aSeconds)
    {
        yield return new WaitForSeconds(aSeconds);
        ChangeAnimatorState("HeartIdle");
    }

}
