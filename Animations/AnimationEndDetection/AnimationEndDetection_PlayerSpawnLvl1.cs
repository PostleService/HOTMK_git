using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AnimationEndDetection_PlayerSpawnLvl1 : AnimationEndDetection
{
    [Tooltip("Player prefab")]
    public GameObject Player;
    public int PlayerLevel;
    public bool AllowPlayerToLevel;

    public override void OnAnimationFinish()
    { 
        Destroy(gameObject);

        if (Player != null)
        {
            GameObject go = Instantiate(Player, transform.position, new Quaternion());
            go.name = Player.name; // make sure Player spawns with name of prefab, no (clone)
            PlayerScript ps = go.GetComponent<PlayerScript>();
            ps.PlayerLevel = PlayerLevel-1; // correction for actual numbering
            ps.AllowLevelUp = AllowPlayerToLevel;
        }
    }

}
