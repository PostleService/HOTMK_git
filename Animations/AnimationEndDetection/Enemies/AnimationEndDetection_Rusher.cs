using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_Rusher : AnimationEndDetection
{
    public override void OnAnimationFinish()
    {
        gameObject.GetComponent<EnemyScript>()._onCooldownPreRush = false;
        gameObject.GetComponent<EnemySoundScript>().PlayAggroSound();
    }

}
