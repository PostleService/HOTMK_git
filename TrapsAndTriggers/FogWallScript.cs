using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogWallScript : MonoBehaviour
{
    private LevelManagerScript _levelManager;
    private bool _chasersDeaggroed = false;

    private void Start()
    { _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>(); }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (_levelManager.LevelStage < 2 && !_chasersDeaggroed)
            {
                List<GameObject> goLis = new List<GameObject>();
                EnemyScript[] goArr = GameObject.FindObjectsOfType<EnemyScript>();

                foreach (EnemyScript ES in goArr)
                {
                    if (ES.EnemyLevel == 2 &&
                        ES.EnemyType == EnemyScript.EnemyOfType.Roamer &&
                        ES.CurrentlyAggroed)
                        ES.Deaggro();
                }
            }
            GameObject.Find("FogManager").GetComponent<FogManager>().PlayerInFog(true, gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player") GameObject.Find("FogManager").GetComponent<FogManager>().PlayerInFog(false, gameObject);
    }
}
