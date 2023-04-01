using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllowLvl3SpawnScript : MonoBehaviour
{
    public bool PassingAllowsLvl3Spawn = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PassingAllowsLvl3Spawn && collision.tag == "Player")
        { OnLvl3TriggerAllow?.Invoke(); }
    }

    public static event Action OnLvl3TriggerAllow;
}
