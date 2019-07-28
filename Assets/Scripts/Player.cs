using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Player:
/// This script manages a Player in a 2D platformer environment.
/// Should be used in the future for reference or in other similar projects.
/// 
/// Includes general functions and references, information on player variables and input
/// and environmental variables such as gravity
/// 
/// Heavily inspired on Sebastian Lague's series of Unity 2D platformers (https://www.youtube.com/channel/UCmtyQOKKmrMVaKuRXz02jbQ)
/// 
/// Caio Guedes, 07/2019

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    // Input Variables
    private float _horizontalInput, _verticalInput;
    private bool _jumpInput;

    // Player variables
    public float _moveSpeed = 6f;
    [Range(0f, 10f)]
    public float _jumpHeight = 4f;
    [Range(0f, 10f)]
    public float _distanceToJumpMax = 2.5f;
    public float _accelerationTimeAirborne = .2f;
    public float _accelerationTimeGrounded = .1f;

    private float _velocityXSmoothing;
    private float _timeToJumpMax;
    private float _gravity;
    [HideInInspector] public Vector3 _velocity;
    private float _jumpVelocity;

    // Player references
    private Controller2D _controller;

    // Animation variables
    public float _runAnimTolerance = .15f;
    public float _fallAnimTolerance = .2f;
    private bool _facingRight, _facingRightOld;

    // Animation references
    private Animator _animator;
    private Player _player;

    // GetInput: gets and stores data for all kinds of player input
    private void GetInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        _jumpInput = Input.GetButtonDown("Jump");
    }

    private void Awake()
    {
        _controller = GetComponent<Controller2D>();

        // Calculating jump variables based on horizontal speed and jump distance
        _timeToJumpMax = _distanceToJumpMax / _moveSpeed;
        _jumpVelocity = (2 * _jumpHeight) / _timeToJumpMax;
        _gravity = (-2 * _jumpHeight) / Mathf.Pow(_timeToJumpMax, 2);

        // Animation set up
        _animator = GetComponentInChildren<Animator>();
        _player = GetComponent<Player>();
        _facingRight = true;
        _facingRightOld = true;
    }

    void Update()
    {
        // Checking player's front direction and updating scale accordingly
        CalculateFacing();

        // If player is grounded or collides with ceiling, stops vertical velocity
        if (_controller._collisions.above || _controller._collisions.below)
        {
            _velocity.y = 0f;
        }

        // Getting player input
        GetInput();

        // Jumping
        if(_jumpInput && _controller._collisions.below)
        {
            _velocity.y = _jumpVelocity;

            // Controlling jumping up animation
            _animator.Play("Player_jump_up");
        }

        // Controlling transition animation
        _animator.SetBool("jumpTransition", (_velocity.y < _fallAnimTolerance && _velocity.y > -_fallAnimTolerance) ? true : false);

        // Controlling landing animation
        _animator.SetBool("hasLanded", (_controller._collisions.below && _controller._collisions.velocityOld.y < 0) ? true : false);

        // Setting horizontal speed
        float targetVelocityX = _horizontalInput * _moveSpeed;
        float accelerationX = _controller._collisions.below ? _accelerationTimeGrounded : _accelerationTimeAirborne;
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, accelerationX);

        // Applying gravity
        _velocity.y += _gravity * Time.deltaTime;

        // Moving the player
        _controller.Move(_velocity * Time.deltaTime);

        // Controlling running animation
        _animator.SetBool("isRunning", (_player._velocity.x > _runAnimTolerance || _player._velocity.x < -_runAnimTolerance));
    }

    private void CalculateFacing()
    {
        _facingRightOld = _facingRight;

        if (_player._velocity.x > 0)
            _facingRight = true;
        else if (_player._velocity.x < 0)
            _facingRight = false;

        if (_facingRightOld != _facingRight)
        {
            Vector3 _localScale = GetComponentInParent<Transform>().localScale;
            _localScale.x *= -1;
            GetComponentInParent<Transform>().localScale = _localScale;
        }
    }
}
