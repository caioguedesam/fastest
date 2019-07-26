using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastController : MonoBehaviour
{

    // Layers to detect collision with
    public LayerMask _collisionMask;

    // Skin width value (to fire rays a little bit off the edge)
    public const float _skinWidth = .015f;

    // Ray counting variables (to store how many rays are being fired in each direction)
    public int _horizontalRayCount = 4;
    public int _verticalRayCount = 4;
    // Spacing between rays
    [HideInInspector]
    public float _horizontalRaySpacing;
    [HideInInspector]
    public float _verticalRaySpacing;

    // Box collider reference and positions
    [HideInInspector]
    public BoxCollider2D _collider;
    public RaycastOrigins _raycastOrigins;

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    // UpdateRaycastOrigins: checks for the collider boundaries and updates
    // stored value correspondingly
    public void UpdateRaycastOrigins()
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
    public void CalculateRaySpacing()
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
    public virtual void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

}
