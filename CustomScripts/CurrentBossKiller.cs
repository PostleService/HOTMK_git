using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentBossKiller : MonoBehaviour
{
    private LevelManagerScript _lm;

    // Start is called before the first frame update
    private void Start()
    { _lm = GameObject.FindObjectOfType<LevelManagerScript>(); }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        { _lm.KillCurrentLvl3(); }
    }
}
