using Photon.Pun;
using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the character movement and attacking mechanics. Animation 
/// is handled in a separate script called AnimationController.
/// </summary>
[RequireComponent(typeof(AnimationController))]
public class MovementController : MonoBehaviourPun, IPunObservable
{
    // Possible improvement: move values out of MovementController and into a manager.
    #region Player Metadata
    [SerializeField]
    private float moveSpeed = 800f;
    [SerializeField]
    private float turnSpeed = 15f;
    private float baseProjectileCooldown = 5f;
    [SerializeField]
    private float projectileSpeed = 100f;
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private GameObject projectileEffect;
    #endregion

    #region Runtime Variables
    public float CurrentProjectileCooldown { get; private set; }
    private float yeetingAnimationCooldown = 0f;
    public bool FiringModeOn { get; private set; }
    private Vector3 rotationData = Vector3.zero;
    public bool IsGrounded { get; private set; }
    private bool jumpKeyHeld;
    private bool canSetup = false;
    private bool setupDone = false;
    private bool canFly = true;
    private float flyingDebugCooldown = 0f;
    #endregion

    #region Prefab Variables
    private Rigidbody rb;
    [SerializeField]
    private Transform twistPivot;
    [SerializeField]
    private Transform firingRotation;
    [SerializeField]
    private Transform firingPivot;
    [SerializeField]
    private Transform lookTowards;
    [SerializeField]
    private Collider groundCollider;
    private GroundedHandler groundedHandler;
    private GameObject projectileReady;
    private AnimationController animationController;
    [SerializeField]
    private GameObject broomParticles;
    [SerializeField]
    private Collider[] flyingColliders;
    #endregion

