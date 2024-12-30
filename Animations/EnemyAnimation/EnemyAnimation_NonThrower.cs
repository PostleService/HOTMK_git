using System;
using System.Collections;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.Events;

class EnemyAnimation_NonThrower : EnemyAnimation_Universal
{
    private void FixedUpdate()
    {
        MonitorEnemyState();
        PassInformationToAnimator();
        if (HasDesiredIdleDirection) AssumeDesiredIdleDirection();
    }

    public void ExecutePreRushAnimation()
    {
        _animator.Play("PreRush", 0, 0);
    }
}