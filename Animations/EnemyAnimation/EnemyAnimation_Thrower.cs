using System;
using System.Collections;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.Events;

class EnemyAnimation_Thrower : EnemyAnimation_Universal
{
    private void FixedUpdate()
    {
        MonitorEnemyDirection();
        MonitorEnemyState();
        PassInformationToAnimator();
    }

    public void ExecuteThrowAnimation()
    { _animator.Play("Throw", 0, 0); _enemyScript._performingThrow = true; }

}
