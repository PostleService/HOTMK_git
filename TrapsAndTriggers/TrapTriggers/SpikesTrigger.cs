using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesTrigger : TrapTrigger
{
    public GameObject PostTrap;

    [Tooltip("if not single use, make sure destruction is not infinity")]
    public bool SingleUse = true;
    
    [Tooltip("How soon is the trap ready to be used again if not single use")]
    public float Cooldown = 10f;
    private float _cooldownTimerCurrent;

    public float SelfActivationDefault = 5f;
    private float _selfActivationCurrent;
    private bool _selfActivationComplete = false;

    private LevelManagerScript _levelManager;
    private Animator _animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        _activationTimerCurrent = ActivationTimer;
        _destroyTimerCurrent = DestroyAfter;

        _animator = this.gameObject.GetComponent<Animator>();
        _cooldownTimerCurrent = Cooldown;
        _selfActivationCurrent = SelfActivationDefault;

        if (ConcealTrapTrigger) { this.gameObject.GetComponent<SpriteRenderer>().sprite = null; }
    }

    protected override void FixedUpdate()
    {
        SelfActivationTimer();
        SelfActivate();

        ActivationCountdown();
        DestroyTimer();
        CooldownTimer(); 
    }

    // UPDATE FUNCTIONS

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

    private void SelfActivationTimer()
    {
        if (ReactTo == ReactToCharacters.SelfActivated && _selfActivationCurrent >= 0 && _selfActivationComplete == false)
        { _selfActivationCurrent -= Time.fixedDeltaTime; }
        else if (ReactTo == ReactToCharacters.SelfActivated && _selfActivationCurrent < 0 && _selfActivationComplete == false)
        { _selfActivationComplete = true; }
    }

    private void SelfActivate()
    {
        if (_selfActivationComplete == true && _hasBeenTriggered == false)
        { ReactToTrigger(); }
    }

    // ON CALL FUNCTIONS

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        if (!_hasBeenTriggered)
        {
            if (ReactTo == ReactToCharacters.Player) { if (collision.tag == "Player") ReactToTrigger(); }
            else if (ReactTo == ReactToCharacters.PlayerAndEnemy) { if (collision.tag == "Player" || collision.tag == "Enemy") ReactToTrigger(); }
            else if (ReactTo == ReactToCharacters.Enemy) { if (collision.tag == "Enemy") ReactToTrigger(); }
        }
    }

    private void ReactToTrigger()
    {
        _hasBeenTriggered = true;
        _animator.Play("SpikesPressedAnim");
        gameObject.GetComponent<SpikesSoundScript>().PlaySpikesDepressedSound();
    }

    protected override void SpawnTrap()
    {
        if (!_hasBeenSpawned)
        {
            Vector3 pos = transform.position;

            if (Trap != null)
            { _trap = Instantiate(Trap, pos, new Quaternion(), this.gameObject.transform); _animator.Play("SpikesRisingAnim"); }
            _hasBeenSpawned = true;
        }
    }

    protected override void DeleteTrap()
    {
        if (_trap != null) Destroy(_trap);
        if (PostTrap != null) Instantiate(PostTrap, transform.position, new Quaternion(), this.gameObject.transform);
        
        _animator.Play("SpikesFallingAnim");

        _hasBeenSpawned = false; _allowedToCooldown = true;
    }

}
