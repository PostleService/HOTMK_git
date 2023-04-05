using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    public int Order = 0;
    public bool IsLast = false;
    private Animator _animator;
    private string _currentAnimatorState;

    private void OnEnable()
    { this.gameObject.GetComponent<SpriteSheetSwapper_Canvas>().ChangeCanvas(); }

    public void SpawnInstructions(string aInstruction)
    {
        _animator = this.gameObject.GetComponent<Animator>();

        if (aInstruction == "HeartSpawn") _animator.Play(aInstruction);
        if (aInstruction == "HeartLevelUp")
        {
            this.gameObject.GetComponent<SpriteSheetSwapper_Canvas>().ChangeCanvas();
            _animator.Play(aInstruction); 
        }
        if (aInstruction == "HeartHeal")
        {
            if (IsLast) { _animator.Play("HeartHeal"); }
            else _animator.Play("HeartIdle");
        }
    }

    public void DespawnInstructions(string aInstruction)
    {
        _animator = this.gameObject.GetComponent<Animator>();

        if (IsLast && (aInstruction == "HeartDamage" || aInstruction == "HeartDeath")) 
        { _animator.Play(aInstruction); }
        else this.gameObject.SetActive(false);
    }

}
