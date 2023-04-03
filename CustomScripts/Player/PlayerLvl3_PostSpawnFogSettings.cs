using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLvl3_PostSpawnFogSettings : MonoBehaviour
{
    public bool RememberFog = false;
    public bool SeeThroughWalls = false;
    public bool DespawnAllFog = false;

    private LevelManagerScript _lms;
    private FogManager _fm;

    // Start is called before the first frame update
    void Start()
    {
        _lms = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        _fm = GameObject.Find("FogManager").GetComponent<FogManager>();

        ExecuteCommands();
        Destroy(this);
    }

    private void ExecuteCommands()
    {
        if (RememberFog) { _fm.StopConcealingFog(); }
        if (SeeThroughWalls) { _lms.StopConcealingEnemies(); }
        if (DespawnAllFog) { _fm.DespawnAllFog(4); }
    }
}
