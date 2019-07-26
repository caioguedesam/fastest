using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    // Layers which counts as passengers
    public LayerMask _passengerMask;

    public Vector3 _move;
    // Waypoint local position vector
    public Vector3[] _localWaypoints;

    List<PassengerMovement> _passengerMovement;
    Dictionary<Transform, Controller2D> _passengerDictionary = new Dictionary<Transform, Controller2D>();

    public struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform, moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = _move * Time.deltaTime;
        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    //
    private void MovePassengers(bool beforeMovePlatform)
    {
        foreach(PassengerMovement passenger in _passengerMovement)
        {
            if(!_passengerDictionary.ContainsKey(passenger.transform))
            {
                _passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if(passenger.moveBeforePlatform == beforeMovePlatform)
            {
                _passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    // Calculates passenger movement on moving platform
    private void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        _passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if(velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + _skinWidth;

            // Drawing all vertical rays based on velocity vector direction
            for (int i = 0; i < _verticalRayCount; i++)
            {
                // if velocity is pointing downwards, draw on bottom, if is upwards, draw on top
                Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
                // determining proper ray origin for i-th ray
                rayOrigin += Vector2.right * (_verticalRaySpacing * i);

                // Proper raycasting (casts i-th ray from origin, with length and vertical direcion according to movement,
                // to determined layers
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, _passengerMask);

                if (hit)
                {
                    if(!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        // Pushing the passenger the appropriate distance while closing the gap between passenger and platform
                        float pushY = velocity.y - (hit.distance - _skinWidth) * directionY;
                        // only move the passenger on the X axis if the platform is moving up
                        float pushX = (directionY == 1) ? velocity.x : 0;

                        _passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), (directionY == 1), true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
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
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _passengerMask);

                if(hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushY = -_skinWidth;
                        // only move the passenger on the X axis if the platform is moving up
                        float pushX = velocity.x - (hit.distance - _skinWidth) * directionX;

                        _passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Passenger is on top of horizontally or downward moving platform
        if(directionY == -1 || (velocity.y == 0 && velocity.x != 0))
        {
            float rayLength = 2 * _skinWidth;

            // Drawing all vertical rays based on velocity vector direction
            for (int i = 0; i < _verticalRayCount; i++)
            {
                // if velocity is pointing downwards, draw on bottom, if is upwards, draw on top
                Vector2 rayOrigin =_raycastOrigins.topLeft + Vector2.right * (_verticalRaySpacing * i);

                // Proper raycasting (casts i-th ray from origin, with length and vertical direcion according to movement,
                // to determined layers
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, _passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        // Pushing the passenger the appropriate distance while closing the gap between passenger and platform
                        float pushY = velocity.y;
                        // only move the passenger on the X axis if the platform is moving up
                        float pushX = velocity.x;

                        _passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    // Drawing platform waypoint
    private void OnDrawGizmos()
    {
        if(_localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .2f;
            float radius = .1f;

            for(int i = 0; i < _localWaypoints.Length; i++)
            {
                // Converting local positions to global positions
                Vector3 globalWaypointPosition = _localWaypoints[i] + transform.position;
                // Drawing cross on waypoints global positions 
                Gizmos.DrawLine(globalWaypointPosition - Vector3.up * size, globalWaypointPosition + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPosition - Vector3.left * size, globalWaypointPosition + Vector3.left * size);
                Gizmos.DrawWireSphere(globalWaypointPosition, radius);
            }
        }
    }

}
