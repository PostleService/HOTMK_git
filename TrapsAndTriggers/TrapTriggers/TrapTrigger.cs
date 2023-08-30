using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    public enum ReactToCharacters
    {
        Player,
        PlayerAndEnemy,
        Enemy, 
        SelfActivated
    }
    public ReactToCharacters ReactTo;

    [Tooltip("GameObject with animation")]
    public GameObject Trap;
    [Tooltip("How soon after triggering does the trap start damaging")]
    public float ActivationTimer = 3f;
    protected float _activationTimerCurrent;
    [Tooltip("How soon after triggering does the animation and collider despawn. -1 for never")]
    public float DestroyAfter;
    protected float _destroyTimerCurrent;

    [Tooltip("Sets Sprite of the trigger to None after startup")]
    public bool ConcealTrapTrigger = true;

    protected GameObject _trap;
    [HideInInspector] public bool _hasBeenTriggered = false;
    protected bool _hasBeenSpawned = false;
    protected bool _allowedToCooldown = false;

    protected virtual void Start()
    {
        _activationTimerCurrent = ActivationTimer;
        _destroyTimerCurrent = DestroyAfter;

        if (ConcealTrapTrigger) { this.gameObject.GetComponent<SpriteRenderer>().sprite = null; }
    }

    protected virtual void FixedUpdate() 
    {
        ActivationCountdown();
        DestroyTimer();
    }

    // UPDATE FUNCTIONS

    protected void ActivationCountdown()
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

    protected void DestroyTimer()
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

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (!_hasBeenTriggered)
        {
            if (ReactTo == ReactToCharacters.Player && collision.tag == "Player") _hasBeenTriggered = true;
            else if (ReactTo == ReactToCharacters.PlayerAndEnemy && (collision.tag == "Player" || collision.tag == "Enemy")) _hasBeenTriggered = true;
            else if (ReactTo == ReactToCharacters.Enemy && collision.tag == "Enemy") _hasBeenTriggered = true;
        }
    }

    protected virtual void SpawnTrap() { }

    protected virtual void DeleteTrap()
    { 
        if (_trap != null) 
        { Destroy(_trap); _hasBeenSpawned = false; } 
    }
}
