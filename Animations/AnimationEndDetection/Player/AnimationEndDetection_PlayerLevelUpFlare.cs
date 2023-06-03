using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_PlayerLevelUpFlare : AnimationEndDetection
{
    private void Awake()
    { transform.localPosition = Vector3.zero; }

    public override void OnAnimationFinish()
    { Destroy(gameObject); }
}

