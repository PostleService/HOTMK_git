using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsableCeilingTriggerAnim : MonoBehaviour
{
    private bool _hasTriggered = false;
    private Animator _animator;
    public PathblockingTrapTrigger ParentTrapScript;

    private void FixedUpdate()
    {
        if (ParentTrapScript._hasBeenTriggered)
        {
            if (!_hasTriggered)
            {
                _animator = this.gameObject.GetComponent<Animator>();
                _animator.Play("TriggeredAnimation");
                if (gameObject.GetComponent<SoundBiteScript>() != null)
                gameObject.GetComponent<SoundBiteScript>().PlayOnCall(gameObject,(name + "_CollapsableCeilingTriggerSound"));
                
                _hasTriggered = true;
            }
        }   
    }
}
