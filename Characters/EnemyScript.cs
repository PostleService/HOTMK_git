using System;
using System.Collections;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;


public class EnemyScript : MonoBehaviour
{
    private LevelManagerScript _levelManager;

    [HideInInspector]
    public GameObject _player;
    [HideInInspector]
    public NavMeshAgent _agent;
    private float _remainingDistance;
    private Transform _currentPatrolTarget;
    private Transform _currentRushTarget;
    private int _currentPatrolPoint = 0; // also doubles as starting position
    private GameObject _currentFleeSpot;
    private int _currentEscapeAttempt;

    [Header("Pathfinding")]
    public Vector2 SpawnPosition = new Vector2(0, 0);
    public Transform CurrentTarget;
    [Tooltip("List of vectors that will create a patrol path for the npc")]
    public PatrolPathVector[] PatrolPath = new PatrolPathVector[5];
    [System.Serializable]
    public class PatrolPathVector
    {
        public Vector2 vec = new Vector2(0, 0);
    }
    private List<Transform> _patrolTransforms = new List<Transform>();
    private Dictionary<string, int> _areaDict = new Dictionary<string, int>();

    public enum EnemyOfType
    {
        Roamer,
        Rusher,
        Thrower
    }
    public EnemyOfType EnemyType;

    [Header("Behaviour and light")]
    public Color NeutralColor;
    public Color AggroNonFearColor;
    public float NeutralIntensity = 1.35f;
    public float AggroNonFearIntensity = 1.5f;

    [Header("Enemy Behaviour")]
    [Tooltip("This point will autocorrect to within bounds even if accidentally assigned to a point not contained within NavMesh")]
    public GameObject NavMeshBoundPoint;
    [Tooltip("If true - will consider raycast and aggro range. Otherwise - if not by default aggroed - patrolling, if aggroed - always chasing")]
    public bool CanAggrDeaggr = true;
    [Tooltip("Instead of aggroing by raycast and deaggroing by pathfinding distance, aggro and deaggro by raw Vector3 distance")]
    public bool AlternativeAggro = false;
    [Tooltip("Distance at which the enemy will notice player with raycast. In alternative aggro - raw distance to aggro")]
    public float RayCastDistance;
    private float _raycastModifier = 0f; // for when enemy is stunned
    [Tooltip("If set to false - enemy will aggro even through seethrough obstacles")]
    public bool IgnoreSeethroughAggro = true;
    [Tooltip("If set to false - enemy will not aggro through fog")]
    public bool IgnoreFogAggro = true;
    public bool DebugRaycast = false;
    public bool DebugPath = false;
    public bool CurrentlyAggroed = false;
    private bool _currentlyAggroed
    {
        get { return _currentlyAggroed; }
        set 
        {
            if (CurrentlyAggroed != value)
            { 
                CurrentlyAggroed = value;
                AdaptLightingToState(value, IsAfraid);
            }
        }
    }
    public bool IsAfraid = false;
    // for deaggroing the enemy once his state changes, so he can instantly find flee point
    private bool _isAfraid
    { set {
            if (IsAfraid != value)
            {
                IsAfraid = value;
                _currentlyAggroed = false;
            } }
    }
    public bool Stunned = false;
    public float StunTimer = 3f;
    private float _currentStunTimer;
    public bool Slowed = false;
    public float SlowTimer = 3f;
    private float _currentSlowTimer;
    bool _startedRushing = false;
    bool _startedFleeing = false;

    public float FleeDistanceMin = 1f;
    public float FleeDistanceMax = 5f;
    public float FleeSpeedModifier = 1.5f;
    private float _speedBeforeFleeing;
    public int EscapeAttempts = 3;
    [Tooltip("Measures distance in units on the navmesh rather than straight line")]
    public float DistanceToDeaggro;
    
    [Tooltip("Layers which will stop rushing behaviour and item spawn")]
    public LayerMask Layers;

    [Header("Boss behaviour")]
    public bool CurrentlyTeleporting = true;
    public bool AllowedToTeleport = false;
    [Tooltip("Distance in path length at which boss starts counting down to teleport")]
    public float DistanceToTeleport = 6f;
    [Tooltip("If a point on a currently calculated path is located past this distance, it will be the next one that will be taken as a teleport point")]
    public float TeleportToPointPastDistanceOf = 3f;
    [Tooltip("By how many units is upper value of teleport distance bigger.")]
    public float TeleportToPointPastDistanceOfUpper = 1f;
    [Tooltip("Will start countdown to teleport after reaching X distance. Will be interrupted by closing distance during countdown")]
    public float DefaultTeleportCountDown = 3f;
    private float _currentTeleportCountDown;
    [Tooltip("Boolean in control of activating and maintaining countdown")]
    public bool ConsideringTeleport = false;
    private bool _consideringTeleport
    { set 
        {
            if (ConsideringTeleport != value)
            { 
                ConsideringTeleport = value;
                if (value != true)
                { ResetTeleportCountDown(); }
            }
        }
    }
    [HideInInspector] public Vector3 SpawnOutDestination = Vector3.zero;

