using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_Rusher : AnimationEndDetection
{
    public override void OnAnimationFinish()
    {
        EnemyScript es = gameObject.GetComponent<EnemyScript>();
        es._onCooldownPreRush = false;
    }

}
