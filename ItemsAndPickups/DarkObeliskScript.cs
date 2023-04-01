using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        if (!_levelManager._playerCanSeeThroughWalls) { this.gameObject.GetComponent<SpriteRenderer>().enabled = false; }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            if (!_incremented)
            {
                Destroy(this.gameObject);
                if (DeathObject != null) { Instantiate(DeathObject, gameObject.transform.position, new Quaternion(), GameObject.Find("EnemyCorpseHolder").transform); }
                _levelManager._currentItemsCount[ItemStageLevel] -= 1;
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
                    if (DeathObject != null) { Instantiate(DeathObject, gameObject.transform.position, new Quaternion(), GameObject.Find("EnemyCorpseHolder").transform); }
                    _levelManager._currentItemsCount[ItemStageLevel] -= 1;
                    _incremented = true;
                    
                }
            }
        }
    }

}
