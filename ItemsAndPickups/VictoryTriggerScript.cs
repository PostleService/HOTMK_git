using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class VictoryTriggerScript : MonoBehaviour
{
    private int _nextSceneToUnlock = 0;
    [Tooltip("How far away from NavMesh can the agent be to be snapped back")]
    public float DistanceOutOfNavMesh = 5f;

    public GameObject PlayerDespawnAnimation;
    private int _maximumLevel;

    private void Start()
    {
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(transform.position, out myNavHit, DistanceOutOfNavMesh, -1))
        { transform.position = myNavHit.position; }

        NavMeshAgent _agent = gameObject.GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        _maximumLevel = GameObject.Find("GameManager").GetComponent<GameManager>().GetMaximumLevel();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            // change it back when more levels are ready
            if (SceneManager.GetActiveScene().buildIndex < _maximumLevel)
            { _nextSceneToUnlock = (SceneManager.GetActiveScene().buildIndex + 1); }
            else if (SceneManager.GetActiveScene().buildIndex >= _maximumLevel)
            { _nextSceneToUnlock = _maximumLevel; }

            // substitute mm part with subscription when win conditions are assigned to a player despawn animation object
            MenuManagerScript mm = GameObject.Find("MenuManager").GetComponent<MenuManagerScript>();
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            // substitute values in Menu and Game Managers before save data
            mm.LevelProgress[_nextSceneToUnlock] = true;
            gm.LevelProgress[_nextSceneToUnlock] = true;

            gm.SaveGame();

            foreach (Transform tr in collision.gameObject.transform)
            { if (tr.GetComponent<Camera>() != null) { tr.SetParent(null); } }
            Destroy(collision.gameObject);

            if (PlayerDespawnAnimation != null) { Instantiate(PlayerDespawnAnimation, gameObject.transform.position, new Quaternion(), null); }
            Destroy(gameObject);
        }
    }

}
