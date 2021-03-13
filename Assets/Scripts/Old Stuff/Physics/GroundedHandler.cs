using UnityEngine;

/// <summary>
/// Attach to the base of a character to check if it is grounded or not, 
/// and also returns information of the normals of nearby ground.
/// </summary>
public class GroundedHandler : MonoBehaviour
{
    private LayerMask groundLayer;
    private int groundMask;

    private void Awake()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        groundMask = 1 << groundLayer.value;
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position,
            -transform.up, 0.2f, groundMask);
    }

    public bool IsStanding()
    {
        return Physics.Raycast(transform.position,
            -transform.up, 0.2f);
    }

    public Vector3 GetGroundNormals(float distance)
    {
        if (Physics.Raycast(transform.position,
            -transform.up, out RaycastHit hitInfo, distance, groundMask))
        {
            return hitInfo.normal;
        }
        return Vector3.up;
    }
}
