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
    public int LevelStage = 0;
    public bool RegularLevelProgressionTracking = true;

    [Tooltip("A number of items of each level the game will attempt to spawn")]
    public List<int> DefaultItemsCount = new List<int>();
    public List<int> _currentItemsCount = new List<int>();

    [Tooltip("A visual representation of levelstage objective: enemy or item")]
    public List<Sprite> LevelStageIcons = new List<Sprite>() { };

    [Header("Walkable Tiles")]
    // COMPILING LIST OF WALKABLE TILES
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

    public delegate void MyHandler(int aLevelStage, int aCurrentItems, int aDefaultItems, Sprite aSprite);
    public static event MyHandler OnLevelStageChange;
    public delegate void Decrementer (int aLevelStage, int aCurrentItems, int aDefaultItems, int aLevelStageItem);
    public static event Decrementer OnObjectiveDecrement;

    private void OnEnable()
    {
        PlayerScript.OnSpawn += RaiseLevelStage;
        PlayerScript.OnEnemiesDeconceal += StopConcealingEnemies;
        EnemyScript.OnSpawn += ReactToEnemySpawn;
        EnemyScript.OnDie += ReactToDeath;
        DarkObeliskScript.OnSpawn += ReactToEnemySpawn;
        DarkObeliskScript.OnDie += ReactToDeath;
    }

    private void OnDisable()
    { Unsubscribe(); }

    // Start is called before the first frame update
    void Start()
    {
        SpawnHealth();
        PlayerSpawner.GetComponent<SpawnerScript>().Activated = true; 
        LocateWalkableTiles();
    }

    // Update is called once per frame
    void FixedUpdate()
    { MonitorItems(); }

    public void Unsubscribe()
    {
        PlayerScript.OnSpawn -= RaiseLevelStage;
        PlayerScript.OnEnemiesDeconceal -= StopConcealingEnemies;
        EnemyScript.OnSpawn -= ReactToEnemySpawn;
        EnemyScript.OnDie -= ReactToDeath;
        DarkObeliskScript.OnSpawn -= ReactToEnemySpawn;
        DarkObeliskScript.OnDie -= ReactToDeath;
    }

    public void ReactToEnemySpawn(int aStageLevel, GameObject aEnemyObject)
    {
        if (aStageLevel == 3) { EnemyLvl3 = aEnemyObject; }
        if (aStageLevel > -1 && aStageLevel < _currentItemsCount.Count) _currentItemsCount[aStageLevel] += 1;
        if (aStageLevel > -1 && aStageLevel < DefaultItemsCount.Count) DefaultItemsCount[aStageLevel] += 1;
    }

    private void ReactToDeath(int aStageLevel, GameObject aGameObject) 
    {
        if (aStageLevel > -1 && aStageLevel < DefaultItemsCount.Count)
        {
            // always decrement current item count even if boss level to track progress to victory
            _currentItemsCount[aStageLevel] -= 1;

            // if not a boss level
            if (RegularLevelProgressionTracking) OnObjectiveDecrement?.Invoke(LevelStage, _currentItemsCount[LevelStage], DefaultItemsCount[LevelStage], aStageLevel);
        }
    }

    // UPDATE FUNCTIONS

    private void MonitorItems()
    {
        if ((LevelStage > - 1 && LevelStage < 3) && _currentItemsCount[LevelStage] <= 0)
        {
            RaiseLevelStage(null);
            if (LevelStage == 2) SpawnLevel3();
        }
        else if (LevelStage == 3) KillCurrentLvl3();
    }

    public void KillCurrentLvl3()
    { if (EnemyLvl3 != null) EnemyLvl3.GetComponent<EnemyScript>().Die(false); }

    public void RequestUIItemUpdate(int aLvlStage, int aCurrItms, int aDefItms, Sprite aSprite)
    { OnLevelStageChange?.Invoke(aLvlStage, aCurrItms, aDefItms, aSprite); }

    public void RaiseLevelStage(GameObject aGo)
    { 
        LevelStage += 1;
        // if not boss level, provide item count and icons based on level stage values
        if (RegularLevelProgressionTracking == true) RequestUIItemUpdate(LevelStage, _currentItemsCount[LevelStage], DefaultItemsCount[LevelStage], LevelStageIcons[LevelStage]);
        // else { GameObject.Find("AudioManager").GetComponent<AudioManager>().ReactToLvlChange(LevelStage, 0, 0, null); }
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

    private void SpawnLevel3()
    { if (Lvl3Spawner != null) Lvl3Spawner.GetComponent<SpawnerScript>().StartSpawnCountdown(); }

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
