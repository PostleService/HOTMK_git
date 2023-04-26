using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesTriggerScript : MonoBehaviour
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
    public GameObject PostTrap;
    [Tooltip("How soon after triggering does the trap start damaging")]
    public float ActivationTimer = 3f;
    private float _activationTimerCurrent;
    [Tooltip("How soon after triggering does the animation and collider despawn. -1 for never")]
    public float DestroyAfter;
    private float _destroyTimerCurrent;
    [Tooltip("if not single use, make sure destruction is not infinity")]
    public bool SingleUse = true;
    [Tooltip("How soon is the trap ready to be used again if not single use")]
    public float Cooldown = 10f;
    private float _cooldownTimerCurrent;

    [Tooltip("sets Sprite of the trigger to None after startup")]
    public bool ConcealTrapTrigger = true;

    private GameObject _trap;
    private bool _hasBeenTriggered = false;
    private bool _hasBeenSpawned = false;
    private bool _allowedToCooldown = false;
    private bool _trapDestroyed = false;
    private LevelManagerScript _levelManager;

    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = this.gameObject.GetComponent<Animator>();
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
        if (_hasBeenTriggered && !_allowedToCooldown)
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
                _trapDestroyed = false;
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
            { if (!_trapDestroyed) DeleteTrap(); }
        }
    }

    // ON CALL FUNCTIONS

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_hasBeenTriggered)
        {
            
            if (ReactTo == ReactToCharacters.Player)
            { if (collision.tag == "Player") { _hasBeenTriggered = true; _animator.Play("SpikesPressedAnim"); } }
            else if (ReactTo == ReactToCharacters.PlayerAndEnemy)
            { if (collision.tag == "Player" || collision.tag == "Enemy") { _hasBeenTriggered = true; _animator.Play("SpikesPressedAnim"); } }
            else if (ReactTo == ReactToCharacters.Enemy)
            { if (collision.tag == "Enemy") { _hasBeenTriggered = true; _animator.Play("SpikesPressedAnim"); } }
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

                pos = new Vector3(transform.position.x, transform.position.y - 0.125f, 0);
            }
            else pos = transform.position;

            if (Trap != null)
            { _trap = Instantiate(Trap, pos, new Quaternion(), this.gameObject.transform); _animator.Play("SpikesRisingAnim"); }
            _hasBeenSpawned = true;
        }
    }

    private void DeleteTrap()
    {
        Destroy(_trap); _hasBeenSpawned = false; _allowedToCooldown = true;
        _animator.Play("SpikesFallingAnim");
        if (PostTrap) Instantiate(PostTrap, transform.position, new Quaternion(), this.gameObject.transform);
        _trapDestroyed = true;
    }
}
