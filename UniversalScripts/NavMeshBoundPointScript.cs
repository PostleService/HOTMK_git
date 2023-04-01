using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBoundPointScript : MonoBehaviour
{
    [Tooltip("How far away from NavMesh can the agent be to be snapped back")]
    public float DistanceOutOfNavMesh = 5f;

    private void Start()
    {
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, DistanceOutOfNavMesh, -1))
        { transform.position = myNavHit.position; }
        SelfDestruct();
    }

    public void SelfDestruct()
    {
        Transform chtr = gameObject.GetComponent<Transform>();
        chtr.rotation = new Quaternion(0, 0, 0, 0); chtr.position = new Vector3(chtr.position.x, chtr.position.y, 0);
        Destroy(this);
    }
}
