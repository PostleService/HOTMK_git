using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_PlayerSpawnLvl3 : AnimationEndDetection
{
    [Header("Player settings")]
    [Tooltip("Player prefab")]
    public GameObject Player;
    public int PlayerLevel;
    public bool AllowPlayerToLevel;

    [Header("Visibility settings")]
    public bool RememberFog = false;
    public bool SeeThroughWalls = false;
    public bool DespawnAllFog = false;

    public override void OnAnimationFinish()
    { 
        Destroy(gameObject);

        if (Player != null)
        {
            GameObject go = Instantiate(Player, transform.position, new Quaternion());
            go.name = Player.name; // make sure Player spawns with name of prefab, no (clone)
            if (go.tag == "Player")
            {
                PlayerScript ps = go.GetComponent<PlayerScript>();
                ps.PlayerLevel = PlayerLevel - 1; // correction for actual numbering
                ps.AllowLevelUp = AllowPlayerToLevel;

                // This wouldn't have had to exist if my code hadn't been such ass

                go.AddComponent<PlayerLvl3_PostSpawnFogSettings>();
                PlayerLvl3_PostSpawnFogSettings postSpawnScript = go.GetComponent<PlayerLvl3_PostSpawnFogSettings>();
                postSpawnScript.RememberFog = RememberFog;
                postSpawnScript.SeeThroughWalls = SeeThroughWalls;
                postSpawnScript.DespawnAllFog = DespawnAllFog;
            }
        }
    }

}
