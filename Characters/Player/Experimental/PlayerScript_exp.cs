using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FMODUnity;

public class PlayerScript_exp : MonoBehaviour
{
    [HideInInspector]
    public float SpeedAfterCalc;
    [Header("Player Movement")]
    [Tooltip("Default speed of movement")]
    public float MovementSpeedDefault;
    [Tooltip("By what value the movement speed should be multiplied during iframes")]
    public float DamageAcceleration;
    private float _currentDamageAcceleration = 1f;
    public bool Slowed = false;
    public bool Stunned = false;
    [HideInInspector] public bool _justLeftStun = false;
    public float SlowedSpeedModifier = 0.5f;
    private float _currentSlowedSpeedModifier = 1f;
    public float StunSpeedModifier = 0f;
    private float _currentStunSpeedModifier = 1f;
    private float _currentStunTimer;
    private float _currentSlowTimer;
    private float _currentPostSpawnCannotMove = 0.1f;

    [Header("Player Stats")]
    public bool AllowLevelUp = true;
    public GameObject LevelUpObject;
    public int PlayerLevel = 0;
    public int MaxPlayerLives = 3;
    public int CurrentLives;
    public GameObject PlayerDeathObject;
    public GameObject PlayerDeathObjectNoCorpse;
    public float CountdownIFramesDefault = 5.0f;
    public float _countdownIFramesCurrent;

    [Tooltip("When iframes are in effect, flash opacity till bottom value of")]
    public float FlashPlayerUponDamage = 0.25f;
    public float SpeedOfFlash = 1f;
    private bool _flashBottomReached = false;
    private bool _isDamaged = false;

    [Header("Vision")]
    public int RememberFogAtLevelStage = 2;
    public int CanSeeThroughWallsAtStage = 3;
    public float RangeOfVision = 10f;
    [Tooltip("Offset from main raycast when looking for enemy")]
    public float DegreeOfSpread = 2f;

    private Animator _animator;
    private StatusEffectScript _statusEffect;

    public delegate void MyHandler (int aCurrentHealth, int aMaxHealth, string aUpdateState);
    public static event MyHandler OnHealthUpdate;
    public delegate void LevelUpTracker (int aPlayerLevel);
    public static event LevelUpTracker OnLevelUp;
    public delegate void SpawnDelegate (GameObject aGameObject);
    public static event SpawnDelegate OnSpawn;
    public delegate void PositionTracker (GameObject aGameObject, Vector2 aPosition);
    public static event PositionTracker OnPositionChange;
    public static event Action OnRememberFog;
    public static event Action OnEnemiesDeconceal;

    private void OnEnable()
    { 
        LevelManagerScript.OnLevelStageChange += LevelUp;
    }

    private void OnDisable()
    { 
        LevelManagerScript.OnLevelStageChange -= LevelUp;
    }

    private void Awake()
    {
        ResetIFramesTimer();
    }

    private void Start()
    {
        _statusEffect = GetComponentInChildren<StatusEffectScript>();

        OnSpawn?.Invoke(this.gameObject); // tell all related managers that player has spawned
        OnPositionChange?.Invoke(this.gameObject, transform.position);

        CurrentLives = MaxPlayerLives;
        OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartSpawn");

        GameObject.Find("PlayerCamera").transform.SetParent(gameObject.transform);
        _animator = gameObject.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (_currentPostSpawnCannotMove >= 0) { _currentPostSpawnCannotMove -= Time.fixedDeltaTime; }

        StunTimerDecrement();
        SlowTimerDecrement();
        CountDownIFrames();

        if (Stunned) { _currentStunSpeedModifier = StunSpeedModifier; }
        else _currentStunSpeedModifier = 1f;

        if (Slowed) { _currentSlowedSpeedModifier = SlowedSpeedModifier; }
        else _currentSlowedSpeedModifier = 1f;

        if (_isDamaged) { _currentDamageAcceleration = DamageAcceleration; }
        else _currentDamageAcceleration = 1f;

        OnPositionChange?.Invoke(this.gameObject, transform.position);
    }