    [Header("Rusher behaviour")]
    public int RushDistange = 15;
    [Tooltip("Increase to speed when rushing")]
    public float SpeedModifier = 2f;
    private bool _rushTargetInSight;
    private Vector2 _playerDir;
    private float _originalSpeed; // receives value from _agent settings
    public float RushCooldown;
    public float _currentRushCooldown;
    [HideInInspector]
    public bool _onCooldown = false;
    private bool _isRushing = false;

    [Header("Thrower behaviour")]
    public GameObject Projectile;
    public float ThrowCooldown;
    [Tooltip("Throw cooldown when not waiting between consecutive throws")]
    public float ThrowCooldownPrepped = 0.5f;
    [SerializeField]
    private float _currentThrowCooldown;
    public bool SlowsEnemies = false;
    public bool StunsEnemies = true;
    public bool DamagesEnemies = true;
    public bool ModifiersAffectPlayer = false;
    public float Speed;
    public float ProjectileLifetime;
    
    [Tooltip("Basic projectile collision without enemy layer. If interacts with enemies in any way, projectile will add enemy layer on its own")]
    public LayerMask ProjectileCollision;
    [Tooltip("If hit by slow, slow down to X of speed")]
    public float SlowToSpeed = 0.5f;
    private float _speedBeforeStun;
    private float _speedBeforeSlow;
    
    public bool _performingThrow = false;

    [Header("Enemy Stats")]
    public int EnemyLevel = 0;
    [Tooltip("Correspond to game stages rather then level itself.")]
    public int ItemStageLevel = 0;
    [Tooltip("In case we ever wanted to change how much damage an enemy deals on contact")]
    public int DamagePerHit = 1;

    [Header("Victory")]
    public GameObject DeathObject;

    void Start()
    {
        AdaptLightingToState(false, false);
        ResetStunTimer();
        ResetSlowTimer();
        ResetRushCooldown();
        ResetThrowCooldownPrepped();
        ResetTeleportCountDown();

        AssignPlayer();
        BecomeAgentAndSpawn();
        PathfindingLayersConversion();
        DropStartingPatrolPoints();
        
        _currentEscapeAttempt = 1;
        _currentPatrolTarget = _patrolTransforms[_currentPatrolPoint];

        _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        _levelManager._currentItemsCount[ItemStageLevel] += 1;
        _levelManager.DefaultItemsCount[ItemStageLevel] += 1;

        if (EnemyLevel == 3) { _levelManager.EnemyLvl3 = this.gameObject; }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetRemainingDistance();
        Aggro();
        Patrolling();
        Flee();
        FollowTarget();
        AssessTeleportDistance();

        StunTimerDecrement();
        SlowTimerDecrement();
        TeleportDecrement();

        if (EnemyType == EnemyOfType.Rusher) { Rush(); RushCooldownDecrement(); }
        if (
            EnemyType == EnemyOfType.Thrower && 
            CurrentlyAggroed && !IsAfraid &&
            !_performingThrow
            )
        { ThrowCooldownDecrement(); }

        if (Stunned) _raycastModifier = 0f;
        else if (!Stunned) _raycastModifier = 1f;

        if (_player != null) 
        { if (_player.GetComponent<PlayerScript>().PlayerLevel >= EnemyLevel) { _isAfraid = true;} }
        
        DrawPath();
    }

    private void OnEnable()
    { PlayerScript.OnSpawn += AssignPlayer; }

    private void OnDisable()
    { PlayerScript.OnSpawn -= AssignPlayer; }

    #region START FUNCTIONS
    private void AssignPlayer()
    { _player = GameObject.Find("Player"); }

    // In order to use layermask, its decimal representation needs to be converted to binary for Unity to read
    public void PathfindingLayersConversion()
    {
        List<string> nOfLayers = new List<string> { "Walkable", "WalkableWhenAngry", "OnlyRusherAndLvl3" };
        List<int> binOfLayers = new List<int>();
        foreach (string str in nOfLayers)
        { 
            int i = 1 << NavMesh.GetAreaFromName(str);
            string iTobin = System.Convert.ToString(i, 2);
            int binToInt = System.Convert.ToInt32(iTobin);
            binOfLayers.Add(binToInt);
        }

        for (int i = 0; i < nOfLayers.Count; i++)
        {
            _areaDict.Add(nOfLayers[i], binOfLayers[i]);
        }
    }

    private void DropStartingPatrolPoints()
    {
        for (int i = 0;  i < PatrolPath.Length; i++)
        {
            GameObject spawnedChild = Instantiate(NavMeshBoundPoint, new Vector3(PatrolPath[i].vec.x, PatrolPath[i].vec.y, 0), new Quaternion(), GameObject.Find("PatrolPathPointHolder").transform);
            spawnedChild.name = this.gameObject.name + ", patrol point " + i;
            _patrolTransforms.Add(spawnedChild.transform);
        }
    }

