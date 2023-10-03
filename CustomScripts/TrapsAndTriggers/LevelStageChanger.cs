using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStageChanger : MonoBehaviour
{
    private AudioManager _am;
    public int LevelStage = 0;

    // Start is called before the first frame update
    void Start()
    {
        _am = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") { _am.ReactToLvlChange(LevelStage, 0, 0, null); }
    }
}