    private void UpdateAnimatorHandlerValues(float aPlayerSpeed, float aHorizontalDirection = 0, float aVerticalDirection = 0)
    {
        _animator.SetFloat("Horizontal", aHorizontalDirection);
        _animator.SetFloat("Vertical", aVerticalDirection);
        _animator.SetFloat("Speed", aPlayerSpeed);
        _animator.speed = _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier; // update animation speed based on status effects
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            if (col.gameObject.GetComponent<EnemyScript>() != null)
            {
                EnemyScript enemyScript = col.gameObject.GetComponent<EnemyScript>();
                if (PlayerLevel < enemyScript.EnemyLevel)
                {
                    if (!_isDamaged)
                    {
                        _isDamaged = true;
                        TakeDamage(enemyScript.DamagePerHit, false);
                    }
                }
                else if (PlayerLevel >= enemyScript.EnemyLevel)
                { enemyScript.Die(false); }
            }
        }
        else if (col.tag == "Trap")
        {
            TrapScript enemyScript = col.gameObject.GetComponent<TrapScript>();
            if (!_isDamaged)
            {
                if (enemyScript.DamagePerHit > 0) _isDamaged = true;
                
                if (!enemyScript.NoCorpse) TakeDamage(enemyScript.DamagePerHit, false);
                else TakeDamage(enemyScript.DamagePerHit, true);
            }
        }
        else if (col.tag == "Projectile")
        { 
            ProjectileScript projScript = col.gameObject.GetComponent<ProjectileScript>();
            if (!_isDamaged)
            {
                if (projScript.DamagePerHit > 0 && projScript.Damages[1] == true) _isDamaged = true;
                TakeDamage(projScript.DamagePerHit, false);
            }
        }
    }

    public void Slow(float aLength)
    { 
        if (!Slowed) 
        { 
            Slowed = true;
            _statusEffect.SetSlowed();
            ResetSlowTimer(aLength);
        } 
    }

    public void Stun(float aLength)
    { 
        if (!Stunned) 
        { 
            Stunned = true;
            _statusEffect.SetStunned();
            ResetStunTimer(aLength); 
        } 
    }

    public void SyncronizeLevelUps(int aLevelStage)
    {
        // in case player spawns lower level than the stage for any reason
        while (aLevelStage < 3 && PlayerLevel < aLevelStage)
        {
            // go through each lvl one by one
            int NewLevel = PlayerLevel + 1;
            LevelUp(NewLevel, 0,0, null);
        }
    }

    public void LevelUp(int aLevelStage, int aCurrentItems, int aDefaultItems, Sprite aSprite)
    {
        if (AllowLevelUp && aLevelStage < 3 && aLevelStage > PlayerLevel)
        {
            PlayerLevel = aLevelStage;
            if (LevelUpObject != null) Instantiate(LevelUpObject, transform.position, new Quaternion(), gameObject.transform);
            if (PlayerLevel == RememberFogAtLevelStage) { RememberFog(); }
            if (PlayerLevel == CanSeeThroughWallsAtStage) { DeconcealEnemies(); }
            OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartLevelUp");
            OnLevelUp?.Invoke(PlayerLevel);
        }
    }

    public void RememberFog() { OnRememberFog?.Invoke(); }
    public void DeconcealEnemies() { OnEnemiesDeconceal?.Invoke(); }

    // perhaps later a call to Animator from within this function, etc.
    public void TakeDamage(int aDamage, bool aNoCorpse) 
    {
        Debug.LogWarning("Lives before damage : " + CurrentLives + " " + "Damage received : " + aDamage);
        CurrentLives = CurrentLives - aDamage;
        Debug.LogWarning("Lives after damage : " + CurrentLives);

        if (CurrentLives <= 0) { Die(aNoCorpse); }
        
        else 
        {
            if (aDamage < 0) { OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartHeal"); }
            else if (aDamage == 0) { return; }
            else 
            {
                OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartDamage");
                gameObject.GetComponent<PlayerSoundScript>().PlayDamageSound();
            }
        }
    }


    private void Die(bool aNoCorpse) 
    {
        foreach (Transform childtr in this.gameObject.transform)
        { if (childtr.GetComponent<Camera>() != null) { childtr.parent = null; } }
        OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartDeath");
        Destroy(this.gameObject);

        if (PlayerDeathObject != null && PlayerDeathObjectNoCorpse != null)
        {
            GameObject toInstantiate;
            if (!aNoCorpse) toInstantiate = PlayerDeathObject;
            else toInstantiate = PlayerDeathObjectNoCorpse;

            GameObject go = Instantiate(toInstantiate, transform.position, new Quaternion());
            go.name = PlayerDeathObject.name; 
            go.GetComponent<SpriteSheetSwapper_PlayerDeath>()._playerLevel = PlayerLevel;
        }
    }

    private void CountDownIFrames()
    {
        if (_isDamaged)
        {
            FlashPlayerOnDamage();
            if (_countdownIFramesCurrent >= 0)
            {
                _countdownIFramesCurrent -= Time.deltaTime;
            }
            else if (_countdownIFramesCurrent < 0)
            {
                _isDamaged = false;
                _flashBottomReached = false;
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                ResetIFramesTimer();
            }
        }
    }

    private void FlashPlayerOnDamage()
    {
        if (_isDamaged)
        {
            if (!_flashBottomReached)
            {
                float OpacityValue = gameObject.GetComponent<SpriteRenderer>().color.a;
                float newOpacityValue = OpacityValue - (Time.deltaTime * SpeedOfFlash);
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, newOpacityValue);

                if (gameObject.GetComponent<SpriteRenderer>().color.a <= FlashPlayerUponDamage)
                { _flashBottomReached = true; }
            }
            if (_flashBottomReached)
            {
                float OpacityValue = gameObject.GetComponent<SpriteRenderer>().color.a;
                float newOpacityValue = OpacityValue + (Time.deltaTime * SpeedOfFlash);
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, newOpacityValue);

                if (gameObject.GetComponent<SpriteRenderer>().color.a >= 1f)
                { _flashBottomReached = false; }
            }
        }
    }

    private void ResetIFramesTimer()
    { _countdownIFramesCurrent = CountdownIFramesDefault; }

    public void ResetStunTimer(float aLength) { _currentStunTimer = aLength; }
    public void ResetSlowTimer(float aLength) { _currentSlowTimer = aLength; }

    public void StunTimerDecrement()
    {
        if (Stunned)
        {
            if (_currentStunTimer >= 0) { _currentStunTimer -= Time.deltaTime; }
            else 
            { 
                Stunned = false;
                if (Slowed == true)
                { _statusEffect.SetSlowed(); }
                else _statusEffect.RemoveStatusEffect();
                
                // xInput = 0; yInput = 0; _lastMovementDirection = Vector2.zero; 
                
                _justLeftStun = true; 
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

}