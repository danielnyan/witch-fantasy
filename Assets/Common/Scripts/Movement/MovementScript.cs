using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public float moveSpeed = 800f;

    private Rigidbody rb;
    private AnimationMovementController animationController;
    private Transform lookTowards;
    private bool jumpKeyHeld;
    private Vector3 rotationData = Vector3.zero;
    [SerializeField]
    private Transform twistPivot;
    public bool IsGrounded { get; private set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // To do: refactor this into a new script
        animationController = GetComponent<AnimationMovementController>();
        Camera.main.transform.root.GetComponent<CameraController>().TrackObject(transform);
        lookTowards = Camera.main.transform.root.Find("Camera Look Direction");
    }

    private void Update()
    {
        GroundedUpdate();
    }

    private void FixedUpdate()
    {
        GroundedFixedUpdate();
    }

    private void GroundedUpdate()
    {
        SetGroundedDirection(lookTowards);
        rotationData = GetRotationData(twistPivot, lookTowards);
        HandleGroundedInput();
        if (!IsStanding())
        {
            rb.useGravity = true;
            animationController.HandleMidairAnimation(rb.velocity.y);
            if (animationController.IsOnFloor)
            {
                animationController.TouchFloor(false);
            }
        }
        else
        {
            if (rb.useGravity)
            {
                rb.useGravity = false;
                animationController.HandleMidairAnimation(0f);
            }
            if (!animationController.IsOnFloor)
            {
                animationController.TouchFloor(true);
            }
            animationController.HandleGroundedAnimation(rotationData,
            Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }

    private void GroundedFixedUpdate()
    {
        HandleFixedGroundedInput();
    }

    private void SetGroundedDirection(Transform lookTowards)
    {
        Vector3 lookDirection = lookTowards.position - transform.position;
        Vector3 flattenedLook = lookDirection - Vector3.Dot(lookDirection, Vector3.up) * Vector3.up;
        twistPivot.rotation = Quaternion.LookRotation(flattenedLook, Vector3.up);
    }

    private void HandleGroundedInput()
    {
        float jumpAmount = Input.GetAxis("Jump");

        if (Mathf.Abs(jumpAmount) > 0.001f && !jumpKeyHeld)
        {
            if (IsStanding())
            {
                rb.velocity -= Vector3.Dot(rb.velocity, twistPivot.up) * twistPivot.up;
                rb.velocity += twistPivot.up * 7f;
            }
            jumpKeyHeld = true;
        }
        else if (Mathf.Abs(jumpAmount) <= 0.001f)
        {
            jumpKeyHeld = false;
        }
    }

    // Let's hope this doesn't break anything in the future. uwu
    // To fix: this actually broke something now that I implemented grappling. 
    private void HandleFixedGroundedInput()
    {
        float forwardAmount = Input.GetAxis("Vertical");
        float sideAmount = Input.GetAxis("Horizontal");
        if (Mathf.Abs(forwardAmount) > 0.001f)
        {
            transform.position += twistPivot.forward * forwardAmount * Time.fixedDeltaTime * moveSpeed / 200f;
        }
        if (Mathf.Abs(sideAmount) > 0.001f)
        {
            transform.position += twistPivot.right * sideAmount * Time.fixedDeltaTime * moveSpeed / 200f;
        }

        if (IsStanding())
        {
            Vector3 temp = Vector3.Dot(rb.velocity, twistPivot.up) * twistPivot.up;
            rb.velocity -= temp;
            rb.velocity *= Mathf.Pow(0.25f, Time.fixedDeltaTime);
            rb.velocity += temp;
        }
    }

    private Vector3 GetRotationData(Transform origin, Transform lookTowards)
    {
        Quaternion forwardVectorCorrection =
            Quaternion.FromToRotation(origin.forward, Vector3.forward);
        Quaternion upVectorCorrection =
            Quaternion.FromToRotation(forwardVectorCorrection * origin.up,
            Vector3.up);
        Vector3 offset = (upVectorCorrection * forwardVectorCorrection *
            (lookTowards.position - origin.position)).normalized;
        Vector3 output = new Vector3(Mathf.Acos(Mathf.Clamp(offset.z
            / Mathf.Sqrt(1 - Mathf.Pow(offset.y, 2)), -0.9999f, 0.9999f))
            * 180f / Mathf.PI, Mathf.Asin(Mathf.Clamp(
                offset.y, -0.9999f, 0.9999f)) * 180f / Mathf.PI);
        if (offset.x < 0)
        {
            output.x *= -1;
        }
        if (output.x < -180)
        {
            output.x += 360;
        }
        else if (output.x > 180)
        {
            output.x -= 360;
        }
        if (output.y < -180)
        {
            output.y += 360;
        }
        else if (output.y > 180)
        {
            output.y -= 360;
        }
        output.x = Mathf.Clamp(output.x, -90f, 90f);
        output.y = Mathf.Clamp(output.y, -90f, 90f);
        return output;
    }

    public bool IsStanding()
    {
        return Physics.Raycast(twistPivot.position,
            -Vector3.up, 0.2f);
    }
}