    private void BecomeAgentAndSpawn()
    {
        _agent = this.gameObject.GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.radius = 0.005f;
        _agent.Warp(SpawnPosition);

    }
    #endregion START FUNCTIONS

    #region BEHAVIOURS

    public void FollowTarget()
    {

        if (CurrentlyAggroed)
        {
            if (!IsAfraid)
            {
                // NavMesh layers
                if (EnemyLevel == 1) 
                { _agent.areaMask = _areaDict["Walkable"];}
                
                if (EnemyLevel == 2 && (EnemyType == EnemyOfType.Roamer || EnemyType == EnemyOfType.Thrower))
                { _agent.areaMask = _areaDict["Walkable"] + _areaDict["WalkableWhenAngry"]; }
                if (EnemyLevel == 2 && (EnemyType == EnemyOfType.Rusher))
                { _agent.areaMask = _areaDict["Walkable"] + _areaDict["WalkableWhenAngry"] + _areaDict["OnlyRusherAndLvl3"]; }

                if (EnemyLevel == 3)
                { _agent.areaMask = _areaDict["Walkable"] + _areaDict["WalkableWhenAngry"] + _areaDict["OnlyRusherAndLvl3"]; }

                // NavMesh agent targets
                if (EnemyType == EnemyOfType.Roamer)
                {
                    if (EnemyLevel != 3) { if (_player != null) { CurrentTarget = _player.transform; } }
                    // boss should stand still while teleporting
                    else
                    {
                        if (CurrentlyTeleporting) { CurrentTarget = gameObject.transform; }
                        else { if (_player != null) { CurrentTarget = _player.transform; } }
                    }
                } // walks to player

                else if (EnemyType == EnemyOfType.Rusher)
                { if (_currentRushTarget != null) { CurrentTarget = _currentRushTarget; } } // rushes to rushpoint

                else if (EnemyType == EnemyOfType.Thrower)
                { CurrentTarget = gameObject.transform; } // stands still

            }
            else if (IsAfraid)
            {
                if (_currentFleeSpot != null)
                {
                    CurrentTarget = _currentFleeSpot.transform;
                    _agent.areaMask = _areaDict["Walkable"];
                }
            }
        }
        else
        {
            // stand still if on cooldown
            if (EnemyType == EnemyOfType.Rusher && _onCooldown)
            { CurrentTarget = gameObject.transform; }
            
            else if (_currentPatrolTarget != null)
            { CurrentTarget = _currentPatrolTarget; _agent.areaMask = _areaDict["Walkable"]; }
        }

        if (CurrentTarget != null) { _agent.SetDestination(CurrentTarget.position); }
        // sets target to itself and stands still
        else
        { CurrentTarget = gameObject.transform; _agent.SetDestination(CurrentTarget.position); }
    }
 
    public (bool, Vector3, Vector3) RayCast()
    {
        RaycastHit2D colliderHit;
        LayerMask layerMask;
        List<string> colliderHitList = new List<string>();
        List<Vector2> vectorList = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };

        foreach (Vector2 vector in vectorList)
        {
            if (DebugRaycast == true)
            { Debug.DrawRay(transform.position, (vector * RayCastDistance * _raycastModifier), Color.red); }

            // for some reason feeding the usual bitwise representation doesn't work for this motherfucker
            // so shifting bits manually. the right side is the number of the layer if we need to add more
            // the | is a bitwise addition
            // if the enemy is a rusher, it disregards raycast through ObstaclesSeethrough
            
            // progressively add more bit layers through checks
            layerMask = (1 << 6) | (1 << 8) | (1 << 10);
            if (IgnoreSeethroughAggro) layerMask = layerMask | (1 << 13);
            if (IgnoreFogAggro) layerMask = layerMask | (1 << 15);

            colliderHit = Physics2D.Raycast(transform.position, vector, RayCastDistance * _raycastModifier, layerMask);

            if (colliderHit.collider != null) 
            { colliderHitList.Add(colliderHit.collider.name); }
        }

        if (colliderHitList.Any())
        {
            bool result = false;
            Vector3 enemyPos = Vector3.zero;
            Vector3 playerPos = Vector3.zero;

            foreach (string col in colliderHitList)
            {
                if (col == "Player") 
                { 
                    result = true;
                    enemyPos = this.gameObject.transform.position;
                    playerPos = _player.transform.position;
                }
            }
            if (result == true) { return (true, playerPos, enemyPos ); }
            else return (false, Vector3.zero, Vector3.zero);
        }

