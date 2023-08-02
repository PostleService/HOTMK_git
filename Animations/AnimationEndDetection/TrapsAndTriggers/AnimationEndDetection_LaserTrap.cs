using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_LaserTrap : AnimationEndDetection
{
    public GameObject ObjectToSpawn;

    [Header("Pass to trap")]
    public bool TeleportBossToCustom = false;
    public Vector3 CustomTeleportDestination = Vector3.zero;
    public override void OnAnimationFinish()
    {
        if (ObjectToSpawn != null)
        {
            GameObject trap = Instantiate(ObjectToSpawn, new Vector3(transform.position.x, transform.position.y, 0), new Quaternion(), GameObject.Find("TrapsAndTriggers").transform);
            TrapScript ts = trap.GetComponent<TrapScript>();
            ts.TeleportBossToCustom = TeleportBossToCustom;
            ts.CustomTeleportDestination = CustomTeleportDestination;
        } 
        Destroy(gameObject);
    }

}
