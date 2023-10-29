using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_PathblockingTrap : AnimationEndDetection
{
    public GameObject CollapsableObstacle;

    [Header("Pass to trap")]
    public bool TeleportBossToCustom = false;
    public Vector3 CustomTeleportDestination = Vector3.zero;
    
    public bool DestroyParent = true;

    public override void OnAnimationFinish()
    {
        if (CollapsableObstacle != null)
        {
            GameObject trap = Instantiate(CollapsableObstacle, new Vector3(transform.position.x, transform.position.y, 0), new Quaternion(), GameObject.Find("TrapsAndTriggers").transform);
            if (trap.GetComponent<TrapScript>() != null)
            {
                TrapScript ts = trap.GetComponent<TrapScript>();
                ts.TeleportBossToCustom = TeleportBossToCustom;
                ts.CustomTeleportDestination = CustomTeleportDestination;
            }
            if (trap.GetComponent<AnimationEndDetection_PathblockingTrap>() != null)
            {
                AnimationEndDetection_PathblockingTrap ts = trap.GetComponent<AnimationEndDetection_PathblockingTrap>();
                ts.TeleportBossToCustom = TeleportBossToCustom;
                ts.CustomTeleportDestination = CustomTeleportDestination;
            }

        }

        if (DestroyParent == true) Destroy(gameObject.transform.parent.gameObject);
        Destroy(gameObject);
    }

}
