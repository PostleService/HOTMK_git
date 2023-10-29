using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMultipleGatesRemoveShadows : MonoBehaviour
{
    public List<Vector2> SpawnLocations = new List<Vector2>();
    public GameObject GatePrefab;
    public GameObject PlayerMoverPrefab;
    public GameObject DeactivateUponGateSpawn;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && _triggered == false)
        {
            if (GatePrefab != null && PlayerMoverPrefab != null)
            {
                foreach (Vector2 vec2 in SpawnLocations)
                {
                    Instantiate(GatePrefab, vec2, new Quaternion(), GameObject.Find("TrapsAndTriggers").transform);
                    Instantiate(PlayerMoverPrefab, vec2, new Quaternion(), GameObject.Find("PlayerMovers").transform);
                }

                _triggered = true;
                if (DeactivateUponGateSpawn != null) DeactivateUponGateSpawn.SetActive(false);
                Destroy(gameObject);
            }
        
        }
    }
}
