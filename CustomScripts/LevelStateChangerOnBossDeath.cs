using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStateChangerOnBossDeath : MonoBehaviour
{
    private AudioManager _am;
    public int LevelStage = 0;
    public bool _hasbeentriggered = false;
    public GameObject _lvl3 = null;

    private void OnEnable()
    {
        SpawnerScript.OnBossSpawn += AssignBoss;
        EnemyScript.OnDie += ReactToDeath;
    }

    private void OnDisable()
    {
        SpawnerScript.OnBossSpawn -= AssignBoss;
        EnemyScript.OnDie -= ReactToDeath;
    }

    // Start is called before the first frame update
    void Start()
    {
        _am = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    private void AssignBoss(GameObject aGO)
    {
        _lvl3 = aGO;
        _hasbeentriggered = true;
    }

    private void ReactToDeath(int aLvlStage, GameObject aGameObject)
    {
        if (_lvl3 != null && aGameObject == _lvl3)
        { 
            _am.ReactToLvlChange(LevelStage, 0, 0, null);
        }
    }

}
