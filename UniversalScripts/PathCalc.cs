using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathCalc : MonoBehaviour
{
    [Tooltip("How far away from NavMesh can the agent be to be snapped back")]
    public float DistanceOutOfNavMesh = 5f;

    // Start is called before the first frame update
    void Start()
    {
        BecomeAgentAndSpawn();
    }

    private void BecomeAgentAndSpawn()
    {
        
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, DistanceOutOfNavMesh, -1))
        { transform.position = myNavHit.position; }

        this.gameObject.GetComponent<NavMeshAgent>().updateRotation = false;
        this.gameObject.GetComponent<NavMeshAgent>().updateUpAxis = false;
        this.gameObject.GetComponent<NavMeshAgent>().radius = 0.005f;
    }

    public void AssignTarget(Vector3 aVec3)
    { this.gameObject.GetComponent<NavMeshAgent>().SetDestination(aVec3); }

    public float GetPathRemainingDistance(NavMeshAgent navMeshAgent)
    {

        if (navMeshAgent.pathPending ||
            navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
            navMeshAgent.path.corners.Length == 0)
            return -1.0f;

        float distance = 0.0f;
        for (int i = 0; i < navMeshAgent.path.corners.Length - 1; ++i)
        {
            distance += Vector3.Distance(navMeshAgent.path.corners[i], navMeshAgent.path.corners[i + 1]);
        }
        return distance;

    }

    public float CalculateDistanceToPoint()
    {
        float _remainingDistance = GetPathRemainingDistance(this.gameObject.GetComponent<NavMeshAgent>());
        return _remainingDistance;
    }

}
