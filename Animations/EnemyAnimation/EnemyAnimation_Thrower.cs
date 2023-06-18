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
    public bool HasDesiredIdleDirection = false;
    public Vector2 DesiredIdleDirection = Vector2.zero;

    private void FixedUpdate()
    {
        MonitorEnemyDirection();
        MonitorEnemyState();
        PassInformationToAnimator();
        if (HasDesiredIdleDirection) AssumeDesiredIdleDirection();
    }

    public void ExecuteThrowAnimation()
    { _animator.Play("Throw", 0, 0); _enemyScript._performingThrow = true; }

    public void AssumeDesiredIdleDirection()
    {
        if (_state == 0 && _enemyScript.CurrentlyAggroed == false)
        {
            _animator.SetFloat("Horizontal", DesiredIdleDirection.x);
            _animator.SetFloat("Vertical", DesiredIdleDirection.y);
        }
    }

}
