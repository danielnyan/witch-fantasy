using Photon.Pun;
using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the character movement and attacking mechanics. Animation 
/// is handled in a separate script called AnimationController.
/// </summary>
[RequireComponent(typeof(AnimationController))]
public class MovementController : MonoBehaviour
{
    // Possible improvement: move values out of MovementController and into a manager.
    #region Player Metadata
    [SerializeField]
    public float moveSpeed = 800f;
    [SerializeField]
    public float turnSpeed = 15f;
    [SerializeField]
    public int invertFlyingYAxis = 1;
    #endregion

    #region Runtime Variables
    public Vector3 rotationData = Vector3.zero;
    public bool IsGrounded;
    public bool jumpKeyHeld;
    public bool canFly = true;
    public bool hasBroom = true;
    public float flyingDebugCooldown = 0f;
    public GameObject beaconInstance;
    #endregion

    #region Prefab Variables
    public Rigidbody rb;
    public Transform twistPivot;
    public Transform lookTowards;
    public Collider groundCollider;
    public GroundedHandler groundedHandler;
    public AnimationController animationController;
    public GameObject broomParticles;
    public Collider[] flyingColliders;
    public GameObject beacon;
    public Transform beaconPivot;
    public GameObject spirit;

    public MovementLogic flyingLogic;
    public MovementLogic groundedLogic;
    public MovementLogic glidingLogic;

    public MovementLogic currentLogic;
    #endregion

    #region MonoBehaviour Callbacks
    private void Reset()
    {
        twistPivot = transform.Find("Animation Parent");
        groundCollider = transform.Find("Animation Parent/Ground Components").
            GetComponent<Collider>();
        flyingColliders = new Collider[] {transform.Find("Animation Parent/Armature/Hip/" +
            "Lower_Torso/Upper_Torso/Neck/Head").GetComponent<Collider>(),
            transform.Find("Animation Parent/Armature/Hip/" +
            "Lower_Torso").GetComponent<Collider>()};
        broomParticles = transform.Find("Animation Parent/Model/Accessories/" +
            "Broom/Broom Particles").gameObject;
    }

    private void OnEnable()
    {
        Setup();
    }

    private void Setup()
    {
        rb = GetComponent<Rigidbody>();
        groundedHandler = GetComponentInChildren<GroundedHandler>(includeInactive: true);
        animationController = GetComponent<AnimationController>();
        rb.mass = 50f;
        rb.drag = 0.7f;
        rb.angularDrag = 2f;
        rb.maxAngularVelocity = 20f;
        Camera.main.transform.root.GetComponent<CameraController>().TrackObject(transform);
        lookTowards = Camera.main.transform.root.Find("Camera Look Direction");

        if (groundedHandler.IsGrounded())
        {
            // animationController.FlyingModeOn(false);
            currentLogic = groundedLogic;
        }
        else
        {
            // animationController.FlyingModeOn(true);
            currentLogic = flyingLogic;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            invertFlyingYAxis *= -1;
        }
        if (!groundedHandler.IsStanding() && hasBroom && 
            Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeLogic(glidingLogic);
        }
        else if (!groundedHandler.IsStanding() && hasBroom && 
            Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeLogic(flyingLogic);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeLogic(groundedLogic);
        }
        currentLogic.MoveUpdate(this);
    }

    private void FixedUpdate()
    {
        currentLogic.MoveFixedUpdate(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsGrounded)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                FlyingToGrounded();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("No Fly Zone"))
        {
            canFly = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("No Fly Zone"))
        {
            canFly = true;
        }
        else if (other.CompareTag("Kill Zone"))
        {
            Destroy(transform.root.gameObject);
        }
    }

    private void OnDisable()
    {
        canFly = true;
        IsGrounded = false;
        twistPivot.rotation = Quaternion.identity;
    }
    #endregion

    #region Universal Functions
    // Returns rotation data with respect to animation shape key data. 
    // As such, X is Y-rotation (called X because it moves horizontally), 
    // and -Y gives the X-rotation. The X and Y values are also clamped 
    // within the range (-90, 90) in degrees. 
    public static Vector3 GetRotationData(Transform origin, Transform lookTowards)
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

    public void GroundedToFlying()
    {
        if (IsGrounded)
        {
            ChangeLogic(glidingLogic);
        }
    }

    public void FlyingToGrounded()
    {
        if (!IsGrounded)
        {
            ChangeLogic(groundedLogic);
        }
    }

    public void ChangeLogic(MovementLogic newLogic)
    {
        currentLogic.Cleanup(this);
        currentLogic = newLogic;
        currentLogic.Initialize(this);
    }
    #endregion
}