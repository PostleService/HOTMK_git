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
    private FogManager _fogManager;

    [HideInInspector] public GameObject _player;
    [HideInInspector] public NavMeshAgent _agent;
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
    public class PatrolPathVector { public Vector2 vec = new Vector2(0, 0); }
    private List<Transform> _patrolTransforms = new List<Transform>();
    private Dictionary<string, int> _areaDict = new Dictionary<string, int>();

    public enum EnemyOfType
    {
        Roamer,
        Rusher,
        Thrower,
        Boss
    }
    public EnemyOfType EnemyType;

    [Header("Behaviour and light")]
    public Color NeutralColor;
    public Color AggroNonFearColor;
    public float NeutralIntensity = 1.35f;
    public float AggroNonFearIntensity = 1.5f;

    [Header("Universal Enemy Behaviour")]
    [Tooltip("This point will autocorrect to within bounds even if accidentally assigned to a point not contained within NavMesh")]
    public GameObject NavMeshBoundPoint;
    [Tooltip("If true - will consider raycast and aggro range. Otherwise - if not by default aggroed - patrolling, if aggroed - always chasing")]
    public bool CanAggrDeaggr = true;
    public bool CanBeScared = true;
    public bool HasMinimalAggroTime = false;
    public float DefaultMinimalAggroTimer = 1.5f;
    private float _currentMinimalAggroTimer;

    [Tooltip("Distance at which the enemy will notice player with raycast. In alternative aggro - raw distance to aggro")]
    public float RayCastDistance;
    private float _raycastModifier = 0f; // for when enemy is stunned
    [Tooltip("If set to false - enemy will aggro even through seethrough obstacles")]
    public bool IgnoreSeethroughAggro = true;
    [Tooltip("If set to false - enemy will not aggro through fog")]
    public bool IgnoreFogAggro = true;

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
    [HideInInspector] public bool _playerSighted = false;

    public bool IsAfraid = false;
    // for deaggroing the enemy once his state changes, so he can instantly find flee point
    private bool _isAfraid
    { set {
            if (IsAfraid != value && CanBeScared == true)
            {
                IsAfraid = value;
                _currentlyAggroed = false;
                _isRushing = false;
                if (_currentRushTarget != null) Destroy(_currentRushTarget.gameObject);
                gameObject.GetComponent<Light2D>().lightOrder = 2;
            } }
    }
    public bool Stunned = false;
    private float _currentStunTimer;
    public bool Slowed = false;
    private float _currentSlowTimer;

    public float FleeDistanceMin = 1f;
    public float FleeDistanceMax = 5f;
    public float FleeSpeedModifier = 1.5f;
    public int EscapeAttempts = 3;
    [Tooltip("Measures distance in units on the navmesh rather than straight line")]
    public float DistanceToDeaggro;
    
    [Header("LayerMasks")]
    [Tooltip("Layers which will stop rushing behaviour")]
    public LayerMask RushPointLayers;
    private LayerMask _raycastLayers;
    private LayerMask _rushSightLayers;

    [Header("Speed")]
    [Tooltip("Speed with no modifiers applied")] public float DefaultSpeed = 3;
    [Tooltip("Speed modifier when scared")] public float SpeedModifier_Flee = 1.15f;
    [Tooltip("Speed modifier when rushing (only rusher)")] public float SpeedModifier_Rush = 1.5f;
    [Tooltip("Speed modifier when slowed")] public float SpeedModifier_Slow = 0.33f;
    [Tooltip("Speed modifier when stunned")] public float SpeedModifier_Stun = 0f;
    [Tooltip("For throwers and rushers on cooldown, and boss teleport")] private float SpeedModifier_Special = 0f;

    private float _currSpeed;
    private float _currSpeedModifier_Flee = 1; private float _currSpeedModifier_Rush = 1;
    private float _currSpeedModifier_Slow = 1; private float _currSpeedModifier_Stun = 1;
    private float _currSpeedModifier_Special = 1;

    [Header("Boss behaviour")]
    public GameObject TeleportVoidZone;
    [HideInInspector] public GameObject _currentTPVoidZone;
    public bool CurrentlyTeleporting = false;
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

    private bool _rushTargetInSight;
    private Vector2 _playerDir;
    public float RushCooldown;
    public float _currentRushCooldown;
    [HideInInspector] public bool _onCooldown = false;
    private bool _isRushing = false;

    [Header("Thrower behaviour")]
    public GameObject Projectile;
    public float ThrowCooldown;
    [Tooltip("Throw cooldown when not waiting between consecutive throws")]
    public float ThrowCooldownPrepped = 0.5f;
    private float _currentThrowCooldown;
    public bool[] Slows = new bool[] { false, false };
    public bool[] Stuns = new bool[] { false, false };
    public bool[] Damages = new bool[] { false, false };
    public float[] SlowFor = new float[] { 2.15f, 1.125f };
    public float[] StunFor = new float[] { 2.15f, 1.75f };
    public int DamagesFor = 1;

    public float ProjectileSpeed;
    public float ProjectileLifetime;
    [HideInInspector] public bool _seesPlayer = false;
    
    [Tooltip("Basic projectile collision without enemy layer. If interacts with enemies in any way, projectile will add enemy layer on its own")]
    public LayerMask ProjectileCollision;
    [Tooltip("If hit by slow, slow down to X of speed")]
    public float SlowToSpeed = 0.5f;
    
    public bool _performingThrow = false;

    [Header("Enemy Stats")]
    public int EnemyLevel = 0;
    [Tooltip("Correspond to game stages rather then level itself.")]
    public int ItemStageLevel = 0;
    [Tooltip("In case we ever wanted to change how much damage an enemy deals on contact")]
    public int DamagePerHit = 1;

    private StatusEffectScript _statusEffect;

    [Header("Victory")]
    public GameObject DeathObject;
    public GameObject DeathObjectNoCorpse;

    public delegate void MyHandler(int aItemStageLevel, GameObject aEnemyObject);
    public static event MyHandler OnDie;
    public static event MyHandler OnSpawn;
    public delegate void PositionTracker (GameObject aGameObject, Vector2 aPosition);
    public static event PositionTracker OnPositionChange;

    private bool _diedOnce = false;

    [Header("Debug")]
    public bool DebugRaycast = false;
    public bool DebugPath = false;

    private void OnEnable()
    { 
        PlayerScript.OnSpawn += AssignPlayer;
        PlayerScript.OnLevelUp += ReactToPlayerLevelUp;
        AnimationEndDetection_PlayerDeath.OnDie += ReactToPlayerDeath;
        PathblockingTrapTrigger.OnTrapSpawn += AssignTargetToFollow;
    }

    private void OnDisable()
    { 
        PlayerScript.OnSpawn -= AssignPlayer;
        PlayerScript.OnLevelUp -= ReactToPlayerLevelUp;
        AnimationEndDetection_PlayerDeath.OnDie -= ReactToPlayerDeath;
        PathblockingTrapTrigger.OnTrapSpawn -= AssignTargetToFollow;
    }

    void Start()
    {
        _statusEffect = GetComponentInChildren<StatusEffectScript>();
        AdaptLightingToState(false, false);

        ResetMinimalAggroCooldown();
        ResetRushCooldown();
        ResetThrowCooldownPrepped();
        ResetTeleportCountDown();

        BecomeAgentAndSpawn();
        PathfindingLayersConversion();
        DropStartingPatrolPoints();

        RaycastLayerMaskCreation();

        _currentEscapeAttempt = 1;
        _currentPatrolTarget = _patrolTransforms[_currentPatrolPoint];
        SpawnOutDestination = SpawnPosition;

        _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
        _fogManager = GameObject.Find("FogManager").GetComponent<FogManager>();

        OnSpawn?.Invoke(ItemStageLevel, this.gameObject);
        if (ItemStageLevel == 3) { OnPositionChange?.Invoke(this.gameObject, transform.position); }
        AssignPlayer(GameObject.Find("Player"));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetRemainingDistance();
        RayCast();
        AggroAndFlee();
        Patrol();
        Flee();
        AssessTeleportDistance();

        MinimalAggroTimerDecrement();
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

        if (_agent != null) _agent.speed = MonitorSpeed();

        if (Stunned) _raycastModifier = 0f; else _raycastModifier = 1f;

        if (ItemStageLevel == 3) OnPositionChange?.Invoke(this.gameObject, transform.position);
    }

    #region START FUNCTIONS
    private void AssignPlayer(GameObject aGameObject) { _player = aGameObject; }

    public void RaycastLayerMaskCreation()
    {
        _raycastLayers = ((1 << 6) | (1 << 8) | (1 << 10));
        if (IgnoreSeethroughAggro) _raycastLayers = _raycastLayers | (1 << 13);
        if (IgnoreFogAggro) _raycastLayers = _raycastLayers | (1 << 15);

        _rushSightLayers = ((1 << 6) | (1 << 11) | (1 << 13));
    }

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
        _agent.radius = 0.002f;
        _agent.Warp(SpawnPosition);

        _currSpeed = DefaultSpeed;
        _agent.speed = _currSpeed;
    }
    #endregion START FUNCTIONS

    #region BEHAVIOURS

    public float MonitorSpeed()
    {
        if (IsAfraid && CurrentlyAggroed) _currSpeedModifier_Flee = SpeedModifier_Flee;
        else _currSpeedModifier_Flee = 1;
        if (_isRushing) _currSpeedModifier_Rush = SpeedModifier_Rush;
        else _currSpeedModifier_Rush = 1;
        if (Slowed) _currSpeedModifier_Slow = SpeedModifier_Slow;
        else _currSpeedModifier_Slow = 1;
        if (Stunned) _currSpeedModifier_Stun = SpeedModifier_Stun;
        else _currSpeedModifier_Stun = 1;

        if (CurrentlyTeleporting || _seesPlayer || _onCooldown) { _currSpeedModifier_Special = SpeedModifier_Special; }
        else _currSpeedModifier_Special = 1;

        _currSpeed = DefaultSpeed * _currSpeedModifier_Flee * _currSpeedModifier_Rush * _currSpeedModifier_Slow * _currSpeedModifier_Stun * _currSpeedModifier_Special;
        return _currSpeed;
    }

    public void AssignTargetToFollow(Transform aTarget)
    {
        if (aTarget != null) CurrentTarget = aTarget; // otherwise, use current target
        _agent.SetDestination(CurrentTarget.position);
    }

    public void RayCast()
    {
        if (CanAggrDeaggr)
        {
            RaycastHit2D colliderHit;
            List<string> colliderHitList = new List<string>();
            List<Vector2> vectorList = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };

            foreach (Vector2 vector in vectorList)
            {
                if (DebugRaycast == true)
                { Debug.DrawRay(transform.position, (vector * RayCastDistance * _raycastModifier), Color.red); }

                colliderHit = Physics2D.Raycast(transform.position, vector, RayCastDistance * _raycastModifier, _raycastLayers);

                if (colliderHit.collider != null)
                { colliderHitList.Add(colliderHit.collider.name); }
            }

            if (colliderHitList.Any())
            {
                foreach (string col in colliderHitList)
                {
                    if (col == "Player")
                    {
                        _playerSighted = true;
                        break;
                    }
                    else _playerSighted = false;
                }
            }
        }
    }

    void CommonAggroSettings()
    {
        _currentlyAggroed = true;
        gameObject.GetComponent<Light2D>().lightOrder = 5;
        gameObject.GetComponent<EnemySoundScript>().PlayAggroSound();
        ChangeNavMeshMasks(CurrentlyAggroed, IsAfraid);
    }

    public void Deaggro()
    {
        if (_currentFleeSpot != null && IsAfraid)
        {
            Destroy(_currentFleeSpot);
            _currentEscapeAttempt = 1;
        }
        if (_currentRushTarget != null) 
        { 
            Destroy(_currentRushTarget.gameObject);
            _isRushing = false;
        }
        if (EnemyType == EnemyOfType.Thrower) { _seesPlayer = false; ResetThrowCooldownPrepped(); }
        if (EnemyType == EnemyOfType.Roamer) ResetMinimalAggroCooldown();
        

        _playerSighted = false;
        _currentlyAggroed = false;

        if (!IsAfraid) gameObject.GetComponent<Light2D>().lightOrder = 4;

        ChangeNavMeshMasks(CurrentlyAggroed, IsAfraid);
        AssignTargetToFollow(_currentPatrolTarget);
    }

    public void AggroAndFlee()
    {
        if (CanAggrDeaggr)
        {
            if (_player == null) { Deaggro(); }

            else if (!CurrentlyAggroed)
            {
                if (!IsAfraid)
                {
                    if (EnemyType == EnemyOfType.Roamer)
                    {
                        if (_playerSighted == true && _fogManager._playerInsideFog != true)
                        { CommonAggroSettings(); }
                    }

                    else if (EnemyType == EnemyOfType.Rusher)
                    {
                        if (!_onCooldown && _playerSighted == true && _fogManager._playerInsideFog != true) // only allow aggro if not on cooldown
                        {
                            CommonAggroSettings();
                            FindRushPoint(GetDirection(_player.transform.position, transform.position));
                        }
                    }

                    else if (EnemyType == EnemyOfType.Thrower)
                    {
                        if (_playerSighted == true && _fogManager._playerInsideFog != true)
                        {
                            CommonAggroSettings();
                            _seesPlayer = true;
                        }
                    }

                }

                else if (IsAfraid)
                {
                    if (_playerSighted == true && _fogManager._playerInsideFog != true)
                    {
                        FindFleeSpot(GetDirection(_player.transform.position, transform.position));
                        ChangeNavMeshMasks(CurrentlyAggroed, IsAfraid);
                        gameObject.GetComponent<EnemySoundScript>().PlayFearSound();
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
                        if (_player != null) AssignTargetToFollow(_player.transform);
                        if (_remainingDistance > DistanceToDeaggro && _remainingDistance != Mathf.Infinity)
                        {
                            if (HasMinimalAggroTime == false) Deaggro();
                            else if (HasMinimalAggroTime == true && _currentMinimalAggroTimer <= 0) Deaggro();
                        }
                    }
                    else if (EnemyType == EnemyOfType.Rusher)
                    {
                        // Have to account for infinity because of navmesh rebuilding. While it's rebuilding, distance to target becomes incalculable
                        if (!_isRushing) { Deaggro(); }
                    }
                    else if (EnemyType == EnemyOfType.Thrower)
                    {
                        if (!IsAfraid && !_playerSighted) { Deaggro(); }
                    }

                }
                else if (IsAfraid)
                {
                    // Have to account for infinity because of navmesh rebuilding. While it's rebuilding, distance to target becomes incalculable
                    if (_remainingDistance > DistanceToDeaggro && _remainingDistance != Mathf.Infinity)
                    { Deaggro(); }
                }

            }
        }
        else if (!CanAggrDeaggr && CurrentlyAggroed && _player != null) { AssignTargetToFollow(_player.transform); } 
    }

    public void Flee()
    {
        if (IsAfraid && _currentFleeSpot != null)
        {
            Vector2 trposV2 = transform.position;
            Vector2 trposTarV2 = _currentFleeSpot.transform.position;

            if (Vector3.Distance(trposV2, trposTarV2) < 0.08f)
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
                    Deaggro();
                }
            }
        }
    }

    public void Patrol()
    {
        if (!CurrentlyAggroed)
        {
            Vector2 trposV2 = transform.position;
            Vector2 trposTarV2 = _currentPatrolTarget.position;
            if (Vector3.Distance(trposV2, trposTarV2) < 0.08f)
            {
                if (_currentPatrolPoint < PatrolPath.Length - 1)
                {
                    _currentPatrolPoint += 1;
                }
                else
                { _currentPatrolPoint = 0; }
                _currentPatrolTarget = _patrolTransforms[_currentPatrolPoint];
                AssignTargetToFollow(_currentPatrolTarget);
            }
        }
    }

    public void Rush()
    {
        if (_currentRushTarget != null)
        {
            Vector2 trposV2 = transform.position;
            Vector2 trposTarV2 = _currentRushTarget.transform.position;
            _isRushing = true;
            RayCastRush();

            // Deaggro patterns:
            if (!_rushTargetInSight) { Deaggro(); } // if lost sight to rushTarget - be ready to aggro instantly
            if (Vector3.Distance(trposV2,trposTarV2) < 0.08f) { Deaggro(); EnterRushCooldown(); } // if reached rush point - enter cooldown
        }
    }

    public void EnterRushCooldown()
    { _onCooldown = true; }

    // Delegated to animation script
    public void PerformThrow(Vector2Int aPlayerPos)
    {
        GameObject go = Instantiate(Projectile, new Vector3((transform.position.x + aPlayerPos.x), (transform.position.y + aPlayerPos.y), 0f), new Quaternion(), GameObject.Find("Lvl2EnemyHolder").transform);
        ProjectileScript ps = go.GetComponent<ProjectileScript>();
        ps.Slows = Slows;
        ps.Stuns = Stuns;
        ps.Damages = Damages;
        ps.DamagePerHit = DamagesFor;
        ps.SlowFor = SlowFor;
        ps.StunFor = StunFor;

        ps.Direction = aPlayerPos;
        ps.ProjectileSpeed = ProjectileSpeed;
        ps.ProjectileLifetime = ProjectileLifetime;
        ps.ProjectileCollision = ProjectileCollision;
    }

    public void AssessTeleportDistance()
    {
        if (AllowedToTeleport && _player != null)
        {
            if (_remainingDistance > DistanceToTeleport || Vector3.Distance(gameObject.transform.position, _player.transform.position) > DistanceToTeleport + 2) { _consideringTeleport = true; }
            else { _consideringTeleport = false; }
        }
    }

    #endregion BEHAVIOURS

    #region AUXILIARY BEHAVIOURS

    public void RayCastRush()
    {
        Vector2 direction = _playerDir;
        
        RaycastHit2D colliderHit = Physics2D.Raycast(transform.position, _playerDir, Vector3.Distance(this.gameObject.transform.position, _currentRushTarget.position), _rushSightLayers);
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
            if (!Physics2D.OverlapCircle(currentlySearchedCell, 0.2f, RushPointLayers))
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

        AssignTargetToFollow(_currentRushTarget);
    }

    /// <summary>
    /// As of Unity 2019.3, NavMeshAgent.remainingDistance is still calculated only after the penultimate 
    /// corner of the path has been reached, and the agent is traversing the last segment.
    /// Before that, remainingDistance will return infinity. Sadly, this is undocumented.
    /// https://stackoverflow.com/questions/61421172/why-does-navmeshagent-remainingdistance-return-values-of-infinity-and-then-a-flo
    /// </summary>
    public void GetRemainingDistance()
    {
        if (CurrentlyAggroed == true)
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
        AssignTargetToFollow(_currentFleeSpot.transform);
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

    private void ReactToPlayerLevelUp(int aPlayerLevel)
    { if (aPlayerLevel >= EnemyLevel) _isAfraid = true; }

    private void ReactToPlayerDeath()
    {
        _isRushing = false;
        CurrentTarget = _patrolTransforms[_currentPatrolPoint];
    }

    public void Die(bool noCorpse) 
    {
        if (_diedOnce == false)
        {
            // destroy all elements
            if (_currentFleeSpot != null) Destroy(_currentFleeSpot);
            if (_currentRushTarget != null) Destroy(_currentRushTarget.gameObject);
            if (_currentTPVoidZone != null) Destroy(_currentTPVoidZone);

            List<SoundBiteInstance> sbis = new List<SoundBiteInstance>();
            foreach (Transform chtr in transform)
            { if (chtr.GetComponent<SoundBiteInstance>() != null) chtr.GetComponent<SoundBiteInstance>().StopImmediately(); }

            Destroy(this.gameObject);


            // instantiate elements
            if (DeathObject != null && noCorpse == false) { Instantiate(DeathObject, transform.position, Quaternion.identity, GameObject.Find("EnemyCorpseHolder").transform); }
            else if (DeathObjectNoCorpse != null && noCorpse == true) { Instantiate(DeathObjectNoCorpse, transform.position, Quaternion.identity, GameObject.Find("EnemyCorpseHolder").transform); }

            // invoke events
            OnDie?.Invoke(ItemStageLevel, this.gameObject);
            _diedOnce = true;
        }
    }

    public void Stun(float aLength)
    { 
        if (!Stunned) 
        {
            ResetStunTimer(aLength);

            Stunned = true;
            _statusEffect.SetStunned();
            gameObject.GetComponent<EnemySoundScript>().PlayDamagedSound();
        } 
    }

    // Enemies that are already stunned won't slow not to mess up speed calc
    public void Slow(float aLength)
    {
        if (!Slowed) {
            if (Stunned) return;
            else 
            {
                ResetSlowTimer(aLength);
                Slowed = true;
                _statusEffect.SetSlowed();
                if (gameObject.GetComponent<EnemySoundScript>() != null) gameObject.GetComponent<EnemySoundScript>().PlayDamagedSound();
            } 
        }
    }

    public void ChangeNavMeshMasks(bool aAggroed, bool aAfraid)
    {
        if (aAggroed)
        {
            if (aAfraid) { _agent.areaMask = _areaDict["Walkable"]; }
            else
            {
                if (EnemyLevel == 2 && EnemyType == EnemyOfType.Roamer)
                { _agent.areaMask = _areaDict["Walkable"] + _areaDict["WalkableWhenAngry"]; }
                else if (EnemyLevel == 3 || (EnemyType == EnemyOfType.Rusher))
                { _agent.areaMask = _areaDict["Walkable"] + _areaDict["WalkableWhenAngry"] + _areaDict["OnlyRusherAndLvl3"]; }
            }
        }
        else { _agent.areaMask = _areaDict["Walkable"];  }
    }

    public void TeleportToDestination(Vector3 aDestination)
    {
        CurrentlyTeleporting = true;
        gameObject.GetComponent<EnemyAnimation_Boss>().ExecuteAnimationSpawnOut();
        SpawnOutDestination = aDestination;

        if (_currentTPVoidZone != null) Destroy(_currentTPVoidZone.gameObject);
        _currentTPVoidZone = Instantiate(TeleportVoidZone, SpawnOutDestination, new Quaternion(), null);
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
            float distanceBeforeCalc = 0.0f;
            float NextStretch = 0.0f;
            float totalDistanceSoFar = 0.0f;

            Vector3 TempDestination = transform.position;

            // start calculating distance from end of path (player), till start of path (boss)
            // if distance exceeds value TeleportToPointPastDistanceOf: 
            // go through as many loops as needed to find midpoint between current point and next point that would be close to desired distance

            for (int i = _agent.path.corners.Length - 1; i > 0; --i)
            {
                NextStretch = Vector3.Distance(_agent.path.corners[i], _agent.path.corners[i - 1]);
                distanceBeforeCalc = totalDistanceSoFar;
                totalDistanceSoFar += NextStretch;

                if (totalDistanceSoFar <= TeleportToPointPastDistanceOf) continue;

                else
                {
                    // Debug.LogWarning("With corner " + (i-1) + " distance " + totalDistanceSoFar + " will exceed desired: " + TeleportToPointPastDistanceOf);
                    if (totalDistanceSoFar <= (TeleportToPointPastDistanceOf + TeleportToPointPastDistanceOfUpper))
                    {
                        // Debug.LogWarning("Point " + TempDestination + " is within range of distance and distance upper. Teleporting");
                        TempDestination = _agent.path.corners[i - 1];
                        SpawnOutDestination = new Vector3(TempDestination.x, TempDestination.y, 0);
                        break;
                    }
                    else
                    {
                        //Debug.LogWarning("Point exceeds range of distance and distance upper. Further actions needed");

                        float distanceTemp = totalDistanceSoFar;
                        int AttemptNum = 0;
                        TempDestination = _agent.path.corners[i - 1];
                        Vector3 BetterTeleportPosition = Vector3.zero;

                        while (distanceTemp > (TeleportToPointPastDistanceOf + TeleportToPointPastDistanceOfUpper) && AttemptNum < 25)
                        {
                            // take V3 of currently examined vector and the next considered corner (closer to boss) and find midpoint skewed towards next point
                            BetterTeleportPosition = Vector3.Lerp(_agent.path.corners[i], TempDestination, 0.9f);
                            distanceTemp = distanceBeforeCalc + Vector3.Distance(_agent.path.corners[i], TempDestination);

                            if (distanceTemp <= (TeleportToPointPastDistanceOf + TeleportToPointPastDistanceOfUpper))
                            { /* Debug.LogWarning("Success. " + BetterTeleportPosition + " will be a new point after " + AttemptNum + " attempts. Teleporting"); */ }

                            TempDestination = BetterTeleportPosition;
                            AttemptNum++;
                        }
                        SpawnOutDestination = new Vector3(TempDestination.x, TempDestination.y, 0);
                        if (TeleportVoidZone != null)
                        {
                            if (_currentTPVoidZone != null) Destroy(_currentTPVoidZone.gameObject);
                            _currentTPVoidZone = Instantiate(TeleportVoidZone, SpawnOutDestination, new Quaternion(), null);
                        }
                    }
                    CurrentlyTeleporting = true;
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                    gameObject.GetComponent<EnemyAnimation_Boss>().ExecuteAnimationSpawnOut();
                    break;
                }
            }
        }
    }


    #endregion ON CALL BEHAVIOURS

    #region TIMER DECREMENTS

    public void MinimalAggroTimerDecrement()
    {
        if (HasMinimalAggroTime && CurrentlyAggroed == true && IsAfraid == false)
            if (_currentMinimalAggroTimer >= 0) { _currentMinimalAggroTimer -= Time.deltaTime; }
    }

    public void StunTimerDecrement()
    { 
        if (Stunned) 
        {
            if (_currentStunTimer >= 0) { _currentStunTimer -= Time.deltaTime; }
            else 
            { 
                Stunned = false;
                if (Slowed == true) { _statusEffect.SetSlowed(); }
                else _statusEffect.RemoveStatusEffect();
            }
        }
    }

    public void SlowTimerDecrement()
    {
        if (Slowed)
        {
            if (_currentSlowTimer >= 0) { _currentSlowTimer -= Time.deltaTime; }
            else
            { 
                Slowed = false;
                _statusEffect.RemoveStatusEffect();
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
        if (AllowedToTeleport && ConsideringTeleport)
        {
            if (_currentTeleportCountDown >= 0) { _currentTeleportCountDown -= Time.deltaTime; }
            else if (_currentTeleportCountDown < 0 && CurrentlyTeleporting != true)
            { PerformBossTeleport(); }
        }
    }

    #endregion TIMER DECREMENTS

    #region TIMER RESETS
    public void ResetStunTimer(float aLength) { _currentStunTimer = aLength; }
    public void ResetSlowTimer(float aLength) { _currentSlowTimer = aLength; }

    public void ResetMinimalAggroCooldown()
    {
        if (EnemyType == EnemyOfType.Roamer)
        { _currentMinimalAggroTimer = DefaultMinimalAggroTimer; }
    }

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

    #endregion TIMER RESETS
}
