using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDespawnMonitor : MonoBehaviour
{
    private GameObject _lvl3;
    private LevelManagerScript _lm;

    private void OnEnable()
    {
        EnemyScript.OnSpawn += AssignLvl3;
        BossLevelArenaDecrementMonitor.OnDestroy += KillLvl3;
    }

    private void OnDisable()
    { 
        EnemyScript.OnSpawn -= AssignLvl3;
        BossLevelArenaDecrementMonitor.OnDestroy -= KillLvl3;
    }

    private void Start()
    { _lm = GameObject.FindObjectOfType<LevelManagerScript>(); }

    private void AssignLvl3(int aItemStageLevel, GameObject aEnemyObject)
    { if (aItemStageLevel == 3) _lvl3 = aEnemyObject; }

    private void KillLvl3(GameObject aGameObject) { _lm.KillCurrentLvl3(); }
}
