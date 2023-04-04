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
        FogManager _fm = GameObject.Find("FogManager").GetComponent<FogManager>();

        if (Player != null)
        {
            GameObject go = Instantiate(Player, transform.position, new Quaternion());
            go.name = Player.name; // make sure Player spawns with name of prefab, no (clone)
            if (go.tag == "Player")
            {
                PlayerScript ps = go.GetComponent<PlayerScript>();
                ps.PlayerLevel = PlayerLevel - 1; // correction for actual numbering
                ps.AllowLevelUp = AllowPlayerToLevel;

                if (RememberFog) ps.RememberFog();
                if (SeeThroughWalls) ps.DeconcealEnemies();
                if (DespawnAllFog) _fm.DespawnAllFog(3, null);
            }
        }
    }

}
