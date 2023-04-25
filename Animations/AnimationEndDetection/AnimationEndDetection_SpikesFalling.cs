using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_SpikesFalling : AnimationEndDetection
{
    public override void OnAnimationFinish() { Destroy(this.gameObject); }
}
