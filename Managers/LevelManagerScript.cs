using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class LevelManagerScript : MonoBehaviour
{
    private Tilemap[] _tilemapsArray;
    private Tilemap _tilemap;
    [HideInInspector]
    public bool _playerDead = false;
    public bool _playerCanSeeThroughWalls = false;

    [Header("Spawners")]
    public GameObject PlayerSpawner;
    public GameObject Lvl3Spawner;

    [Header("Level Progression")]
    [Tooltip("Five in total: items_lvl1,mobs_lvl1,items_lvl2,mobs_lvl2,endgame_lvl3")]
    public int LevelStage = -1;

    [Tooltip("A number of items of each level the game will attempt to spawn")]
    public List<int> DefaultItemsCount = new List<int>();
    public List<int> _currentItemsCount = new List<int>();

    public GameObject ObeliskPrefab;

    // ITEM SPAWN
    public List<Vector3> Lvl1ItemLocations = new List<Vector3>();
    [Tooltip("A distance in units below which items do not spawn close to each other")]
    public float ItemSpawnDistance = 5;
    [Tooltip("Layers which will be discarded when trying to spawn items. Walls, collision")]
    public LayerMask ObstacleLayers;
    public LayerMask ItemLayers;

    [Tooltip("lower bottom values of bounds that are to be excluded from all Walkable tiles")]
    public List<Vector2> ZonesToExcludeMinVal = new List<Vector2>() { };
    [Tooltip("upper top values of bounds that are to be excluded from all Walkable tiles")]
    public List<Vector2> ZonesToExcludeMaxVal = new List<Vector2>() { };
    
    public List<Vector3> AllWalkableTiles = new List<Vector3>();

    private float _colliderSearchRadiusObstacle = 0.2f; // renders best results and least false positives
    
    [HideInInspector]
    public GameObject EnemyLvl3 = null;

    [Header("Player Health")]
    public GameObject HealthItem;
    public List<Vector2> PossibleHealthLocations = new List<Vector2>(5);
    [Tooltip("By how many items is the pool of potential healing items decreased")]
    public int HealthCountDecrement = 2;

    public delegate void MyHandler(int aLevelStage, int aCurrentItems, int aDefaultItems);
    public static event MyHandler OnLevelStageChange;

    private void OnEnable()
    {
        PlayerScript.OnSpawn += SynchronizePlayerLevel;
        PlayerScript.OnEnemiesDeconceal += StopConcealingEnemies;
        AllowLvl3SpawnScript.OnLvl3TriggerAllow += AllowLvl3ToSpawn;
        EnemyScript.OnSpawn += ReactToEnemySpawn;
        EnemyScript.OnDie += ReactToDeath;
        DarkObeliskScript.OnDie += ReactToDeath;
    }

    private void OnDisable()
    {
        PlayerScript.OnSpawn -= SynchronizePlayerLevel;
        PlayerScript.OnEnemiesDeconceal -= StopConcealingEnemies;
        AllowLvl3SpawnScript.OnLvl3TriggerAllow -= AllowLvl3ToSpawn;
        EnemyScript.OnSpawn -= ReactToEnemySpawn;
        EnemyScript.OnDie -= ReactToDeath;
        DarkObeliskScript.OnDie -= ReactToDeath;
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnHealth();
        PlayerSpawner.GetComponent<SpawnerScript>().Activated = true; 
        LocateWalkableTiles();
        SpawnItems();
    }

    // Update is called once per frame
    void FixedUpdate()
    { MonitorItems(); }

    private void SynchronizePlayerLevel(GameObject aGameObject)
    { aGameObject?.GetComponent<PlayerScript>().SyncronizeLevelUps(LevelStage); }

    private void AllowLvl3ToSpawn()
    { Lvl3Spawner.GetComponent<SpawnerScript>().AllowLvl3Spawn = true; }

    public void ReactToEnemySpawn(int aStageLevel, GameObject aEnemyObject)
    {
        if (aStageLevel == 3) { EnemyLvl3 = aEnemyObject; }
        _currentItemsCount[aStageLevel] += 1;
        DefaultItemsCount[aStageLevel] += 1;
    }

    private void ReactToDeath(int aStageLevel, GameObject aGameObject) 
    { _currentItemsCount[aStageLevel] -= 1; }

    // UPDATE FUNCTIONS

    private void MonitorItems()
    {
        if (LevelStage > - 1 && LevelStage < 3)
        {
            if (_currentItemsCount[LevelStage] <= 0)
            {
                RaiseLevelStage();
                if (LevelStage == 2) { SpawnLevel3(LevelStage); }
            }
        }

        else if (LevelStage == 3)
        { if (EnemyLvl3 != null) 
            {  EnemyLvl3.GetComponent<EnemyScript>().Die(); } 
        }
    }

    private void RaiseLevelStage()
    {
        LevelStage += 1;
        OnLevelStageChange?.Invoke(LevelStage, _currentItemsCount[LevelStage], DefaultItemsCount[LevelStage]);
    }

    public void StopConcealingEnemies() { _playerCanSeeThroughWalls = true; }

    private void LocateWalkableTiles()
    {
        _tilemapsArray = GameObject.FindObjectsOfType<Tilemap>();
        foreach (Tilemap tlmp in _tilemapsArray)
        {
            if (tlmp.name == "Ground")
            { _tilemap = tlmp; }
        }

        // make a list of vectors of all tiles within sectors to be discarded from item spawn
        List<Vector2> VectorsToDiscard = new List<Vector2>() { };
        for (int i = 0; i < ZonesToExcludeMinVal.Count; i++)
        {
            for (float x = ZonesToExcludeMinVal[i].x; x <= ZonesToExcludeMaxVal[i].x; x++)
            {
                for (float y = ZonesToExcludeMinVal[i].y; y <= ZonesToExcludeMaxVal[i].y; y++)
                { VectorsToDiscard.Add(new Vector2(x, y)); }
            }
        }

        // if I get lost for any reason, original grid cellcize was 1 with tiles = 32x32 pixels
        Grid grid = FindObjectOfType<Grid>();
        float scaleX = grid.cellSize.x; float scaleY = grid.cellSize.y;

        Bounds tlmpBounds = _tilemap.GetComponent<TilemapRenderer>().bounds;
        float boundsXmin = tlmpBounds.min.x; float boundsXmax = tlmpBounds.max.x;
        float boundsYmin = tlmpBounds.min.y; float boundsYmax = tlmpBounds.max.y;

        // Because the bounds start at the corner and we are searching for centers of tiles in a tile scale of 1, we need a formula below
        for (float x = (boundsXmin + (scaleX / 2)); x <= (boundsXmax - (scaleX / 2)); x += scaleX)
        {
            for (float y = (boundsYmin + (scaleY / 2)); y <= (boundsYmax - (scaleY / 2)); y += scaleY)
            {
                // if the value is not contained within a list of zones to be excluded and the check confirms none of the colliders from undesireable layers are present - add tile to AllWalkableTiles
                if (!Physics2D.OverlapCircle(new Vector2(x, y), _colliderSearchRadiusObstacle, ObstacleLayers) && (!VectorsToDiscard.Contains(new Vector2(x, y))))
                { AllWalkableTiles.Add(new Vector3(x, y, 0)); }
            }
        }
    }

    // even if no items are spawned, it will still increment lvl stage form -1, which will initiate monitoring process and give time for enemies to increment respective counts
    private void SpawnItems()
    {
        int nItemsToSpawn = DefaultItemsCount[0]; // take level stage and check corresponding index within the DefaultItemsCount list
        int randomNumber = new System.Random().Next(AllWalkableTiles.Count);
        Vector3 itemSpawnLocation = Vector3.zero;

        for (int i = 0; i < nItemsToSpawn; i++)
        {
            int IterationsOfManual = -1;
            IterationsOfManual = Lvl1ItemLocations.Count();
            
            // create a temp list of vectors, checking whether any of them are close to already spawned items
            List<Vector3> tempV3List = new List<Vector3>();

            if (i < IterationsOfManual)
            {
                itemSpawnLocation = Lvl1ItemLocations[i];

                GameObject itemToSpawn = Instantiate(ObeliskPrefab, itemSpawnLocation, new Quaternion(), GameObject.Find("ItemHolder").transform);
                _currentItemsCount[0] += 1;
                itemToSpawn.GetComponent<DarkObeliskScript>()._levelManager = this.gameObject.GetComponent<LevelManagerScript>();
            }

            // If we have no manually placed locations or run out of predefined item location List positions and the nItemsToSpawn is still iterating, switch to random
            else
            {
                // for each item we spawn. If index = 0, spawn right away at random location
                // otherwise, list only vectors far enough from other items, pick random, spawn, repeat.
                if (i == 0)
                { itemSpawnLocation = AllWalkableTiles[randomNumber]; }
                else
                {
                    foreach (Vector3 vec3 in AllWalkableTiles)
                    {
                        if (!Physics2D.OverlapCircle(vec3, ItemSpawnDistance, ItemLayers))
                        { tempV3List.Add(vec3); }
                    }
                    // if cannot place any more items with distance limitations set - will search ever smaller circles until can populate temp list
                    while (!tempV3List.Any())
                    {
                        for (int x = (int)ItemSpawnDistance - 1; x >= 0; x -= 1)
                        {
                            foreach (Vector3 vec3 in AllWalkableTiles)
                            {
                                if (!Physics2D.OverlapCircle(vec3, x, ItemLayers))
                                { tempV3List.Add(vec3); }
                            }
                        }
                    }
                    randomNumber = new System.Random().Next(tempV3List.Count);
                    itemSpawnLocation = tempV3List[randomNumber];
                }
                GameObject itemToSpawn = Instantiate(ObeliskPrefab, itemSpawnLocation, new Quaternion(), GameObject.Find("ItemHolder").transform);
                _currentItemsCount[0] += 1;
                itemToSpawn.GetComponent<DarkObeliskScript>()._levelManager = this.gameObject.GetComponent<LevelManagerScript>();
            }
        }
        RaiseLevelStage();
    }

    private void SpawnLevel3(int aLevelStage)
    { Lvl3Spawner.GetComponent<SpawnerScript>().StartSpawnCountdown(); }

    public void SpawnHealth()
    {
        // raffle X positions out of Y available in PossibleHealthLocations
        int HowManyToRaffle;
        if (PossibleHealthLocations.Count > 0 && (PossibleHealthLocations.Count - HealthCountDecrement) > 0)
        { HowManyToRaffle = PossibleHealthLocations.Count - HealthCountDecrement; }
        else HowManyToRaffle = 0;

        List<int> RaffledNumbers = new List<int>();
        while (RaffledNumbers.Count < HowManyToRaffle && HowManyToRaffle > 0)
        {
            int RandNumber = new System.Random().Next(0, PossibleHealthLocations.Count);
            if (!RaffledNumbers.Contains(RandNumber)) { RaffledNumbers.Add(RandNumber); }
            else continue;
        }

        foreach (int i in RaffledNumbers) 
        {
            if (HealthItem != null && PossibleHealthLocations[i] != null)
            {
                Instantiate(HealthItem, PossibleHealthLocations[i], new Quaternion(), GameObject.Find("ItemHolder").transform);
            }
        }
    }
}
