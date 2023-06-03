using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_VictoryTrigger : AnimationEndDetection
{
    public override void OnAnimationFinish()
    {
        MenuManagerScript mm = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>();
        mm.ReactToVictory();
        Destroy(gameObject);
    }

}
