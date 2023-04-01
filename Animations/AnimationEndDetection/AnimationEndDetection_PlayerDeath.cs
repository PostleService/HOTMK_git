using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_PlayerDeath : AnimationEndDetection
{
    public override void OnAnimationFinish()
    // tell menumanager to start countdown till menu is open
    { OnDie?.Invoke(); }
    public static event Action OnDie;
}

