using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_EnemiesDeath : AnimationEndDetection
{
    public GameObject DeathObject;

    public override void OnAnimationFinish()
    { 
        GameObject go = Instantiate(DeathObject, transform.position, new Quaternion(), GameObject.Find("EnemyCorpseHolder").transform);
        Destroy(gameObject);
    }

}

