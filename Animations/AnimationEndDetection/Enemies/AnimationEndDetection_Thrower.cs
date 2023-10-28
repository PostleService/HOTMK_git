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
            if (es._playerSighted == true)
            { 
                es.PerformThrow(es.GetDirection(es._player.transform.position, es.transform.position));
                CanExecuteAgain = false;
            }
        }
        es._performingThrow = false;
    }

}
