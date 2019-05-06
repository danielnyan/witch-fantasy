using UnityEngine;

/// <summary>
/// Handles the animation for MovementController
/// </summary>
public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public bool IsOnFloor { get; private set; }

    // Start is called before the first frame update
    private void OnEnable()
    {
        animator = GetComponentInChildren<Animator>(includeInactive: true);
    }
    
    public void FiringModeOn(bool value)
    {
        animator.SetBool("Firing Mode On", value);
    }

    public void Yeet()
    {
        animator.SetTrigger("Yeet");
    }

    public void TouchFloor(bool value)
    {
        animator.SetBool("Is On Floor", value);
        IsOnFloor = value;
    }

    public void FlyingModeOn(bool value)
    {
        animator.SetBool("Flying Mode On", value);
    }

    public void HandleGroundedAnimation(Vector3 rotationData, float horizontal, float vertical)
    {
        float bodyAngleY = rotationData.y * 2f / 90f;
        bodyAngleY = Mathf.Lerp(animator.GetFloat("Vertical Body Direction"),
            Mathf.Clamp(bodyAngleY, -1, 1), 0.2f);
        animator.SetFloat("Vertical Body Direction", bodyAngleY);
        float forwardTarget;
        if (vertical > 0.001f || (vertical >= -0.001f && Mathf.Abs(horizontal) > 0.001f))
        {
            forwardTarget = Mathf.Lerp(animator.GetFloat("Forward Movement Amount"),
                1f, 0.2f);
        } else if (vertical < -0.001f)
        {
            forwardTarget = Mathf.Lerp(animator.GetFloat("Forward Movement Amount"),
                -1f, 0.2f);
        } else
        {
            forwardTarget = Mathf.Lerp(animator.GetFloat("Forward Movement Amount"),
                0f, 0.2f);
        }
        animator.SetFloat("Forward Movement Amount", forwardTarget);
    }

    public void HandleMidairAnimation(float verticalVelocity)
    {
        animator.SetFloat("Vertical Velocity", verticalVelocity);
    }

    public void HandleFlyingAnimation(Vector3 rotationData)
    {
        // Rotation data returns values within the range (-90, 90)
        float bodyAngleX = rotationData.x * 2f / 90f;
        float bodyAngleY = rotationData.y * 2f / 90f;

        // The values of bodyAngleX and bodyAngleY lie within (-2, 2), 
        // where -2 and 2 corresponds to -90 and 90 degrees respectively. 
        bodyAngleX = Mathf.Lerp(animator.GetFloat("Horizontal Body Direction"),
            Mathf.Clamp(bodyAngleX, -2, 2), 0.2f);
        bodyAngleY = Mathf.Lerp(animator.GetFloat("Vertical Body Direction"),
            Mathf.Clamp(bodyAngleY, -2, 2), 0.2f);
        animator.SetFloat("Vertical Body Direction", bodyAngleY);
        animator.SetFloat("Horizontal Body Direction", bodyAngleX);
    }

    public bool IsReady()
    {
        return (animator != null);
    }
}
