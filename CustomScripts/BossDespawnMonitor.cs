using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDespawnMonitor : MonoBehaviour
{
    private LevelManagerScript _lm;

    private void OnEnable()
    {
        BossLevelArenaDecrementMonitor.OnDestroy += KillLvl3;
    }

    private void OnDisable()
    { BossLevelArenaDecrementMonitor.OnDestroy -= KillLvl3; }

    private void Start()
    { _lm = GameObject.FindObjectOfType<LevelManagerScript>(); }

    private void KillLvl3(GameObject aGameObject) { _lm.KillCurrentLvl3(); }
}
