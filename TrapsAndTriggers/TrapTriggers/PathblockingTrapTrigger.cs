using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathblockingTrapTrigger : TrapTrigger
{
    public Vector3 SpawnOffset;
    private LevelManagerScript _levelManager;

    [Header("Pass to trap")]
    public bool TeleportBossToCustom = false;
    public Vector3 CustomTeleportDestination = Vector3.zero;

    protected override void SpawnTrap()
    {
        if (!_hasBeenSpawned)
        {
            // for proper layering, pivot on collapsables set lower than needed. Correcting offset through lower y pos
            Vector3 pos;
            pos = new Vector3(transform.position.x + SpawnOffset.x, transform.position.y + SpawnOffset.y, 0);

            _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
            // Remove the tile from a list of AllWalkableTiles in the LevelManager
            // No navmesh recalculation required - using Nav Mesh Obstacle to carve from calculated navmesh
            _levelManager.AllWalkableTiles.Remove(this.gameObject.transform.position);

            if (Trap != null)
            {
                _trap = Instantiate(Trap, pos, new Quaternion(), this.gameObject.transform);

                AnimationEndDetection_PathblockingTrap aed = _trap.GetComponent<AnimationEndDetection_PathblockingTrap>();
                aed.TeleportBossToCustom = TeleportBossToCustom;
                aed.CustomTeleportDestination = CustomTeleportDestination;
            }

            _hasBeenSpawned = true;
        }
    }
}
