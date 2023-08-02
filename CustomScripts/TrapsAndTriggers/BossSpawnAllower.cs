using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawnAllower : MonoBehaviour
{
    public SpawnerScript MonitoredSpawnerScript;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") { MonitoredSpawnerScript.AllowLvl3Spawn = true; }
    }
}
