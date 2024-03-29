using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DestructiblesScript : MonoBehaviour
{
    [Tooltip("This is a costly function processing-wise.")]
    public bool RebuildNavmeshUponDestruction = true;
    [Tooltip("Change tiles around upon destruction")]
    public bool ChangeTilesAround = true;
    [Tooltip("For when we have to instantiate effects of destruction. First prefab get assigned a copy of tilechanger. So in objects with just one element - assign only this one")]
    public GameObject DestructionPrefab1;
    public GameObject DestructionPrefab2;
    private NavMeshSurface _navMesh;
    private LevelManagerScript _levelManager;

    private void Awake()
    {
        _navMesh = FindObjectOfType<NavMeshSurface>();
        _levelManager = FindObjectOfType<LevelManagerScript>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    { if (collision.tag == "Enemy") Crumble(collision.gameObject);  }

    // placeholder to spawn additional animation upon death
    private void Crumble(GameObject aGameObject)
    {
        // Spawn destruction animation, add a tilechanger script with the exact same settings to not have to make destruction prefabs
        GameObject destructPrefab = Instantiate(DestructionPrefab1, new Vector3(transform.position.x, transform.position.y, 0), new Quaternion(), GameObject.Find("DestructiblesHolder").transform);
        
        List<Transform> trlis = new List<Transform>();
        trlis.Add(destructPrefab.transform);
        foreach (Transform chtr in destructPrefab.transform) { trlis.Add(chtr); }

        foreach (Transform tr in trlis)
        {
            if (tr.gameObject.GetComponent<DestructibleWallAnimationEffectSpawner>() != null)
            {
                DestructibleWallAnimationEffectSpawner dwaes = tr.gameObject.GetComponent<DestructibleWallAnimationEffectSpawner>();
                dwaes.EnemyCausingDestruction = aGameObject;
            }
            if (tr.gameObject.GetComponent<AnimationEndDetection_CrackedWall>() != null)
            {
                if (tr.gameObject.GetComponent<AnimationEndDetection_CrackedWall>().Main == true)
                {
                    TileChangerScript tchScrTo = tr.gameObject.AddComponent<TileChangerScript>();
                    TileChangerScript tchScrFr = this.gameObject.GetComponent<TileChangerScript>();
                    for (int i = 0; i < tchScrTo.TilesToSpawn.Length; i++)
                    { tchScrTo.TilesToSpawn[i] = tchScrFr.TilesToSpawn[i]; }
                    tchScrTo.NameOfTilemap = tchScrFr.NameOfTilemap;
                }
            }
        }

        // destroy and rebuild navmesh
        Destroy(this.gameObject);
        if (RebuildNavmeshUponDestruction) { RebuildNavmesh(); }
    }

    // Very costly to call resource-wise, it seems, but it's one time and it updates the navmesh
    // If it breaks the pace too much - get rid of it. It won't change the gameplay too much
    private void RebuildNavmesh()
    {
        // set the area to walkable before rebuilding navmesh
        // int representing walkable area may change from build to build, but as long as all areas come in the same order - should be fine
        GetComponent<NavMeshModifier>().area = 0; 
        _navMesh.BuildNavMesh();
        // Add the tile to a list of AllWalkableTiles in the LevelManager
        _levelManager.AllWalkableTiles.Add(this.gameObject.transform.position);
    }

}
