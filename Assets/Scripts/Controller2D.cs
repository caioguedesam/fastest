using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Controller2D:
/// This script manages a Character Controller in a 2D platformer environment.
/// Should be used in the future for reference or in other similar projects.
/// 
/// Functionality includes a raycasting system which detects collisions with player
/// and a movement function which moves the player and stops movement on corresponding
/// axis if a collision is found by the raycasts.
/// 
/// Heavily inspired on Sebastian Lague's series of Unity 2D platformers (https://www.youtube.com/channel/UCmtyQOKKmrMVaKuRXz02jbQ)
/// 
/// Caio Guedes, 07/2019


[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    // Layers to detect collision with
    public LayerMask _collisionMask;

    // Skin width value (to fire rays a little bit off the edge)
    const float _skinWidth = .015f;

    // Ray counting variables (to store how many rays are being fired in each direction)
    public int _horizontalRayCount = 4;
    public int _verticalRayCount = 4;
    // Spacing between rays
    private float _horizontalRaySpacing;
    private float _verticalRaySpacing;

    // Box collider reference and positions
    private BoxCollider2D _collider;
    RaycastOrigins _raycastOrigins;

    // Collision information reference
    public CollisionInfo _collisions;

    // Maximum climbable slope angle
    public float _maxClimbAngle = 80f;

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below, left, right;
        public bool climbingSlope;
        public float slopeAngle, slopeAngleOld;

        public void Reset()
        {
            above = below = left = right = false;
            climbingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0f;
        }
    }

    // UpdateRaycastOrigins: checks for the collider boundaries and updates
    // stored value correspondingly
    private void UpdateRaycastOrigins()
    {
        Bounds bounds = _collider.bounds;
        bounds.Expand(_skinWidth * -2);

        _raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // CalculateRaySpacing: calculates space between rays based on ray count and
    // collider boundary positions
    private void CalculateRaySpacing()
    {
        // Getting boundaries
        Bounds bounds = _collider.bounds;
        bounds.Expand(_skinWidth * -2);

        // At least 2 rays firing at all times (at least one on each corner)
        _horizontalRayCount = Mathf.Clamp(_horizontalRayCount, 2, int.MaxValue);
        _verticalRayCount = Mathf.Clamp(_verticalRayCount, 2, int.MaxValue);

        // Setting ray spacing to be size of boundaries on corresponding axis
        // divided by the number of spaces between rays
        _horizontalRaySpacing = bounds.size.y / (_horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (_verticalRayCount - 1);
    }

    // HorizontalCollisions: Casts rays horizontally based on horizontal velocity direction and length
    private void HorizontalCollisions(ref Vector3 velocity)
    {
        // Getting ray direction and length
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + _skinWidth;


        // Drawing all horizontal rays based on velocity vector direction
        for (int i = 0; i < _horizontalRayCount; i++)
        {
            // if velocity is pointing left, draw on left, if is right, draw on right
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            // determining proper ray origin for i-th ray
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);

            // Proper raycasting (casts i-th ray from origin, with length and horizontal direcion according to movement,
            // to determined layers
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit)
            {
                // Getting the angle of hit surface
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // Checking to see if there is a slope collision
                if (i == 0 && slopeAngle <= _maxClimbAngle)
                {
                    // Checking to see if there's a new slope
                    float distanceToSlopeStart = 0f;
                    if(slopeAngle != _collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - _skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                // Check other rays only if not climbing slope
                if(!_collisions.climbingSlope || slopeAngle > _maxClimbAngle)
                {
                    // This stops the player if it finds a horizontal collision
                    velocity.x = (hit.distance - _skinWidth) * directionX;
                    rayLength = hit.distance;

                    // Setting collisions to be equal to the horizontal direction value for each direction
                    _collisions.left = (directionX == -1);
                    _collisions.right = (directionX == 1);
                }
            }  
        }
    }

    // HorizontalCollisions: Casts rays vertically based on horizontal velocity direction and length
    private void VerticalCollisions(ref Vector3 velocity)
    {
        // Getting ray direction and length
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + _skinWidth;


        // Drawing all vertical rays based on velocity vector direction
        for (int i = 0; i < _verticalRayCount; i++)
        {
            // if velocity is pointing downwards, draw on bottom, if is upwards, draw on top
            Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            // determining proper ray origin for i-th ray (assing x velocity to only cast rays after movement is done on x axis)
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + velocity.x);

            // Proper raycasting (casts i-th ray from origin, with length and vertical direcion according to movement,
            // to determined layers
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, _collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit)
            {
                // This stops the player if it finds a vertical collision
                velocity.y = (hit.distance - _skinWidth) * directionY;
                rayLength = hit.distance;

                // Setting collisions to be equal to the vertical direction value for each direction
                _collisions.below = (directionY == -1);
                _collisions.above = (directionY == 1);
            }
        }
    }

    public void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = moveDistance * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
        if(velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = moveDistance * Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
            _collisions.below = true;
            _collisions.climbingSlope = true;
            _collisions.slopeAngle = slopeAngle;
        }
    }

    public void Move(Vector3 velocity)
    {
        // Resetting collisions for every move
        _collisions.Reset();
        // Updating ray positions
        UpdateRaycastOrigins();

        // Casting collision rays if velocity on corresponding axis is not null
        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);
        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        // Moving the player along velocity vector
        transform.Translate(velocity);
    }

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }
}
