using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingUpdate : MovementLogic
{
    public override void Cleanup(MovementController m)
    {
    }

    public override void MoveFixedUpdate(MovementController m)
    {
        HandleFixedFlyingInput(m);
        AddUpthrust(m); ;
    }

    public override void Initialize(MovementController m)
    {
        m.rb.drag = 0.7f;
        m.rb.freezeRotation = false;
        
        m.animationController.FlyingModeOn(true);
        // broomParticles.SetActive(true);
        foreach (Collider c in m.flyingColliders)
        {
            c.enabled = true;
        }
        transform.rotation = m.twistPivot.rotation;
        m.twistPivot.localRotation = Quaternion.identity;
    }

    public override void MoveUpdate(MovementController m)
    {
        m.rotationData = MovementController.GetRotationData(m.twistPivot, m.lookTowards);
        m.animationController.HandleFlyingAnimation(m.rotationData);
        if (m.groundedHandler.IsGrounded() || !m.canFly)
        {
            m.FlyingToGrounded();
        }
        HandleFlyingInput(m);
    }

    private void HandleFixedFlyingInput(MovementController m)
    {
        float pitchAmount = Input.GetAxis("Vertical");
        float thrustAmount = Input.GetAxis("Jump");
        float yawAmount = Input.GetAxis("Yaw");

        // Thrust
        m.rb.AddForce(transform.forward * thrustAmount * m.moveSpeed);

        // Pitch
        m.rb.AddTorque(m.invertFlyingYAxis * transform.right * pitchAmount * m.turnSpeed);
        m.rb.AddForce(transform.up * pitchAmount * m.moveSpeed);

        // Yaw
        m.rb.AddTorque(transform.up * yawAmount * m.turnSpeed / 5f);
    }

    private void HandleFlyingInput(MovementController m)
    {
        float rollAmount = Input.GetAxis("Horizontal");

        // Roll
        transform.Rotate(-transform.forward, rollAmount *
            Time.deltaTime * m.turnSpeed * 5f, Space.World);
    }

    private void AddUpthrust(MovementController m)
    {
        m.rb.AddForce(transform.up * m.rb.velocity.magnitude * transform.up.y * 25f);
    }
}
