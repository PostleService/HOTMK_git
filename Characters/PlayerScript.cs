using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
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
    private bool _justLeftStun = false;
    public float SlowedSpeedModifier = 0.5f;
    private float _currentSlowedSpeedModifier = 1f;
    public float StunSpeedModifier = 0f;
    private float _currentStunSpeedModifier = 1f;
    public float StunTimer = 3f;
    private float _currentStunTimer;
    public float SlowTimer = 3f;
    private float _currentSlowTimer;
    private float _currentPostSpawnCannotMove = 0.1f;

    [Tooltip("How far in units does the player have to be from distance point to accept new movemnt input")]
    [HideInInspector]
    public float MovementSwitchFlipDistance = 0f;

    [Header("Collision")]
    [Tooltip("Layers with which object will collide")]
    public LayerMask Layers;
    [Tooltip("Radius in which to search for collider in the next tile ")]
    public float ColliderSearchRadius = 0.2f;

    // This is the actual object controlled by keyboard input. The parent object will attempt to move towards it.
    private Transform _movementPoint;
    [HideInInspector]
    public int xInput;
    [HideInInspector]
    public int yInput;
    [HideInInspector]
    public bool movementInitiated = false;
    private Vector2 _lastMovementInput;
    private Vector2 _lastMovementDirection;
    private bool _resolvingConflictX = false; 
    private bool _resolvingConflictY = false;

    [Tooltip("This is the downtime between movement from tile to tile. Necessary, as otherwise animation has no time to update facing direction. Also, introduces a break before moving from tile to tile.")]
    public float CountdownMovementDefault = 0.01f;
    private float _countdownMovementCurrent;

    [Header("Player Stats")]
    public bool AllowLevelUp = true;
    public int PlayerLevel = 0;
    public int MaxPlayerLives = 3;
    public int CurrentLives;
    public GameObject PlayerDeathObject;
    public float CountdownIFramesDefault = 5.0f;
    public float _countdownIFramesCurrent;
    [SerializeField]
    [Tooltip("When iframes are in effect, flash opacity till bottom value of")]
    public float FlashPlayerUponDamage = 0.25f;
    public float SpeedOfFlash = 1f;
    private bool _flashBottomReached = false;
    private bool _isDamaged = false;
    private UIManager _canvasManager;

    [Header("Vision")]
    public int RememberFogAtLevelStage = 2;
    public bool CanSeeThroughWalls = false;
    public int CanSeeThroughWallsAtStage = 3;
    public float RangeOfVision = 10f;
    [Tooltip("Offset from main raycast when looking for enemy")]
    public float DegreeOfSpread = 2f;

    // ANIMATION
    private Animator _animator;

    private void Awake()
    {
        ResetMovementTimer();
        ResetIFramesTimer();
    }

    private void Start()
    {
        _animator = gameObject.GetComponent<Animator>();

        OnSpawn?.Invoke(); // tell all related managers that player has spawned
        GameObject.Find("PlayerCamera").transform.SetParent(gameObject.transform);

        CurrentLives = MaxPlayerLives;
        _canvasManager = GameObject.Find("CanvasManager").GetComponent<UIManager>();
        _canvasManager.DrawHearts("start");

        ResetStunTimer();
        ResetSlowTimer();
        FindMovementPoint();
    }

    public static event Action OnSpawn;

    private void FixedUpdate()
    {
        if (_currentPostSpawnCannotMove >= 0) { _currentPostSpawnCannotMove -= Time.fixedDeltaTime; }

        CheckMovementInput();
        PeformMove();

        StunTimerDecrement();
        SlowTimerDecrement();
        CountDownIFrames();

        if (Stunned) { _currentStunSpeedModifier = StunSpeedModifier; }
        else _currentStunSpeedModifier = 1f;

        if (Slowed) { _currentSlowedSpeedModifier = SlowedSpeedModifier; }
        else _currentSlowedSpeedModifier = 1f;

        if (_isDamaged) { _currentDamageAcceleration = DamageAcceleration; }
        else _currentDamageAcceleration = 1f;
    }

    private void OnEnable()
    { LevelManagerScript.OnEnemiesDeconceal += OnDeconceal; }

    private void OnDisable()
    { LevelManagerScript.OnEnemiesDeconceal -= OnDeconceal; }

    private void OnDeconceal()
    { CanSeeThroughWalls = true; }

    void FindMovementPoint()
    {
        foreach (Transform childtransf in this.gameObject.transform)
        {
            if (childtransf.name == "MovementPoint")
            {
                _movementPoint = childtransf;
                //change name of movement point to contain parent name so that when it is in holder to avoid clutter in scene - it can still be located
                _movementPoint.name += _movementPoint.parent.name;
                //set movement point parent to something else so that its transform.position does not depend on parent
                _movementPoint.parent = GameObject.Find("PatrolPathPointHolder").transform;
            }
        }
    }

    private void CheckMovementInput()
    {
        xInput = (int)(Input.GetAxisRaw("Horizontal"));
        yInput = (int)(Input.GetAxisRaw("Vertical"));

        void UpdateAnimatorHandlerValues(float aPlayerSpeed, float aHorizontalDirection = 0, float aVerticalDirection = 0)
        {
            _animator.SetFloat("Horizontal", aHorizontalDirection);
            _animator.SetFloat("Vertical", aVerticalDirection);
            _animator.SetFloat("Speed", aPlayerSpeed);
            _animator.speed = _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier; // update animation speed based on status effects
        }

        if (Vector3.Distance(transform.position, _movementPoint.transform.position) <= MovementSwitchFlipDistance)
        {
            // speed equalized to zero only after arriving to the movement point. locks animations until action completed
            movementInitiated = false;
            MoveDowntime();
            SpeedAfterCalc = 0;
            _animator.SetFloat("Speed", SpeedAfterCalc);
            _animator.speed = _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier;

            bool xInputChanged = false; bool yInputChanged = false;

            bool directionPressed = false;
            if (
                    (Input.GetKey(KeyCode.UpArrow) || (Input.GetKey(KeyCode.W))) ||
                    (Input.GetKey(KeyCode.DownArrow) || (Input.GetKey(KeyCode.S)))
                    ||
                    (Input.GetKey(KeyCode.LeftArrow) || (Input.GetKey(KeyCode.A))) ||
                    (Input.GetKey(KeyCode.RightArrow) || (Input.GetKey(KeyCode.D)))
               )
            { directionPressed = true; }

            // overall check for input change
            if (directionPressed && _countdownMovementCurrent <= 0 && _currentPostSpawnCannotMove <= 0)
            {
                SpeedAfterCalc = MovementSpeedDefault * _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier;

                // resolving input conflict to avoid dead stops on one of the axes due to combining up and down input:
                if
                    (
                    xInput == 0 && _lastMovementInput.x != 0 &&
                    (
                    (Input.GetKey(KeyCode.LeftArrow) || (Input.GetKey(KeyCode.A))) &&
                    (Input.GetKey(KeyCode.RightArrow) || (Input.GetKey(KeyCode.D)))
                    )
                    )
                {
                    if (!_resolvingConflictX)
                    {
                        if (_lastMovementInput.x == -1 && (Input.GetKey(KeyCode.RightArrow) || (Input.GetKey(KeyCode.D))))
                        { xInput = 1; _resolvingConflictX = true; }
                        else if (_lastMovementInput.x == 1 && (Input.GetKey(KeyCode.LeftArrow) || (Input.GetKey(KeyCode.A))))
                        { xInput = -1; _resolvingConflictX = true; }
                    }
                    else { xInput = (int)_lastMovementInput.x; }
                }
                else { _resolvingConflictX = false; }

                if
                    (
                    yInput == 0 && _lastMovementInput.y != 0 &&
                    (
                    (Input.GetKey(KeyCode.UpArrow) || (Input.GetKey(KeyCode.W))) &&
                    (Input.GetKey(KeyCode.DownArrow) || (Input.GetKey(KeyCode.S)))
                    )
                    )
                {
                    if (!_resolvingConflictY)
                    {
                        if (_lastMovementInput.y == -1 && (Input.GetKey(KeyCode.UpArrow) || (Input.GetKey(KeyCode.W))))
                        { yInput = 1; _resolvingConflictY = true; }
                        else if (_lastMovementInput.y == 1 && (Input.GetKey(KeyCode.DownArrow) || (Input.GetKey(KeyCode.S))))
                        { yInput = -1; _resolvingConflictY = true; }
                    }
                    else { yInput = (int)_lastMovementInput.y; }
                }
                else { _resolvingConflictY = false; }

                if (xInput != _lastMovementInput.x) { xInputChanged = true; }
                if (yInput != _lastMovementInput.y) { yInputChanged = true; }

                if (xInputChanged && !yInputChanged)
                {
                    if (xInput != 0)
                    {
                        if (!Stunned)
                        {
                            UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                            MoveMovementPoint(xinp: xInput);
                            ResetMovementTimer();
                        }
                    }
                    else if (yInput != 0)
                    {
                        if (!Stunned)
                        {
                            UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                            MoveMovementPoint(yinp: yInput);
                            ResetMovementTimer();
                        }
                    }
                    _lastMovementInput = new Vector2(xInput, yInput);
                }
                else if (yInputChanged && !xInputChanged)
                {
                    if (yInput != 0)
                    {
                        if (!Stunned)
                        {
                            UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                            MoveMovementPoint(yinp: yInput);
                            ResetMovementTimer();
                        }
                    }
                    else if (xInput != 0)
                    {
                        if (!Stunned)
                        {
                            UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                            MoveMovementPoint(xinp: xInput);
                            ResetMovementTimer();
                        }
                    }
                    _lastMovementInput = new Vector2(xInput, yInput);
                }
                // for no change use the same movement direction as last
                else if (!yInputChanged && !xInputChanged)
                {
                    // unless has just left stun, keep moving in the same direction, otherwise, check for input
                    if (!_justLeftStun)
                    {
                        if (_lastMovementDirection.x != 0)
                        {
                            if (!Stunned)
                            {
                                UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: _lastMovementDirection.x);
                                MoveMovementPoint(xinp: _lastMovementDirection.x);
                                ResetMovementTimer();
                            }
                        }
                        else if (_lastMovementDirection.y != 0)
                        {
                            if (!Stunned)
                            {
                                UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: _lastMovementDirection.y);
                                MoveMovementPoint(yinp: _lastMovementDirection.y);
                                ResetMovementTimer();
                            }
                        }
                        _lastMovementInput = new Vector2(xInput, yInput);
                    }
                    // otherwise, default to checking X axis first for one move
                    else
                    {
                        if (xInput != 0)
                        {
                            if (!Stunned)
                            {
                                UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                                MoveMovementPoint(xinp: xInput);
                                ResetMovementTimer();
                            }
                        }
                        else if (yInput != 0)
                        {
                            if (!Stunned)
                            {
                                UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                                MoveMovementPoint(yinp: yInput);
                                ResetMovementTimer();
                            }
                        }
                        _lastMovementInput = new Vector2(xInput, yInput);
                        _justLeftStun = false;
                    }
                    
                }
                // for total change, default to checking X first. Shouldn't mess up controls too much - total change is unlikely and lasts for one movement
                else if (yInputChanged && xInputChanged)
                {
                    if (xInput != 0)
                    {
                        if (!Stunned)
                        {
                            UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                            MoveMovementPoint(xinp: xInput);
                            ResetMovementTimer();
                        }
                    }
                    else if (yInput != 0)
                    {
                        if (!Stunned)
                        {
                            UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                            MoveMovementPoint(yinp: yInput);
                            ResetMovementTimer();
                        }
                    }
                    _lastMovementInput = new Vector2(xInput, yInput);
                }
            }

            else if ((xInput == 0 && yInput == 0) && _countdownMovementCurrent <= 0)
            { _lastMovementInput = new Vector2(0, 0); }
        }

    }

    // This bit of code moves the Movement point once and waits for Player object to flip the switches back by occupying the same space as the point
    // aFromPlayer monitors whether the input comes from outside of player control to not mess with the lastMovementDirection. PlayerMovers would override direction undesireably
    public void MoveMovementPoint(bool aFromPlayer = true, float xinp = 0, float yinp = 0)
    {
        Vector3 movementDirectionV3 = new Vector3(xinp, yinp, 0);

        // Check if interactable collision is present in the desired direction of movement. If yes - movement point does not move.
        if (!Physics2D.OverlapCircle(_movementPoint.position + movementDirectionV3, ColliderSearchRadius, Layers))
        {
            _movementPoint.position += movementDirectionV3;
            movementInitiated = true;

            if (aFromPlayer) { _lastMovementDirection = movementDirectionV3; }
        }
        else { _lastMovementDirection = Vector2.zero; }
    }

    private void PeformMove()
    {
        transform.position = Vector2.MoveTowards(transform.position, _movementPoint.transform.position, SpeedAfterCalc * Time.deltaTime);
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
                        TakeDamage(enemyScript.DamagePerHit);
                    }
                }
                else if (PlayerLevel >= enemyScript.EnemyLevel)
                { enemyScript.Die(); }
            }
        }
        else if (col.tag == "Trap")
        {
            TrapScript enemyScript = col.gameObject.GetComponent<TrapScript>();
            if (!_isDamaged)
            {
                if (enemyScript.DamagePerHit > 0) _isDamaged = true;
                TakeDamage(enemyScript.DamagePerHit);
            }
        }
        else if (col.tag == "Projectile")
        { 
            ProjectileScript projScript = col.gameObject.GetComponent<ProjectileScript>();
            if (!_isDamaged)
            {
                if (projScript.Damage > 0) _isDamaged = true;
                TakeDamage(projScript.Damage);
            }
        }
    }

    public void Slow()
    { if (!Slowed) { Slowed = true; } }

    public void Stun()
    { if (!Stunned) { Stunned = true; } }

    public void LevelUp()
    {
        if (AllowLevelUp)
        {
            PlayerLevel += 1;
            _canvasManager.DrawHearts("levelup");
        }
    }

    // perhaps later a call to Animator from within this function, etc.
    public void TakeDamage(int aDamage) 
    { 
        CurrentLives = CurrentLives - aDamage;

        if (CurrentLives <= 0)
        {
            Die();
            _canvasManager.DrawHearts("death");
        }
        else 
        {
            if (aDamage < 0) { _canvasManager.DrawHearts("heal"); }
            else if (aDamage == 0) { return; }
            else { _canvasManager.DrawHearts("damage"); }
        }
    }

    private void Die() 
    {
        foreach (Transform childtr in this.gameObject.transform)
        { if (childtr.GetComponent<Camera>() != null) { childtr.parent = null; } }
        Destroy(this.gameObject);

        if (PlayerDeathObject != null)
        {
            GameObject go = Instantiate(PlayerDeathObject, transform.position, new Quaternion());
            go.name = PlayerDeathObject.name; // make sure Player spawns with name of prefab, no (clone)
            go.GetComponent<SpriteSheetSwapper_PlayerDeath>()._playerLevel = PlayerLevel;
        }
    }

    

    private void MoveDowntime()
    {
        if (_countdownMovementCurrent >= 0)
        {
            _countdownMovementCurrent -= Time.deltaTime;
        }
    }

    private void ResetMovementTimer()
    {
        _countdownMovementCurrent = CountdownMovementDefault;
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

    public void ResetStunTimer() { _currentStunTimer = StunTimer; }
    public void ResetSlowTimer() { _currentSlowTimer = SlowTimer; }

    public void StunTimerDecrement()
    {
        if (Stunned)
        {
            if (_currentStunTimer >= 0) { _currentStunTimer -= Time.deltaTime; }
            else { ResetStunTimer(); Stunned = false; xInput = 0; yInput = 0; _lastMovementDirection = Vector2.zero; _justLeftStun = true; }
        }
    }

    public void SlowTimerDecrement()
    {
        if (Slowed)
        {
            if (_currentSlowTimer >= 0) { _currentSlowTimer -= Time.deltaTime; }
            else { ResetSlowTimer(); Slowed = false; }
        }
    }

}