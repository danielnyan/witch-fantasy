using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedUpdate : MovementLogic
{
    public override void Cleanup(MovementController m)
    {
        m.groundCollider.enabled = false;
        m.IsGrounded = false;
    }

    public override void MoveFixedUpdate(MovementController m)
    {
        HandleFixedGroundedInput(m);
    }

    public override void Initialize(MovementController m)
    {
        m.rb.drag = 0.1f;
        m.rb.freezeRotation = true;
        m.groundCollider.enabled = true;
        m.animationController.FlyingModeOn(false);
        // broomParticles.SetActive(false);
        foreach (Collider c in m.flyingColliders)
        {
            c.enabled = false;
        }
        transform.rotation = m.twistPivot.rotation;
        m.twistPivot.localRotation = Quaternion.identity;
        m.IsGrounded = true;
        m.rb.velocity = Vector3.zero;
    }

    public override void MoveUpdate(MovementController m)
    {
        SetGroundedDirection(m.lookTowards, m);
        m.rotationData = MovementController.GetRotationData(m.twistPivot, m.lookTowards);
        if (HandleGroundedInput(m))
        {
            return;
        }

        if (!m.groundedHandler.IsStanding())
        {
            m.rb.useGravity = true;
            m.animationController.HandleMidairAnimation(m.rb.velocity.y);
            if (m.animationController.IsOnFloor)
            {
                m.animationController.TouchFloor(false);
            }
        }
        else
        {
            if (m.rb.useGravity)
            {
                m.rb.useGravity = false;
                m.animationController.HandleMidairAnimation(0f);
            }
            if (!m.animationController.IsOnFloor)
            {
                m.animationController.TouchFloor(true);
            }
            m.animationController.HandleGroundedAnimation(m.rotationData,
            Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

    }

    private void SetGroundedDirection(Transform lookTowards, MovementController m)
    {
        m.transform.up = Vector3.Slerp(m.transform.up, m.groundedHandler.GetGroundNormals(50f),
            0.1f).normalized;
        Vector3 lookDirection = lookTowards.position - m.transform.position;
        Vector3 flattenedLook = lookDirection - Vector3.Dot(lookDirection, m.transform.up)
            / Vector3.SqrMagnitude(m.transform.up) * m.transform.up;
        m.twistPivot.rotation = Quaternion.LookRotation(flattenedLook, m.transform.up);
    }

    private bool HandleGroundedInput(MovementController m)
    {
        bool isFlying = false;
        float jumpAmount = Input.GetAxis("Jump");

        // To add: if is not grounded and conditions below, fly. Implement no-fly zones by using on trigger stay, 
        // or perhaps add a "canFly" attribute which toggles when entering a zone. 
        if (Mathf.Abs(jumpAmount) > 0.001f && !m.jumpKeyHeld)
        {
            if (m.groundedHandler.IsStanding())
            {
                m.rb.velocity -= Vector3.Dot(m.rb.velocity, m.twistPivot.up) * m.twistPivot.up;
                m.rb.velocity += m.twistPivot.up * 7f;
            }
            else if (m.canFly)
            {
                m.GroundedToFlying();
                isFlying = true;
            }
            else if (m.flyingDebugCooldown < 0f)
            {
                m.canFly = CheckNoFlyZone(m);
                if (m.canFly)
                {
                    m.GroundedToFlying();
                    isFlying = true;
                }
                m.flyingDebugCooldown = 1f;
            }
            m.jumpKeyHeld = true;
        }
        else if (Mathf.Abs(jumpAmount) <= 0.001f)
        {
            m.jumpKeyHeld = false;
        }
        return isFlying;
    }

    // Let's hope this doesn't break anything in the future. uwu
    private void HandleFixedGroundedInput(MovementController m)
    {
        float forwardAmount = Input.GetAxis("Vertical");
        float sideAmount = Input.GetAxis("Horizontal");
        if (Mathf.Abs(forwardAmount) > 0.001f)
        {
            m.transform.position += m.twistPivot.forward * forwardAmount * Time.fixedDeltaTime * m.moveSpeed / 200f;
        }
        if (Mathf.Abs(sideAmount) > 0.001f)
        {
            m.transform.position += m.twistPivot.right * sideAmount * Time.fixedDeltaTime * m.moveSpeed / 200f;
        }
    }

    // Handles an edge case when OnTriggerEnter is called after OnTriggerExit due to race conditions, 
    // thus causing the player to be unable to fly despite not being in a No Fly Zone. 
    private bool CheckNoFlyZone(MovementController m)
    {
        int eventLayer = LayerMask.NameToLayer("EventZone");
        RaycastHit[] hitColliders = Physics.RaycastAll(m.transform.position, m.transform.up * 0.2f, 1 << eventLayer);
        foreach (RaycastHit hit in hitColliders)
        {
            if (hit.transform.CompareTag("No Fly Zone"))
            {
                return false;
            }
        }
        return true;
    }
}
