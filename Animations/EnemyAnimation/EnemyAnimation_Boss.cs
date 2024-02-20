using System;
using System.Collections;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.Events;


class EnemyAnimation_Boss : EnemyAnimation_Universal
{
    private void FixedUpdate()
    {
        MonitorEnemyState();
        PassInformationToAnimator();
        if (HasDesiredIdleDirection) AssumeDesiredIdleDirection();
    }

    public void ExecuteAnimationSpawnIn()
    { _animator.Play("Spawn_In", 0, 0); }

    public void ExecuteAnimationSpawnOut()
    { _animator.Play("Spawn_Out", 0, 0); }
}