        return (false, Vector3.zero, Vector3.zero);
    }

    public void Aggro()
    {
        if (CanAggrDeaggr)
        {
            void Deaggro()
            {
                if (_currentFleeSpot != null && IsAfraid)
                {
                    Destroy(_currentFleeSpot);
                    _currentEscapeAttempt = 1;
                }
                if (_currentRushTarget != null) { Destroy(_currentRushTarget.gameObject); }
                ResetThrowCooldownPrepped();

                _currentlyAggroed = false;
            }
            
            if (_player == null) { Deaggro(); }
            
            else if (!CurrentlyAggroed)
            {
                if (!IsAfraid) 
                {
                    if (EnemyType == EnemyOfType.Roamer)
                    {
                        if (AlternativeAggro && _player != null)
                        {
                            if (Vector3.Distance(transform.position, _player.transform.position) <= RayCastDistance)
                            { _currentlyAggroed = true; }
                        }
                        else if (!AlternativeAggro)
                        {
                            if (RayCast().Item1)
                            { _currentlyAggroed = true; }
                        }
                    }

                    else if (EnemyType == EnemyOfType.Rusher)
                    {
                        if (!_onCooldown && RayCast().Item1) // only allow aggro if not on cooldown
                        {
                            _currentlyAggroed = true;
                            FindRushPoint(GetDirection(RayCast().Item2, RayCast().Item3));
                        }
                    }

                    else if (EnemyType == EnemyOfType.Thrower)
                    {
                        if (RayCast().Item1)
                        {  _currentlyAggroed = true; }
                    }
   
                }

                else if (IsAfraid)
                {
                    if (AlternativeAggro)
                    {
                        if (Vector3.Distance(transform.position, _player.transform.position) <= RayCastDistance)
                        {
                            FindFleeSpot(GetDirection(_player.transform.position, transform.position));
                            _speedBeforeFleeing = _agent.speed;
                            _agent.speed = _speedBeforeFleeing * FleeSpeedModifier;
                        }
                    }
                    else if (!AlternativeAggro)
                    {
                        if (RayCast().Item1)
                        {
                            FindFleeSpot(GetDirection(RayCast().Item2, RayCast().Item3));
                            _speedBeforeFleeing = _agent.speed;
                            _agent.speed = _speedBeforeFleeing * FleeSpeedModifier;
                        }
                    }
                }
            }

            else if (CurrentlyAggroed)
            {
                if (_player == null) { Deaggro(); }

                if (!IsAfraid)
                {
                    if (EnemyType == EnemyOfType.Roamer)
                    {
                        if (AlternativeAggro && Vector3.Distance(transform.position, _player.transform.position) > RayCastDistance && Vector3.Distance(transform.position, _player.transform.position) != Mathf.Infinity)
                        { Deaggro(); }

                        // Have to account for infinity because of navmesh rebuilding. While it's rebuilding, distance to target becomes incalculable
                        else if (!AlternativeAggro && _remainingDistance > DistanceToDeaggro && _remainingDistance != Mathf.Infinity)
                        { Deaggro(); }
                    }
                    else if (EnemyType == EnemyOfType.Rusher)
                    {
                        if (!_isRushing && AlternativeAggro && Vector3.Distance(transform.position, _player.transform.position) > RayCastDistance && Vector3.Distance(transform.position, _player.transform.position) != Mathf.Infinity)
                        { Deaggro(); }

                        // Have to account for infinity because of navmesh rebuilding. While it's rebuilding, distance to target becomes incalculable
                        else if (!_isRushing && !AlternativeAggro && _remainingDistance > DistanceToDeaggro && _remainingDistance != Mathf.Infinity)
                        { Deaggro(); }
                    }
                    else if (EnemyType == EnemyOfType.Thrower)
                    {
                        if (!IsAfraid && !RayCast().Item1) { Deaggro(); }
                    }

                }
                else if (IsAfraid)
                {
                    if (AlternativeAggro && Vector3.Distance(transform.position, _player.transform.position) > RayCastDistance && Vector3.Distance(transform.position, _player.transform.position) != Mathf.Infinity)
                    { 
                        Deaggro();
                        if (Stunned) { _agent.speed = 0; }
                        else if (Slowed) { _agent.speed = _speedBeforeFleeing * SlowToSpeed; }
                        else { _agent.speed = _speedBeforeFleeing; }
                    }

                    // Have to account for infinity because of navmesh rebuilding. While it's rebuilding, distance to target becomes incalculable
                    else if (!AlternativeAggro && _remainingDistance > DistanceToDeaggro && _remainingDistance != Mathf.Infinity)
                    { 
                        Deaggro();
                        if (Stunned) { _agent.speed = 0; }
                        else if (Slowed) { _agent.speed = _speedBeforeFleeing * SlowToSpeed; }
                        else { _agent.speed = _speedBeforeFleeing; }
                    }
                }

            }
        }
    }

    public void Flee()
    {
        if (IsAfraid)
        {
            Vector2 trposV2 = transform.position;
            Vector2 trposTarV2 = Vector2.zero;

            if (_currentFleeSpot != null) { trposTarV2 = _currentFleeSpot.transform.position; }
            if (_currentFleeSpot != null)
            {
                if (trposV2 == trposTarV2)
                {
                    Destroy(_currentFleeSpot);
                    if (_currentEscapeAttempt < EscapeAttempts)
                    {
                        _currentEscapeAttempt += 1;
                        FindFleeSpot(GetDirection(_player.transform.position, transform.position));
                    }
                    else
                    {
                        _currentEscapeAttempt = 1;
                        _currentlyAggroed = false;
                        if (Stunned) { _agent.speed = 0; }
                        else if (Slowed) { _agent.speed = _speedBeforeFleeing * SlowToSpeed; }
                        else { _agent.speed = _speedBeforeFleeing; }
                    }
                }
            }
        }
    }

    public void Patrolling()
    {
        Vector2 trposV2 = transform.position;
        Vector2 trposTarV2 = _currentPatrolTarget.position;
        if (trposV2 == trposTarV2)
        {
            if (_currentPatrolPoint < PatrolPath.Length - 1)
            {
                _currentPatrolPoint += 1;
            }
            else
            { _currentPatrolPoint = 0; }
            _currentPatrolTarget = _patrolTransforms[_currentPatrolPoint];
        }
    }

    public void Rush()
    {
        Vector2 trposV2 = transform.position;
        Vector2 trposTarV2 = Vector2.zero;

        if (_currentRushTarget != null)
        {
            void Deaggro()
            {
                if (Stunned) { _agent.speed = 0; }
                else if (Slowed) { _agent.speed = _originalSpeed * SlowToSpeed; }
                else { _agent.speed = _originalSpeed; }
                
                Destroy(_currentRushTarget.gameObject);
                _currentlyAggroed = false;
                _isRushing = false;
            }
            
            trposTarV2 = _currentRushTarget.transform.position;
            _isRushing = true;
            RayCastRush();

            // Deaggro patterns:
            if (!_rushTargetInSight) { Deaggro(); } // if lost sight to rushTarget - be ready to aggro instantly
            if (trposV2 == trposTarV2) { Deaggro(); EnterRushCooldown(); } // if reached rush point - enter cooldown
        }
    }

    public void EnterRushCooldown()
    { _onCooldown = true; }

    // Delegated to animation script
    public void PerformThrow(Vector2Int aPlayerPos)
    {
        GameObject go = Instantiate(Projectile, new Vector3((transform.position.x + aPlayerPos.x), (transform.position.y + aPlayerPos.y), 0f), new Quaternion(), GameObject.Find("Lvl2EnemyHolder").transform);
        ProjectileScript ps = go.GetComponent<ProjectileScript>();
        ps.SlowsEnemies = SlowsEnemies;
        ps.StunsEnemies = StunsEnemies;
        ps.DamagesEnemies = DamagesEnemies;
        ps.ModifiersAffectPlayer = ModifiersAffectPlayer;
        ps.Damage = DamagePerHit;
        ps.Direction = aPlayerPos;
        ps.Speed = Speed;
        ps.ProjectileLifetime = ProjectileLifetime;
        ps.ProjectileCollision = ProjectileCollision;

    }

    public void AssessTeleportDistance()
    {
        if (AllowedToTeleport)
        {
            if (_remainingDistance > DistanceToTeleport) { _consideringTeleport = true; }
            else { _consideringTeleport = false; }
        }
    }

    #endregion // BEHAVIOURS

    #region AUXILIARY BEHAVIOURS

    public void RayCastRush()
    {
        Vector2 direction = _playerDir;

        Debug.DrawRay(transform.position, _playerDir * Vector3.Distance(this.gameObject.transform.position, _currentRushTarget.position), Color.yellow);

        // for some reason feeding the usual bitwise representation doesn't work for this motherfucker
        // so shifting bits manually. the right side is the number of the layer if we need to add more
        // the | is a bitwise addition
        LayerMask layerMask = ((1 << 6) | (1 << 11) | (1 << 13));
        RaycastHit2D colliderHit = Physics2D.Raycast(transform.position, _playerDir, Vector3.Distance(this.gameObject.transform.position, _currentRushTarget.position), layerMask);
        if (colliderHit.collider != null)
        {
            if (colliderHit.collider.tag == "RushTarget") { _rushTargetInSight = true; }
            else { _rushTargetInSight = false; }
        }
    }

    private void FindRushPoint(Vector2Int aPlayerPos)
    {
        _playerDir = aPlayerPos;
        Vector2Int playerDirection = aPlayerPos;
        Vector2Int posWhenPlayerDetected  = Vector2Int.RoundToInt((transform.position));
        Vector2Int currentlySearchedCell = posWhenPlayerDetected;
        List<Vector2Int> searchedCells = new List<Vector2Int>();

        Vector2Int RushPointPosition = Vector2Int.zero;

        bool obstacleEncountered = false;

        for (int i = 0; i < RushDistange && !obstacleEncountered; i++) {
            currentlySearchedCell = posWhenPlayerDetected + (playerDirection * i);
            if (!Physics2D.OverlapCircle(currentlySearchedCell, 0.2f, Layers))
            { searchedCells.Add(currentlySearchedCell); }
            else 
            { obstacleEncountered = true; RushPointPosition = searchedCells[i - 1]; }
            if (!obstacleEncountered) { RushPointPosition = searchedCells[i]; }
        }

        if (_currentRushTarget != null) { Destroy(_currentRushTarget.gameObject); }
        
        int rushTargetLayer = LayerMask.NameToLayer("RushTarget");
        GameObject rushObject = Instantiate(NavMeshBoundPoint, new Vector3(RushPointPosition.x, RushPointPosition.y, 0), new Quaternion(), GameObject.Find("RushPointHolder").transform);
        rushObject.name = this.gameObject.name + ", rush point";
        rushObject.layer = rushTargetLayer; rushObject.tag = "RushTarget";
        BoxCollider2D col2D = rushObject.AddComponent<BoxCollider2D>();
        col2D.isTrigger = true; col2D.size = new Vector2(1f,1f);
        _currentRushTarget = rushObject.transform;
        _rushTargetInSight = true;
        _originalSpeed = _agent.speed;
        _agent.speed = _originalSpeed * SpeedModifier;
    }

    /// <summary>
    /// As of Unity 2019.3, NavMeshAgent.remainingDistance is still calculated only after the penultimate 
    /// corner of the path has been reached, and the agent is traversing the last segment.
    /// Before that, remainingDistance will return infinity. Sadly, this is undocumented.
    /// https://stackoverflow.com/questions/61421172/why-does-navmeshagent-remainingdistance-return-values-of-infinity-and-then-a-flo
    /// </summary>
    public void GetRemainingDistance()
    {
        float distance = 0;
        Vector3[] corners = _agent.path.corners;

        if (corners.Length > 2)
        {
            for (int i = 1; i < corners.Length; i++)
            {
                Vector2 previous = new Vector2(corners[i - 1].x, corners[i - 1].z);
                Vector2 current = new Vector2(corners[i].x, corners[i].z);
                distance += Vector2.Distance(previous, current);
            }
        }
        else
        {
            distance = _agent.remainingDistance;
        }
        _remainingDistance = distance;
    }

    public void DrawPath()
    {
        if (DebugPath)
        { 
            Vector3[] corners = _agent.path.corners;
            if (corners.Length > 0)
            {
                float colorR = 1;
                for (int i = 1; i < corners.Length; i++)
                {
                    Color col = new Color(colorR, 0, 1, 1);

                    if (i < corners.Length - 1)
                    { Debug.DrawLine(_agent.path.corners[i], _agent.path.corners[i + 1], col, 0.2f,true); }
                    if (colorR == 1f) { colorR = 0f; }
                    else { colorR = 1f; }
                }
            }
        }
    }

    public void FindFleeSpot(Vector2Int aPlayerPos)
    {
        List<int> acceptableXList = new List<int>();
        List<int> acceptableYList = new List<int>();

        List<Vector3> tilesWithinDistance = new List<Vector3>();
        List<Vector3> tilesAwayFromPlayer = new List<Vector3>();

        int randomNumber = 0;
        Vector3 fleeSpotPosition = Vector3.zero;

        if (aPlayerPos.x < 0) { acceptableXList.Add(0); acceptableXList.Add(1); }
        else if (aPlayerPos.x == 0) { acceptableXList.Add(-1); acceptableXList.Add(0); acceptableXList.Add(1); }
        else if (aPlayerPos.x > 0) { acceptableXList.Add(-1); acceptableXList.Add(0); }

        if (aPlayerPos.y < 0) { acceptableYList.Add(0); acceptableYList.Add(1); }
        else if (aPlayerPos.y == 0) { acceptableYList.Add(-1); acceptableYList.Add(0); acceptableYList.Add(1); }
        else if (aPlayerPos.y > 0) { acceptableYList.Add(-1); acceptableYList.Add(0); }

        foreach (Vector3 vec3 in _levelManager.AllWalkableTiles)
        {
            float distance = (Vector3.Distance(vec3, this.gameObject.transform.position));
            if ((distance > FleeDistanceMin) && (distance <= FleeDistanceMax))
            { tilesWithinDistance.Add(vec3); }
        }
        if (tilesWithinDistance.Any())
        {
            foreach (Vector3 vec3 in tilesWithinDistance)
            {
                Vector2Int direction = GetDirection(vec3, this.gameObject.transform.position);
                if (acceptableXList.Contains(direction.x) && acceptableYList.Contains(direction.y))
                { tilesAwayFromPlayer.Add(vec3); }
            }
        }

        // flee somewhere within distance that is away from player
        if (tilesAwayFromPlayer.Any())
        {
            randomNumber = new System.Random().Next(tilesAwayFromPlayer.Count);
            fleeSpotPosition = tilesAwayFromPlayer[randomNumber];
        }
        // flee somewhere that is within distance
        else if (tilesWithinDistance.Any())
        {
            randomNumber = new System.Random().Next(tilesWithinDistance.Count);
            fleeSpotPosition = tilesWithinDistance[randomNumber];
        }
        // just flee somewhere on the map 
        else
        {
            randomNumber = new System.Random().Next(_levelManager.AllWalkableTiles.Count);
            fleeSpotPosition = _levelManager.AllWalkableTiles[randomNumber];
        }

        if (_currentFleeSpot != null) { Destroy(_currentFleeSpot); }

        GameObject fleeObject = Instantiate(NavMeshBoundPoint, new Vector3(fleeSpotPosition.x, fleeSpotPosition.y, 0), new Quaternion(), GameObject.Find("FleePointHolder").transform);
        fleeObject.name = this.gameObject.name + ", flee point " + _currentEscapeAttempt;
        _patrolTransforms.Add(fleeObject.transform);
        _currentFleeSpot = fleeObject;
        _currentlyAggroed = true;
    }

    public Vector2Int GetDirection(Vector3 aTargetPos, Vector3 aEnemyPos)
    {
        float posX = aTargetPos.x - aEnemyPos.x;
        float posY = aTargetPos.y - aEnemyPos.y;
        float angle = Mathf.Atan2(posY, posX) * Mathf.Rad2Deg;

        GameObject temp = new GameObject();
        temp.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        Vector2 dirTemp = temp.transform.up;
        Vector2Int direction = Vector2Int.RoundToInt(dirTemp);
        Destroy(temp);

        return direction;
    }
    #endregion AUXILIARY BEHAVIOURS

    #region ON CALL BEHAVIOURS

    public void Die() 
    {
        if (_currentFleeSpot != null) { Destroy(_currentFleeSpot); }
        if (_currentRushTarget != null) { Destroy(_currentRushTarget.gameObject); }
        _levelManager._currentItemsCount[ItemStageLevel] -= 1;
        Destroy(this.gameObject);
        if (DeathObject != null) { Instantiate(DeathObject, transform.position, Quaternion.identity, GameObject.Find("EnemyCorpseHolder").transform); }
        if (EnemyLevel == 3) { GameObject.Find("FogManager").GetComponent<FogManager>().DespawnAllFog(); }
    }

    public void Stun()
    { 
        if (!Stunned) 
        {
            _startedRushing = _isRushing;
            if (CurrentlyAggroed && IsAfraid)
            { _startedFleeing = true; }
            else { _startedFleeing = true; }

            Stunned = true;
            
            if (Slowed) _speedBeforeStun = _speedBeforeSlow;
            else if (!Slowed) _speedBeforeStun = _agent.speed;

            _agent.speed = 0f;
        } 
    }

    // Enemies that are already stunned won't slow not to mess up speed calc
    public void Slow()
    {
        _startedRushing = _isRushing;
        if (CurrentlyAggroed && IsAfraid)
        { _startedFleeing = true; }
        else { _startedFleeing = true; }

        if (!Slowed) {
            if (Stunned) return;
            else 
            {
                Slowed = true;
                _speedBeforeSlow = _agent.speed;
                _agent.speed = _agent.speed * SlowToSpeed;
            } 
        }
    }

    public void TeleportToSpawn()
    {
        CurrentlyTeleporting = true;
        gameObject.GetComponent<EnemyAnimation_Boss>().ExecuteAnimationSpawnOut();
        SpawnOutDestination = new Vector3(SpawnPosition.x, SpawnPosition.y, 0);
    }

    public void AdaptLightingToState(bool aAggression, bool aFear)
    {
        Light2D L2Delem = gameObject.GetComponent<Light2D>();
        if (aAggression == true && aFear != true)
        { 
            L2Delem.color = AggroNonFearColor;
            L2Delem.intensity = AggroNonFearIntensity;
        }
        else
        {
            L2Delem.color = NeutralColor;
            L2Delem.intensity = NeutralIntensity;
        }
    }

    public void PerformBossTeleport()
    {
        if (!_agent.pathPending &&
        _agent.pathStatus != NavMeshPathStatus.PathInvalid &&
        _agent.path.corners.Length != 0)
        {
            float distanceWithNext = 0.0f;
            float distanceFromCurrent = 0.0f;
            float distanceTemp = 0.0f;
            Vector3 tempBetterTeleportPoint = Vector3.zero;

            // start calculating distance from end of path (player), till start of path (boss)
            // if distance exceeds value TeleportToPointPastDistanceOf: 
            // go through as many loops as needed to find midpoint between current point and next point that would be close to desired distance
            for (int i = _agent.path.corners.Length-1; i > 1; --i)
            {
                distanceFromCurrent = distanceWithNext;
                distanceWithNext += Vector3.Distance(_agent.path.corners[i], _agent.path.corners[i - 1]);
                distanceTemp = distanceWithNext;

                if (distanceWithNext > TeleportToPointPastDistanceOf)
                {
                    int AttemptNum = 0;
                    Vector3 TempDestination = _agent.path.corners[i - 1];

                    while (distanceTemp > (TeleportToPointPastDistanceOf+TeleportToPointPastDistanceOfUpper))
                    {
                        // find X, Y midpoints, shifting closer to the next point than current point (closer to player)
                        tempBetterTeleportPoint = Vector3.Lerp(_agent.path.corners[i], TempDestination, 0.75f);
                        distanceTemp = distanceFromCurrent + Vector3.Distance(_agent.path.corners[i], tempBetterTeleportPoint);

                        // assign new Vector to the previously found "better teleport point" in case of further iteration
                        TempDestination = tempBetterTeleportPoint;
                        AttemptNum += 1;
                    }

                    /*
                    // Leaving useful debugging in, just in case
                    Debug.LogWarning("Instead of teleporting to " + _agent.path.corners[i - 1] + ", will be teleporting to " + TempDestination);
                    Debug.LogWarning("Instead of teleporting to distance of " + distanceWithNext + ", will be teleporting to " + distanceTemp);
                    Debug.LogWarning("Approximations taken to recalculate teleport point: " + AttemptNum);
                    */
                    if (EnemyType == EnemyOfType.Roamer && EnemyLevel == 3)
                    {
                        CurrentlyTeleporting = true;
                        gameObject.GetComponent<EnemyAnimation_Boss>().ExecuteAnimationSpawnOut();
                        SpawnOutDestination = new Vector3(TempDestination.x, TempDestination.y, 0);
                    }
                    else _agent.Warp(new Vector3(TempDestination.x, TempDestination.y,0)); // in case we still want to teleport someone and it's not a boss
                    break;
                }
            }
        }
    }

    #endregion // ON CALL BEHAVIOURS

    #region TIMER DECREMENTS
    public void StunTimerDecrement()
    { 
        if (Stunned) 
        {
            if (_currentStunTimer >= 0) { _currentStunTimer -= Time.deltaTime; }
            else 
            { 
                ResetStunTimer(); 
                Stunned = false;

                if (_startedRushing && !_isRushing)
                { _agent.speed = _speedBeforeStun / SpeedModifier; _startedRushing = false; }
                else { _agent.speed = _speedBeforeStun; }

                if (_startedFleeing && !CurrentlyAggroed && IsAfraid)
                { _agent.speed = _speedBeforeStun / FleeSpeedModifier; _startedFleeing = false; }
                else { _agent.speed = _speedBeforeStun; }

            }
        }
    }

    public void SlowTimerDecrement()
    {
        if (Slowed)
        {
            if (_currentSlowTimer >= 0) { _currentSlowTimer -= Time.deltaTime;}
            else 
            { 
                ResetSlowTimer(); 
                Slowed = false;
                // only reset speed back to previous values if not stunned - otherwise, wait till stun expires so it sets speed
                if (!Stunned) 
                {
                    if (_startedRushing && !_isRushing)
                    { _agent.speed = _speedBeforeSlow / SpeedModifier; _startedRushing = false; }
                    else { _agent.speed = _speedBeforeSlow; }

                    if (_startedFleeing && !CurrentlyAggroed && IsAfraid)
                    { _agent.speed = _speedBeforeSlow / FleeSpeedModifier; _startedFleeing = false; }
                    else { _agent.speed = _speedBeforeSlow; }
                } 
            }
        }
    }

    public void RushCooldownDecrement()
    {
        if (_currentRushCooldown >= 0 && _onCooldown) { _currentRushCooldown -= Time.deltaTime; }
        else if (_currentRushCooldown < 0)
        { ResetRushCooldown(); _onCooldown = false; }
    }

    public void ThrowCooldownDecrement()
    {
        if (_currentThrowCooldown >= 0 && !Stunned) { _currentThrowCooldown -= Time.deltaTime; }
        else if (_currentThrowCooldown < 0)
        {
            ResetThrowCooldownBetween();

            // At the end of this animation, projectile will spawn and timer countdown will be reenabled
            if (EnemyType == EnemyOfType.Thrower)
            { gameObject.GetComponent<EnemyAnimation_Thrower>().ExecuteThrowAnimation(); }
        }
    }

    public void TeleportDecrement()
    {
        if (ConsideringTeleport)
        {
            if (_currentTeleportCountDown >= 0) { _currentTeleportCountDown -= Time.deltaTime; }
            else 
            {
                // placeholder behaviour:
                PerformBossTeleport();
            }
        }
        
        
    }

    #endregion TIMER DECREMENTS

    #region TIMER RESETS
    public void ResetStunTimer() { _currentStunTimer = StunTimer; }
    public void ResetSlowTimer() { _currentSlowTimer = SlowTimer; }

    public void ResetRushCooldown()
    {
        if (EnemyType == EnemyOfType.Rusher)
        { _currentRushCooldown = RushCooldown; }
    }

    public void ResetThrowCooldownPrepped()
    {
        if (EnemyType == EnemyOfType.Thrower)
        {
            _currentThrowCooldown = ThrowCooldownPrepped;
            gameObject.GetComponent<AnimationEndDetection_Thrower>().CanExecuteAgain = true;
        }
    }

    public void ResetThrowCooldownBetween()
    {
        if (EnemyType == EnemyOfType.Thrower)
        {
            _currentThrowCooldown = ThrowCooldown;
            gameObject.GetComponent<AnimationEndDetection_Thrower>().CanExecuteAgain = true;
        }
    }

    public void ResetTeleportCountDown()
    { _currentTeleportCountDown = DefaultTeleportCountDown; }

    #endregion // TIMER RESETS
}
