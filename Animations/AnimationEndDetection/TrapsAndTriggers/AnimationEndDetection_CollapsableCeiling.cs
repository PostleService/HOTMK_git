using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_CollapsableCeiling : AnimationEndDetection
{
    public GameObject CollapsableObstacle;
    public override void OnAnimationFinish()
    {
        Instantiate(CollapsableObstacle, new Vector3(transform.position.x, transform.position.y, 0), new Quaternion(), GameObject.Find("TrapsAndTriggers").transform);
        Destroy(gameObject.transform.parent.gameObject);
        Destroy(gameObject);
    }

}
