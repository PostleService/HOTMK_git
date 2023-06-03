using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class AnimationEndDetection_Heart : AnimationEndDetection
{
    public override void OnAnimationFinish()
    { gameObject.SetActive(false); }

}
