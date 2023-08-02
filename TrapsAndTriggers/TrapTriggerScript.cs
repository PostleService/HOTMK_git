using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTriggerScript : MonoBehaviour
{
    public enum ReactToCharacters
    {
        Player,
        PlayerAndEnemy,
        Enemy
    }
    public ReactToCharacters ReactTo;

    public enum TypesOfTraps 
    {
        NonCollapsable,
        Collapsable,

    }
    public TypesOfTraps TypeOfTrap;

    [Tooltip("GameObject with animation")]
    public GameObject Trap;
    public Vector3 SpawnOffset;
    [Tooltip("How soon after triggering does the trap start damaging")]
    public float ActivationTimer = 3f;
    private float _activationTimerCurrent;
    [Tooltip("How soon after triggering does the animation and collider despawn. -1 for never")]
    public float DestroyAfter;
    public bool DestroySelfAfterSpawn = false;
    private float _destroyTimerCurrent;
    [Tooltip("if not single use, make sure destruction is not infinity")]
    public bool SingleUse = true;
    [Tooltip("How soon is the trap ready to be used again if not single use")]
    public float Cooldown = 10f;
    private float _cooldownTimerCurrent;

    [Tooltip("sets Sprite of the trigger to None after startup")]
    public bool ConcealTrapTrigger = true;

    private GameObject _trap;
    [HideInInspector] public bool _hasBeenTriggered = false;
    [HideInInspector] public bool _hasBeenSpawned = false;
    private bool _allowedToCooldown = false;
    private LevelManagerScript _levelManager;

    [Header("Pass to trap")]
    public bool TeleportBossToCustom = false;
    public Vector3 CustomTeleportDestination = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        _activationTimerCurrent = ActivationTimer;
        _destroyTimerCurrent = DestroyAfter;
        _cooldownTimerCurrent = Cooldown;

        if (ConcealTrapTrigger) { this.gameObject.GetComponent<SpriteRenderer>().sprite = null; }
    }

    void FixedUpdate() 
    {
        ActivationCountdown();
        DestroyTimer();
        CooldownTimer();
    }

    // UPDATE FUNCTIONS

    private void ActivationCountdown()
    {
        if (_hasBeenTriggered)
        {
            if (_activationTimerCurrent >= 0)
            { _activationTimerCurrent -= Time.deltaTime; }
            else if (_activationTimerCurrent < 0)
            {
                SpawnTrap();
            }
        }
    }
    
    private void CooldownTimer()
    {
        if (!SingleUse)
        {
            if (_cooldownTimerCurrent >= 0 && _allowedToCooldown)
            { _cooldownTimerCurrent -= Time.deltaTime; }
            else if (_cooldownTimerCurrent < 0)
            {
                _activationTimerCurrent = ActivationTimer;
                _cooldownTimerCurrent = Cooldown;
                _destroyTimerCurrent = DestroyAfter;
                _hasBeenTriggered = false;
                _allowedToCooldown = false;
            }
        }
    }

    private void DestroyTimer()
    {
        if (DestroyAfter != -1 && _hasBeenSpawned)
        {
            if (_destroyTimerCurrent >= 0)
            { _destroyTimerCurrent -= Time.deltaTime; }
            else if (_destroyTimerCurrent < 0)
            { DeleteTrap(); }
        }
    }

    // ON CALL FUNCTIONS

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_hasBeenTriggered)
        {
            if (ReactTo == ReactToCharacters.Player)
            { if (collision.tag == "Player") { _hasBeenTriggered = true; } }
            else if (ReactTo == ReactToCharacters.PlayerAndEnemy)
            { if (collision.tag == "Player" || collision.tag == "Enemy") { _hasBeenTriggered = true; } }
            else if (ReactTo == ReactToCharacters.Enemy)
            { if (collision.tag == "Enemy") { _hasBeenTriggered = true; } }
        }
    }

    private void SpawnTrap()
    {
        if (!_hasBeenSpawned)
        {
            // for proper layering, pivot on collapsables set lower than needed. Correcting offset through lower y pos
            Vector3 pos = Vector3.zero;

            if (TypeOfTrap == TypesOfTraps.Collapsable)
            {
                _levelManager = GameObject.Find("LevelManager").GetComponent<LevelManagerScript>();
                // Remove the tile from a list of AllWalkableTiles in the LevelManager
                // No navmesh recalculation required - using Nav Mesh Obstacle to carve from calculated navmesh
                _levelManager.AllWalkableTiles.Remove(this.gameObject.transform.position);

                pos = new Vector3(transform.position.x + SpawnOffset.x, transform.position.y + SpawnOffset.y, 0);
            }
            else pos = transform.position;

            if (Trap != null)
            {  
                _trap = Instantiate(Trap, pos, new Quaternion(), this.gameObject.transform);

                AnimationEndDetection_CollapsableCeiling aed1 = _trap.GetComponent<AnimationEndDetection_CollapsableCeiling>();
                if (aed1 != null)
                {
                    aed1.TeleportBossToCustom = TeleportBossToCustom;
                    aed1.CustomTeleportDestination = CustomTeleportDestination;
                }
                AnimationEndDetection_LaserTrap aed2 = _trap.GetComponent<AnimationEndDetection_LaserTrap>();
                if (aed2 != null)
                {
                    aed2.TeleportBossToCustom = TeleportBossToCustom;
                    aed2.CustomTeleportDestination = CustomTeleportDestination;
                }
                    
            }

            _hasBeenSpawned = true;
            if (DestroySelfAfterSpawn == true)
            {
                foreach (Transform chTr in transform)
                { chTr.parent = GameObject.Find("TrapsAndTriggers").transform; }
                Destroy(gameObject);
            }
        }
    }

    private void DeleteTrap()
    { if (_trap != null) 
        { Destroy(_trap); _hasBeenSpawned = false; _allowedToCooldown = true;  } 
    }
}
