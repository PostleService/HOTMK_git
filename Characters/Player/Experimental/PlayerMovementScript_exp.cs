using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovementScript_exp : MonoBehaviour
{
    // This is the actual object controlled by keyboard input. The parent object will attempt to move towards it.
    private InputControl _inputControl;
    private PlayerScript _playerScript;
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;

    private float _inputValueXRight = 0f;
    private float _inputValueXLeft = 0f;
    private float _inputValueYUp = 0f;
    private float _inputValueYDown = 0f;
    public Vector2 _joystickValue = Vector2.zero;

    [HideInInspector] public int xInput;
    [HideInInspector] public int yInput;
    private Vector2 _lastMovementInput;
    private Vector2 _lastMovementDirection;
    private bool _resolvingConflictX = false;
    private bool _resolvingConflictY = false;

    public float PlayerSpeed = 3f;
    public float PlayerMaxSpeed = 3f;

    private void OnEnable()
    {
        _inputControl = new InputControl();
        _inputControl.PlayerControls.Enable();

        #region InputSubscriptions

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
    }

    private void OnDisable()
    {
        _inputControl.PlayerControls.Disable();

        #region InputSubscriptions

        _inputControl.PlayerControls.PlayerInput_Right.performed -= (value) => _inputValueXRight = 1;
        _inputControl.PlayerControls.PlayerInput_Right.canceled -= (value) => _inputValueXRight = 0f;

        _inputControl.PlayerControls.PlayerInput_Left.performed -= (value) => _inputValueXLeft = -1;
        _inputControl.PlayerControls.PlayerInput_Left.canceled -= (value) => _inputValueXLeft = 0f;

        _inputControl.PlayerControls.PlayerInput_Up.performed -= (value) => _inputValueYUp = 1;
        _inputControl.PlayerControls.PlayerInput_Up.canceled -= (value) => _inputValueYUp = 0f;

        _inputControl.PlayerControls.PlayerInput_Down.performed -= (value) => _inputValueYDown = -1;
        _inputControl.PlayerControls.PlayerInput_Down.canceled -= (value) => _inputValueYDown = 0f;

        _inputControl.PlayerControls.PlayerInput_Joystick.performed -= (value) => _joystickValue = value.ReadValue<Vector2>();
        _inputControl.PlayerControls.PlayerInput_Joystick.canceled -= (value) => _joystickValue = Vector2.zero;

        #endregion InputSubscriptions
    }

    private void Awake()
    {
        _playerScript = GetComponent<PlayerScript>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        ProcessInput();
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

        // _animator.SetFloat("Speed", SpeedAfterCalc);
        // _animator.speed = _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier;

        bool xInputChanged = false; bool yInputChanged = false;

        bool directionPressed = false;
        if (UpArrowPressed || DownArrowPressed || LeftArrowPressed || RightArrowPressed)
        { directionPressed = true; }

        // overall check for input change
        // if (directionPressed <= 0 && _currentPostSpawnCannotMove <= 0)
        if (directionPressed)
        {
            Debug.LogWarning("Direction Pressed");
            //SpeedAfterCalc = MovementSpeedDefault * _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier;

            // resolving input conflict to avoid dead stops on one of the axes due to combining up and down input:
            if (xInput == 0 && _lastMovementInput.x != 0 && (LeftArrowPressed && RightArrowPressed))
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

            if (yInput == 0 && _lastMovementInput.y != 0 && (UpArrowPressed && DownArrowPressed))
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

                    if (!_playerScript.Stunned)
                    {
                        MovePlayer(xinp: xInput);
                        UpdateAnimatorHandlerValues(aHorizontalDirection: xInput);
                        /*   UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                           MoveMovementPoint(xinp: xInput);
                           ResetMovementTimer();*/
                    }
                }
                else if (yInput != 0)
                {

                    if (!_playerScript.Stunned)
                    {
                        MovePlayer(yinp: yInput);
                        UpdateAnimatorHandlerValues(aVerticalDirection: yInput);
                        /*   UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                           MoveMovementPoint(yinp: yInput);
                           ResetMovementTimer();*/
                    }
                }
                _lastMovementInput = new Vector2(xInput, yInput);
            }
            else if (yInputChanged && !xInputChanged)
            {
                if (yInput != 0)
                {
                    if (!_playerScript.Stunned)
                    {
                        MovePlayer(yinp: yInput);
                        UpdateAnimatorHandlerValues(aVerticalDirection: yInput);
                        /*   UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                           MoveMovementPoint(yinp: yInput);
                           ResetMovementTimer();*/
                    }
                }
                else if (xInput != 0)
                {
                    if (!_playerScript.Stunned)
                    {
                        MovePlayer(xinp: xInput);
                        UpdateAnimatorHandlerValues(aHorizontalDirection: xInput);
                        /*UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                        MoveMovementPoint(xinp: xInput);
                        ResetMovementTimer();*/
                    }
                }
                _lastMovementInput = new Vector2(xInput, yInput);
            }
            // for no change use the same movement direction as last
            else if (!yInputChanged && !xInputChanged)
            {
                Debug.LogWarning("Hasn't changed");
                // unless has just left stun, keep moving in the same direction, otherwise, check for input
                if (!_playerScript._justLeftStun)
                {
                    if (_lastMovementDirection.x != 0)
                    {
                        if (!_playerScript.Stunned)
                        {
                            MovePlayer(xinp: xInput);
                            UpdateAnimatorHandlerValues(aHorizontalDirection: xInput);
                            /*UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: _lastMovementDirection.x);
                            MoveMovementPoint(xinp: _lastMovementDirection.x);
                            ResetMovementTimer();*/
                        }
                    }
                    else if (_lastMovementDirection.y != 0)
                    {
                        if (!_playerScript.Stunned)
                        {
                            MovePlayer(yinp: yInput);
                            UpdateAnimatorHandlerValues(aVerticalDirection: yInput);
                            /*UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: _lastMovementDirection.y);
                            MoveMovementPoint(yinp: _lastMovementDirection.y);
                            ResetMovementTimer();*/
                        }
                    }
                    _lastMovementInput = new Vector2(xInput, yInput);
                }
                // otherwise, default to checking X axis first for one move
                else
                {
                    if (xInput != 0)
                    {
                        if (!_playerScript.Stunned)
                        {
                            MovePlayer(xinp: xInput);
                            UpdateAnimatorHandlerValues(aHorizontalDirection: xInput);
                            /*UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                            MoveMovementPoint(xinp: xInput);
                            ResetMovementTimer();*/
                        }
                    }
                    else if (yInput != 0)
                    {
                        if (!_playerScript.Stunned)
                        {
                            MovePlayer(yinp: yInput);
                            UpdateAnimatorHandlerValues(aVerticalDirection: yInput);
                            /*   UpdateAnimatorHandlerValues(SpeedAfterCalc, aVerticalDirection: yInput);
                               MoveMovementPoint(yinp: yInput);
                               ResetMovementTimer();*/
                        }
                    }
                    _lastMovementInput = new Vector2(xInput, yInput);
                    _playerScript._justLeftStun = false;
                }

            }
            // for total change, default to checking X first. Shouldn't mess up controls too much - total change is unlikely and lasts for one movement
            else if (yInputChanged && xInputChanged)
            {
                if (xInput != 0)
                {
                    if (!_playerScript.Stunned)
                    {
                        MovePlayer(xinp: xInput);
                        UpdateAnimatorHandlerValues(aHorizontalDirection: xInput);
                        /*UpdateAnimatorHandlerValues(SpeedAfterCalc, aHorizontalDirection: xInput);
                        MoveMovementPoint(xinp: xInput);
                        ResetMovementTimer();*/
                    }
                }
                else if (yInput != 0)
                {
                    if (!_playerScript.Stunned)
                    {
                        MovePlayer(yinp: yInput);
                        UpdateAnimatorHandlerValues(aVerticalDirection: yInput);
                        /*MoveMovementPoint(yinp: yInput);
                        ResetMovementTimer();*/
                    }
                }
                _lastMovementInput = new Vector2(xInput, yInput);
            }
        }

        else if ((xInput == 0 && yInput == 0))
        { _lastMovementInput = new Vector2(0, 0); }

    }

    private void MovePlayer(bool aFromPlayer = true, float xinp = 0, float yinp = 0)
    {
        Vector2 direction = new Vector2(xinp, yinp);
        _rigidbody2D.velocity = Vector2.ClampMagnitude(direction * PlayerSpeed, PlayerMaxSpeed);
        _lastMovementDirection = direction;
    }

    private void UpdateAnimatorHandlerValues(float aHorizontalDirection = 0, float aVerticalDirection = 0)
    {
        _animator.SetFloat("Horizontal", aHorizontalDirection);
        _animator.SetFloat("Vertical", aVerticalDirection);
        _animator.speed = 1;
        // _animator.speed = _currentDamageAcceleration * _currentStunSpeedModifier * _currentSlowedSpeedModifier; // update animation speed based on status effects
    }


}