    #region IPunObservable Implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(FiringModeOn);
            stream.SendNext(rotationData);
            stream.SendNext(CurrentProjectileCooldown);
        }
        else
        {
            FiringModeOn = (bool)stream.ReceiveNext();
            rotationData = (Vector3)stream.ReceiveNext();
            CurrentProjectileCooldown = (float)stream.ReceiveNext();
        }
    }
    #endregion

    #region MonoBehaviour Callbacks
    private void Reset()
    {
        twistPivot = transform.Find("Animation Parent");
        firingRotation = transform.Find("Animation Parent/Firing Rotation");
        firingPivot = transform.Find("Animation Parent/Armature/Hip/Lower_Torso" +
            "/Upper_Torso/Shoulder_R/Upper_Arm_R/Lower_Arm_R/Palm_R/Firing Pivot");
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
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        int currentPhase = 0;
        while (true)
        {
            if (!canSetup)
            {
                yield return new WaitForEndOfFrame();
            }
            if (currentPhase == 0)
            {
                rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    currentPhase += 1;
                }
            }
            if (currentPhase == 1)
            {
                groundedHandler = GetComponentInChildren<GroundedHandler>(includeInactive: true);
                animationController = GetComponent<AnimationController>();
                if (groundedHandler == null || animationController == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    currentPhase += 1;
                }
            }
            if (currentPhase == 2)
            {
                if (projectileReady == null)
                {
                    projectileReady = Instantiate(projectileEffect, firingPivot);
                    projectileReady.SetActive(false);
                }

                rb.mass = 50f;
                rb.drag = 0.7f;
                rb.angularDrag = 2f;
                rb.maxAngularVelocity = 20f;

                currentPhase += 1;
            }

            if (currentPhase == 3)
            {
                if (Camera.main == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (photonView.IsMine || !PhotonNetwork.IsConnected)
                {
                    Camera.main.transform.root.GetComponent<CameraController>().TrackObject(transform);
                    lookTowards = Camera.main.transform.root.Find("Camera Look Direction");
                }
                if (lookTowards == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                currentPhase += 1;
            }
            if (currentPhase == 4)
            {
                if (SettingsManager.instance == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (SettingsManager.instance.transform.root.GetComponentInChildren<UIManager>() == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (photonView.IsMine || !PhotonNetwork.IsConnected)
                {
                    UIManager uiManager = SettingsManager.instance.transform.root.GetComponentInChildren<UIManager>();
                    uiManager.TrackObject(gameObject);
                }
                currentPhase += 1;
            }
            if (currentPhase == 5)
            {
                if (animationController.IsReady())
                {
                    if (groundedHandler.IsGrounded())
                    {
                        animationController.FlyingModeOn(false);
                    }
                    else
                    {
                        animationController.FlyingModeOn(true);
                    }
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            setupDone = true;
            yield break;
        }
    }

    private void Update()
    {
        if (!setupDone)
        {
            return;
        }

        if (IsGrounded)
        {
            GroundedUpdate();
        }
        else
        {
            FlyingUpdate();
        }

        // Add offline functionality
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            if (Input.GetMouseButton(1))
            {
                FiringModeOn = true;
            }
            else
            {
                FiringModeOn = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (CurrentProjectileCooldown <= 0f && FiringModeOn)
                {
                    Vector3 firePosition = firingPivot.position;
                    Vector3 fireRotation = firingRotation.forward;
                    Vector3 currVelocity = rb.velocity;
                    FireProjectile(firePosition, fireRotation, currVelocity);
                    CurrentProjectileCooldown = baseProjectileCooldown;
                }
            }
        }

        if (FiringModeOn)
        {
            firingRotation.localEulerAngles =
                new Vector3(-rotationData.y, rotationData.x, 0);
            animationController.FiringModeOn(true);
            if (CurrentProjectileCooldown <= 0f)
            {
                projectileReady.SetActive(true);
            }
            else
            {
                projectileReady.SetActive(false);
            }
        }
        else
        {
            if (yeetingAnimationCooldown <= 0f)
            {
                animationController.FiringModeOn(false);
            }
            projectileReady.SetActive(false);
        }
        
        HandleCooldowns();
    }

    private void FixedUpdate()
    {
        if (!setupDone)
        {
            return;
        }

        if (IsGrounded)
        {
            GroundedFixedUpdate();
        }
        else
        {
            FlyingFixedUpdate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!setupDone)
        {
            return;
        }

        if (!IsGrounded)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                photonView.RPC("FlyingToGrounded", RpcTarget.All);
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
            GameEvents.KillPlayer(photonView.ViewID);
        }
    }

    private void OnDisable()
    {
        canFly = true;
        IsGrounded = false;
        twistPivot.rotation = Quaternion.identity;
        firingRotation.rotation = Quaternion.identity;
        projectileReady.SetActive(false);
    }
    #endregion

    #region Update Functions
    // To do: when transitioning to grounded mode, gradually reset all twistPivot stuff and vice versa
    private void GroundedUpdate()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            SetGroundedDirection(lookTowards);
            rotationData = GetRotationData(twistPivot, lookTowards);
            HandleGroundedInput();
        }
        if (!groundedHandler.IsStanding())
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
            if (photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                animationController.HandleGroundedAnimation(rotationData,
                Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }
        }
    }

    private void GroundedFixedUpdate()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            HandleFixedGroundedInput();
        }
    }

    private void FlyingUpdate()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            HandleFlyingInput();
            rotationData = GetRotationData(twistPivot, lookTowards);
            animationController.HandleFlyingAnimation(rotationData);
        }
        if (groundedHandler.IsGrounded() || !canFly)
        {
            photonView.RPC("FlyingToGrounded", RpcTarget.All);
        }
    }

    private void FlyingFixedUpdate()
    {
        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            HandleFixedFlyingInput();
        }
        AddUpthrust();
    }
    #endregion

    #region Grounded Functions
    private void SetGroundedDirection(Transform lookTowards)
    {
        transform.up = Vector3.Slerp(transform.up, groundedHandler.GetGroundNormals(50f),
            0.1f).normalized;
        Vector3 lookDirection = lookTowards.position - transform.position;
        Vector3 flattenedLook = lookDirection - Vector3.Dot(lookDirection, transform.up)
            / Vector3.SqrMagnitude(transform.up) * transform.up;
        twistPivot.rotation = Quaternion.LookRotation(flattenedLook, transform.up);
    }

    private void HandleGroundedInput()
    {
        float jumpAmount = Input.GetAxis("Jump");

        // To add: if is not grounded and conditions below, fly. Implement no-fly zones by using on trigger stay, 
        // or perhaps add a "canFly" attribute which toggles when entering a zone. 
        if (Mathf.Abs(jumpAmount) > 0.001f && !jumpKeyHeld)
        {
            if (groundedHandler.IsStanding())
            {
                rb.velocity -= Vector3.Dot(rb.velocity, twistPivot.up) * twistPivot.up;
                rb.velocity += twistPivot.up * 7f;
            }
            else if (canFly)
            {
                photonView.RPC("GroundedToFlying", RpcTarget.All);
            }
            else if (flyingDebugCooldown < 0f)
            {
                canFly = CheckNoFlyZone();
                if (canFly)
                {
                    photonView.RPC("GroundedToFlying", RpcTarget.All);
                }
                flyingDebugCooldown = 1f;
            }
            jumpKeyHeld = true;
        }
        else if (Mathf.Abs(jumpAmount) <= 0.001f)
        {
            jumpKeyHeld = false;
        }
    }

    // Let's hope this doesn't break anything in the future. uwu
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
    }

    // Handles an edge case when OnTriggerEnter is called after OnTriggerExit due to race conditions, 
    // thus causing the player to be unable to fly despite not being in a No Fly Zone. 
    private bool CheckNoFlyZone()
    {
        int eventLayer = LayerMask.NameToLayer("EventZone");
        RaycastHit[] hitColliders = Physics.RaycastAll(transform.position, transform.up * 0.2f, 1 << eventLayer);
        foreach (RaycastHit hit in hitColliders)
        {
            if (hit.transform.CompareTag("No Fly Zone"))
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Flying Functions
    private void HandleFixedFlyingInput()
    {
        float pitchAmount = Input.GetAxis("Vertical");
        float thrustAmount = Input.GetAxis("Jump");
        float yawAmount = Input.GetAxis("Yaw");

        // Thrust
        rb.AddForce(transform.forward * thrustAmount * moveSpeed);

        // Pitch
        rb.AddTorque(SettingsManager.instance.invertFlyingYAxis * transform.right * pitchAmount * turnSpeed);
        rb.AddForce(transform.up * pitchAmount * moveSpeed);

        // Yaw
        rb.AddTorque(transform.up * yawAmount * turnSpeed / 5f);
    }

    private void HandleFlyingInput()
    {
        float rollAmount = Input.GetAxis("Horizontal");

        // Roll
        transform.Rotate(-transform.forward, rollAmount *
            Time.deltaTime * turnSpeed * 5f, Space.World);
    }

    private void AddUpthrust()
    {
        rb.AddForce(transform.up * rb.velocity.magnitude * transform.up.y * 25f);
    }
    #endregion

    #region Universal Functions
    private void HandleCooldowns()
    {
        if (CurrentProjectileCooldown > 0)
        {
            CurrentProjectileCooldown -= Time.deltaTime;
        }
        if (yeetingAnimationCooldown > 0)
        {
            yeetingAnimationCooldown -= Time.deltaTime;
        }
        if (flyingDebugCooldown > 0)
        {
            flyingDebugCooldown -= Time.deltaTime;
        }
    }

    // Possible improvements: remove rotationData, parent firingPivot to hand, 
    // direction of fire dictated by firing rotation which is adjusted by rotation data. 
    // Or even better, use twistPivot.forward directly by multiplying twistPivot.rotation 
    // with Quaternion.Euler(-rotationData.y, rotationData.x, 0)
    // Remember to also update the boy prefab once you're done
    private void FireProjectile(Vector3 firePosition, Vector3 fireDirection, Vector3 currVelocity)
    {
        GameEvents.FireProjectile(PhotonNetwork.LocalPlayer.ActorNumber,
            photonView.ViewID, firePosition, fireDirection, currVelocity);
        animationController.Yeet();
        yeetingAnimationCooldown = 0.5f;
    }

    // Returns rotation data with respect to animation shape key data. 
    // As such, X is Y-rotation (called X because it moves horizontally), 
    // and -Y gives the X-rotation. The X and Y values are also clamped 
    // within the range (-90, 90) in degrees. 
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

    [PunRPC]
    private void GroundedToFlying()
    {
        if (!setupDone)
        {
            return;
        }
        if (IsGrounded)
        {
            rb.drag = 0.7f;
            rb.freezeRotation = false;
            groundCollider.enabled = false;
            animationController.FlyingModeOn(true);
            broomParticles.SetActive(true);
            foreach (Collider c in flyingColliders)
            {
                c.enabled = true;
            }
            transform.rotation = twistPivot.rotation;
            twistPivot.localRotation = Quaternion.identity;
            IsGrounded = false;
        }
    }

    [PunRPC]
    private void FlyingToGrounded()
    {
        if (!setupDone)
        {
            return;
        }
        if (!IsGrounded)
        {
            rb.drag = 0.1f;
            rb.freezeRotation = true;
            groundCollider.enabled = true;
            animationController.FlyingModeOn(false);
            broomParticles.SetActive(false);
            foreach (Collider c in flyingColliders)
            {
                c.enabled = false;
            }
            transform.rotation = twistPivot.rotation;
            twistPivot.localRotation = Quaternion.identity;
            IsGrounded = true;
            rb.velocity = Vector3.zero;
        }
    }
    #endregion

    #region Public Methods
    public Vector3 GetFiringPivotPosition()
    {
        return firingPivot.position;
    }

    public void EnableCharacter()
    {
        canSetup = true;
    }
    #endregion
}