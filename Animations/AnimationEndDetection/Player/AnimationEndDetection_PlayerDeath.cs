using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_PlayerDeath : AnimationEndDetection
{
    public static event Action OnDie;
    private bool _invoked = false;

    public override void OnAnimationFinish()
    // tell menumanager to start countdown till menu is open
    {
        if (_invoked == false)
        {
            OnDie?.Invoke();
            _invoked = true;
        }
    }
}

