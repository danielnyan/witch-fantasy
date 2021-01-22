using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMovementController : MonoBehaviour
{
    private Animator animator;
    public bool IsOnFloor { get; private set; }

    // Start is called before the first frame update
    private void OnEnable()
    {
        animator = GetComponentInChildren<Animator>(includeInactive: true);
    }

    public void TouchFloor(bool value)
    {
        animator.SetBool("Is On Floor", value);
        IsOnFloor = value;
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
        }
        else if (vertical < -0.001f)
        {
            forwardTarget = Mathf.Lerp(animator.GetFloat("Forward Movement Amount"),
                -1f, 0.2f);
        }
        else
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

    public bool IsReady()
    {
        return (animator != null);
    }
}
