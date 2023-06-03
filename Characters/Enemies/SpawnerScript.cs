using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    public enum EnemyVsPlayer 
    { 
        Enemy,
        Player
    };
    public EnemyVsPlayer EnemyOrPlayer;

    [Header("General")]
    public GameObject ObjectToSpawn;
    public Vector3 Coordinates;
    public float Delay;
    public bool Activated = false;
    public bool DestroyAfterSpawn = true;
    private float _delayDecremented;
    private bool _instantiated = false;

    public bool AllowLvl3Spawn = false;

    private void Start()
    { _delayDecremented = Delay; }

    private void FixedUpdate()
    {
        if (Activated && _instantiated != true) { Countdown(); }
    }

    public void StartSpawnCountdown()
    { Activated = true; }

    public void SpawnObject()
    {
        /* 
         * if object is an enemy, its spawn position will have to be set prior to instantiation
         * because pathfinding will warp him to default coordinates otherwise
        */
        if (ObjectToSpawn != null && _instantiated == false)
        {
            if (EnemyOrPlayer == EnemyVsPlayer.Enemy && AllowLvl3Spawn)
            {
                GameObject go = Instantiate(ObjectToSpawn, Coordinates, new Quaternion(), null);
                EnemyScript es = go.GetComponent<EnemyScript>();
                es.SpawnPosition.x = Coordinates.x;
                es.SpawnPosition.y = Coordinates.y;
                
                EnemyScript.PatrolPathVector ppv = new EnemyScript.PatrolPathVector();
                ppv.vec = new Vector2(Coordinates.x, Coordinates.y);
                es.PatrolPath[0] = ppv;

                go.name = ObjectToSpawn.name;
                
                _instantiated = true;
                if (DestroyAfterSpawn) { Destroy(gameObject); }
            }
            // otherwise, if a player - just instantiate with spawner position
            else if (EnemyOrPlayer == EnemyVsPlayer.Player)
            {
                GameObject go = Instantiate(ObjectToSpawn, Coordinates, new Quaternion());
                go.name = ObjectToSpawn.name;

                _instantiated = true;
                if (DestroyAfterSpawn) { Destroy(gameObject); }
            }
        }
    }

    public void Countdown()
    {
        if (_delayDecremented >= 0) { _delayDecremented -= Time.deltaTime; }
        else if (_delayDecremented < 0) { SpawnObject(); }
    }

}
