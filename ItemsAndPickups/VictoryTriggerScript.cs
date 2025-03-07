using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class VictoryTriggerScript : MonoBehaviour
{
    [Tooltip("How far away from NavMesh can the agent be to be snapped back")]
    public float DistanceOutOfNavMesh = 5f;

    public GameObject PlayerDespawnAnimation;

    private void Start()
    {
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, DistanceOutOfNavMesh, -1))
        { transform.position = myNavHit.position; }

        NavMeshAgent _agent = gameObject.GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            MenuManagerScript mm = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>();
            mm.UnlockNextLevel();

            foreach (Transform tr in collision.gameObject.transform)
            { if (tr.GetComponent<Camera>() != null) { tr.SetParent(null); } }
            Destroy(collision.gameObject);

            if (PlayerDespawnAnimation != null) { Instantiate(PlayerDespawnAnimation, gameObject.transform.position, new Quaternion(), null); }
            Destroy(gameObject);
        }
    }

}
