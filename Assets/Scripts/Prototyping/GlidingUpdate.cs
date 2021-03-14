using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlidingUpdate : MovementLogic
{
    public override void Cleanup(MovementController m)
    {
        // m.rb.useGravity = true;
    }

    public override void Initialize(MovementController m)
    {
        m.rb.drag = 0.5f;
        m.rb.freezeRotation = false;
        m.rb.useGravity = false;

        m.animationController.FlyingModeOn(true);
        // broomParticles.SetActive(true);
        foreach (Collider c in m.flyingColliders)
        {
            c.enabled = true;
        }
        transform.rotation = m.twistPivot.rotation;
        m.twistPivot.localRotation = Quaternion.identity;

        m.twistPivot.localRotation *= Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            0f,
            transform.rotation.eulerAngles.z);
        transform.rotation *= Quaternion.Euler(
            -transform.rotation.eulerAngles.x,
            0f,
            -transform.rotation.eulerAngles.z);

        m.twistPivot.localRotation = Quaternion.Slerp(m.twistPivot.localRotation, 
            Quaternion.identity, 1 - Mathf.Exp(-8f * Time.deltaTime));
    }

    public override void MoveFixedUpdate(MovementController m)
    {
        float thrustAmount = Input.GetAxis("Elevate");
        float elevateAmount = Input.GetAxis("Vertical");
        float yawAmount = Input.GetAxis("Yaw");
        float horizontalAmount = Input.GetAxis("Horizontal");

        // Thrust
        m.rb.AddForce(transform.forward * thrustAmount * m.moveSpeed / 2f);
        m.rb.AddForce(transform.right * horizontalAmount * m.moveSpeed / 2f);
        m.rb.AddForce(transform.up * elevateAmount * m.moveSpeed / 2f);

        // Yaw
        m.rb.AddTorque(transform.up * yawAmount * m.turnSpeed / 3f);
    }

    public override void MoveUpdate(MovementController m)
    {
        m.twistPivot.localRotation *= Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            0f,
            transform.rotation.eulerAngles.z);
        transform.rotation *= Quaternion.Euler(
            -transform.rotation.eulerAngles.x,
            0f,
            -transform.rotation.eulerAngles.z);

        m.twistPivot.localRotation = Quaternion.Slerp(m.twistPivot.localRotation,
            Quaternion.identity, 1 - Mathf.Exp(-8f * Time.deltaTime));

        m.rotationData = MovementController.GetRotationData(m.twistPivot, m.lookTowards);
        m.animationController.HandleFlyingAnimation(m.rotationData);
        if (m.groundedHandler.IsGrounded() || !m.canFly)
        {
            m.FlyingToGrounded();
        }
    }
}
