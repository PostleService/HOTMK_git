using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_LaserTrap : AnimationEndDetection
{
    public GameObject ObjectToSpawn;
    public override void OnAnimationFinish()
    {
        if (ObjectToSpawn != null) 
            Instantiate(ObjectToSpawn, new Vector3(transform.position.x, transform.position.y, 0), new Quaternion(), GameObject.Find("TrapsAndTriggers").transform);
        Destroy(gameObject);
    }

}
