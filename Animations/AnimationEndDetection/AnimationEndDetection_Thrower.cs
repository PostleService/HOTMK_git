using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_Thrower : AnimationEndDetection
{
    public bool CanExecuteAgain = true;
    public override void OnAnimationFinish()
    {
        EnemyScript es = gameObject.GetComponent<EnemyScript>();
        // CanExecuteAgain is there to prevent spawning multiple instances of the item
        if (CanExecuteAgain)
        {
            // only perform throw if player is still within vision
            if (es.RayCast().Item1)
            { 
                es.PerformThrow(es.GetDirection(es.RayCast().Item2, es.RayCast().Item3));
                CanExecuteAgain = false;
            }
        }
        es._performingThrow = false;
    }

}
