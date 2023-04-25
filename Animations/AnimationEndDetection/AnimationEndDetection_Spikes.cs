using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_Spikes : AnimationEndDetection
{
    public override void OnAnimationFinish()
    { this.gameObject.GetComponent<BoxCollider2D>().enabled = true; }

}
