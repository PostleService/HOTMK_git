using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DarkObeliskScript : MonoBehaviour
{
    [HideInInspector]
    public LevelManagerScript _levelManager;
    [Tooltip("Correspond to game stages rather then level itself.")]
    public int ItemStageLevel;

    public GameObject DeathObject;

    [Header("Rendering")]
    public float CountdownDerenderDefault = 3.0f;
    public float _countdownDerenderCurrent;

    private bool _incremented = false;
    [HideInInspector]public bool SendSpawnIncrementInfo = true; // only for boss levels

    public delegate void MyHandler(int aItemStageLevel, GameObject aGameObject);
    public static event MyHandler OnDie;
    public static event MyHandler OnSpawn;

    private void Start()
    {
        if (_levelManager == null) _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        if (!_levelManager._playerCanSeeThroughWalls) { this.gameObject.GetComponent<SpriteRenderer>().enabled = false; }
        if (SendSpawnIncrementInfo) OnSpawn?.Invoke(ItemStageLevel, this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            if (!_incremented)
            {
                Destroy(this.gameObject);
                if (DeathObject != null) { Instantiate(DeathObject, gameObject.transform.position, new Quaternion(), GameObject.Find("ItemEndStateHolder").transform); }
                OnDie?.Invoke(ItemStageLevel, this.gameObject);
                _incremented = true;
            }
        }
        else if (collision.tag == "Trap")
        {
            if (collision.gameObject.GetComponent<TrapScript>().Collapsable == true)
            {
                if (!_incremented)
                {
                    Destroy(this.gameObject);
                    if (DeathObject != null) { Instantiate(DeathObject, gameObject.transform.position, new Quaternion(), GameObject.Find("ItemEndStateHolder").transform); }

                    OnDie?.Invoke(ItemStageLevel, this.gameObject);
                    _incremented = true;
                }
            }
        }
    }



}
