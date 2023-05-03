using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    private float _currentStunTimer;
    private float _currentSlowTimer;
    private float _currentPostSpawnCannotMove = 0.1f;

    [Tooltip("How far in units does the player have to be from distance point to accept new movemnt input")]
    [HideInInspector] public float MovementSwitchFlipDistance = 0f;

    [Header("Collision")]
    [Tooltip("Layers with which object will collide")]
    public LayerMask Layers;
    [Tooltip("Radius in which to search for collider in the next tile ")]
    public float ColliderSearchRadius = 0.2f;

    // This is the actual object controlled by keyboard input. The parent object will attempt to move towards it.
    private InputControl _inputControl;

    private float _inputValueXRight = 0f;
    private float _inputValueXLeft = 0f;
    private float _inputValueYUp = 0f;
    private float _inputValueYDown = 0f;
    public Vector2 _joystickValue = Vector2.zero;

    private Transform _movementPoint;
    [HideInInspector] public int xInput;
    [HideInInspector] public int yInput;
    [HideInInspector] public bool movementInitiated = false;
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

    public delegate void MyHandler (int aCurrentHealth, int aMaxHealth, string aUpdateState);
    public static event MyHandler OnHealthUpdate;
    public delegate void SpawnDelegate (GameObject aGameObject);
    public static event SpawnDelegate OnSpawn;
    public delegate void PositionTracker (GameObject aGameObject, Vector2 aPosition);
    public static event PositionTracker OnPositionChange;
    public static event Action OnRememberFog;
    public static event Action OnEnemiesDeconceal;

    private void OnEnable()
    { 
        LevelManagerScript.OnLevelStageChange += LevelUp;
        _inputControl.PlayerControls.Enable();
    }

    private void OnDisable()
    { 
        LevelManagerScript.OnLevelStageChange -= LevelUp;
        _inputControl.PlayerControls.Disable();
    }

    private void Awake()
    {
        #region InputSubscriptions
        _inputControl = new InputControl();

        _inputControl.PlayerControls.PlayerInput_Right.performed += (value) => _inputValueXRight = 1;
        _inputControl.PlayerControls.PlayerInput_Right.canceled += (value) => _inputValueXRight = 0f;

        _inputControl.PlayerControls.PlayerInput_Left.performed += (value) => _inputValueXLeft = -1;
        _inputControl.PlayerControls.PlayerInput_Left.canceled += (value) => _inputValueXLeft = 0f;

        _inputControl.PlayerControls.PlayerInput_Up.performed += (value) => _inputValueYUp = 1;
        _inputControl.PlayerControls.PlayerInput_Up.canceled += (value) => _inputValueYUp = 0f;

        _inputControl.PlayerControls.PlayerInput_Down.performed += (value) => _inputValueYDown = -1;
        _inputControl.PlayerControls.PlayerInput_Down.canceled += (value) => _inputValueYDown = 0f;

        _inputControl.PlayerControls.PlayerInput_Joystick.performed += (value) => _joystickValue = value.ReadValue<Vector2>();
        _inputControl.PlayerControls.PlayerInput_Joystick.canceled += (value) => _joystickValue = Vector2.zero;

        #endregion InputSubscriptions

        ResetMovementTimer();
        ResetIFramesTimer();
    }

    private void Start()
    {
        OnSpawn?.Invoke(this.gameObject); // tell all related managers that player has spawned
        OnPositionChange?.Invoke(this.gameObject, transform.position);

        CurrentLives = MaxPlayerLives;
        OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartSpawn");
        
        GameObject.Find("PlayerCamera").transform.SetParent(gameObject.transform);
        _animator = gameObject.GetComponent<Animator>();

        FindMovementPoint();
    }

    private void FixedUpdate()
    {
        if (_currentPostSpawnCannotMove >= 0) { _currentPostSpawnCannotMove -= Time.fixedDeltaTime; }

        ProcessInput();
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

        OnPositionChange?.Invoke(this.gameObject, transform.position);
    }

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

    private void ProcessInput()
    {
        bool UpArrowPressed = false;
        bool DownArrowPressed = false;
        bool RightArrowPressed = false;
        bool LeftArrowPressed = false;

        #region JoystickInput

        Vector2 JoystickAxis = Vector2.zero;

        if (Mathf.Abs(_joystickValue.x) > Mathf.Abs(_joystickValue.y))
        {
            if (_joystickValue.x < 0)
            {
                JoystickAxis.x = -1;
                LeftArrowPressed = true;
            }

            else if (_joystickValue.x > 0)
            {
                JoystickAxis.x = 1;
                RightArrowPressed = true;
            }
        }
        else if (Mathf.Abs(_joystickValue.y) > Mathf.Abs(_joystickValue.x))
        {
            if (_joystickValue.y < 0)
            {
                JoystickAxis.y = -1;
                DownArrowPressed = true;
            }
            else if (_joystickValue.y > 0)
            {
                JoystickAxis.y = 1;
                UpArrowPressed = true;
            }
        }

        #endregion JoystickInput

        #region AnalogKeyInput

        float axisX = 0f; float axisY = 0f;
        Vector2 AnalogKeyAxis = Vector2.zero;

        axisX = _inputValueXLeft + _inputValueXRight;
        axisY = _inputValueYDown + _inputValueYUp;

        AnalogKeyAxis = new Vector2(axisX, axisY);

        if (_inputValueXLeft != 0) LeftArrowPressed = true;
        if (_inputValueXRight != 0) RightArrowPressed = true;
        if (_inputValueYDown != 0) DownArrowPressed = true;
        if (_inputValueYUp != 0) UpArrowPressed = true;

        #endregion KeyboardInput

        #region UnifiedInput

        float xAxisUnified = 0f; float yAxisUnified = 0f;

        xAxisUnified = JoystickAxis.x + AnalogKeyAxis.x;
        yAxisUnified = JoystickAxis.y + AnalogKeyAxis.y;

        if (xAxisUnified < 0) xInput = -1;
        else if (xAxisUnified > 0) xInput = 1;
        else xInput = 0;

        if (yAxisUnified < 0) yInput = -1;
        else if (yAxisUnified > 0) yInput = 1;
        else yInput = 0;

        #endregion UnifiedInput

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
            if ( UpArrowPressed || DownArrowPressed || LeftArrowPressed || RightArrowPressed )
            { directionPressed = true; }

            // overall check for input change
            if (directionPressed && _countdownMovementCurrent <= 0 && _currentPostSpawnCannotMove <= 0)
            {
                SpeedAfterCalc = MovementSpeedDefault * _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier;

                // resolving input conflict to avoid dead stops on one of the axes due to combining up and down input:
                if ( xInput == 0 && _lastMovementInput.x != 0 && ( LeftArrowPressed && RightArrowPressed ) )
                {
                    if (!_resolvingConflictX)
                    {
                        if (_lastMovementInput.x == -1 && RightArrowPressed)
                        { xInput = 1; _resolvingConflictX = true; }
                        else if (_lastMovementInput.x == 1 && LeftArrowPressed)
                        { xInput = -1; _resolvingConflictX = true; }
                    }
                    else { xInput = (int)_lastMovementInput.x; }
                }
                else { _resolvingConflictX = false; }

                if ( yInput == 0 && _lastMovementInput.y != 0 && ( UpArrowPressed && DownArrowPressed ) )
                {
                    if (!_resolvingConflictY)
                    {
                        if (_lastMovementInput.y == -1 && UpArrowPressed)
                        { yInput = 1; _resolvingConflictY = true; }
                        else if (_lastMovementInput.y == 1 && DownArrowPressed)
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
                { enemyScript.Die(false); }
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
                if (projScript.DamagePerHit > 0) _isDamaged = true;
                TakeDamage(projScript.DamagePerHit);
            }
        }
    }

    public void Slow(float aLength)
    { if (!Slowed) { Slowed = true; ResetSlowTimer(aLength); } }

    public void Stun(float aLength)
    { if (!Stunned) { Stunned = true; ResetStunTimer(aLength); } }

    public void SyncronizeLevelUps(int aLevelStage)
    {
        // in case player spawns lower level than the stage for any reason
        while (aLevelStage < 3 && PlayerLevel < aLevelStage)
        {
            // go through each lvl one by one
            int NewLevel = PlayerLevel + 1;
            LevelUp(NewLevel, 0,0);
        }
    }

    public void LevelUp(int aLevelStage, int aCurrentItems, int aDefaultItems)
    {
        if (aLevelStage < 3 && AllowLevelUp)
        {
            PlayerLevel = aLevelStage;
            if (PlayerLevel == RememberFogAtLevelStage) { RememberFog(); }
            if (PlayerLevel == CanSeeThroughWallsAtStage) { DeconcealEnemies(); }
            OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartLevelUp");
        }
    }

    public void RememberFog() { OnRememberFog?.Invoke(); }
    public void DeconcealEnemies() { OnEnemiesDeconceal?.Invoke(); }

    // perhaps later a call to Animator from within this function, etc.
    public void TakeDamage(int aDamage) 
    { 
        CurrentLives = CurrentLives - aDamage;

        if (CurrentLives <= 0)
        { Die(); }
        else 
        {
            if (aDamage < 0) { OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartHeal"); }
            else if (aDamage == 0) { return; }
            else { OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartDamage"); }
        }
    }

    private void Die() 
    {
        foreach (Transform childtr in this.gameObject.transform)
        { if (childtr.GetComponent<Camera>() != null) { childtr.parent = null; } }
        OnHealthUpdate?.Invoke(CurrentLives, MaxPlayerLives, "HeartDeath");
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

    public void ResetStunTimer(float aLength) { _currentStunTimer = aLength; }
    public void ResetSlowTimer(float aLength) { _currentSlowTimer = aLength; }

    public void StunTimerDecrement()
    {
        if (Stunned)
        {
            if (_currentStunTimer >= 0) { _currentStunTimer -= Time.deltaTime; }
            else { Stunned = false; xInput = 0; yInput = 0; _lastMovementDirection = Vector2.zero; _justLeftStun = true; }
        }
    }

    public void SlowTimerDecrement()
    {
        if (Slowed)
        {
            if (_currentSlowTimer >= 0) { _currentSlowTimer -= Time.deltaTime; }
            else { Slowed = false; }
        }
    }

}