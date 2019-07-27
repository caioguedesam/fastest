using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    // Animation variables
    public float _idleToleranceVelocity = .15f;
    private bool _facingRight, _facingRightOld;

    // Animation references
    private Animator _animator;
    private Player _player;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _player = GetComponentInParent<Player>();
        _facingRight = true;
        _facingRightOld = true;
    }

    private void Update()
    {
        CalculateFacing();

        _animator.SetBool("isRunning", (_player._velocity.x > _idleToleranceVelocity || _player._velocity.x < -_idleToleranceVelocity));
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
