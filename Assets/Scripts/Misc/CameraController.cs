using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Private Serializable Fields
    [SerializeField]
    private float cameraSpeed = 3f;
    [SerializeField]
    private Transform trackedPivot = null;
    [SerializeField]
    private bool fullRotationEnabled = false;
    [SerializeField]
    private bool translucencyEnabled = false;
    [SerializeField]
    private float smoothTime = 0.1f;
    [SerializeField]
    private MovementController controller;
    [SerializeField]
    private float cameraDistance = 2.5f;
    #endregion

    #region Private Runtime Variables
    private Vector3 rotationalOffset = Vector3.zero;
    private Quaternion baseRotation = Quaternion.identity;
    private bool isGrounded;
    private bool isTranslucent;
    private float translucencyTime = 0f;
    private int layermask;
    #endregion

    #region MonoBehaviour Callbacks
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (trackedPivot != null && trackedPivot.gameObject.activeSelf)
        {
            if (controller.IsGrounded)
            {
                GroundedCameraProcedure();
                if (!isGrounded)
                {
                    isGrounded = true;
                }
            }
            else
            {
                FlyingCameraProcedure();
                if (isGrounded)
                {
                    isGrounded = false;
                    if (fullRotationEnabled)
                    {
                        rotationalOffset = Vector3.zero;
                        baseRotation = Quaternion.identity;
                    }
                    else
                    {
                        rotationalOffset = transform.rotation.eulerAngles;
                        rotationalOffset.z = 0;
                        baseRotation = Quaternion.identity;
                    }
                }
            }
            if (translucencyEnabled)
            {
                if (translucencyTime > 0f)
                {
                    if (!isTranslucent)
                    {
                        isTranslucent = true;
                        trackedPivot.gameObject.
                            GetComponentInChildren<ModelMaterialOptions>().TranslucentMaterials();
                    }
                }
                else
                {
                    if (isTranslucent)
                    {
                        isTranslucent = false;
                        trackedPivot.gameObject.
                            GetComponentInChildren<ModelMaterialOptions>().UpdateMaterials();
                    }
                }
            }

            if (CheckSight())
            {
                translucencyTime = 2f;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleSmartRotate();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTranslucency();
        }
        if (cameraDistance < 7.5f)
        {
            if (Input.mouseScrollDelta.y < 0)
            {
                cameraDistance -= Input.mouseScrollDelta.y;
            }
            cameraDistance += Input.GetKey(KeyCode.Minus) ? Time.deltaTime * 10 : 0;
        }
        if (cameraDistance >= 7.5f)
        {
            cameraDistance = 7.5f;
        }
        if (cameraDistance > 2.5f)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                cameraDistance -= Input.mouseScrollDelta.y;
            }
            cameraDistance += Input.GetKey(KeyCode.Equals) ? -Time.deltaTime * 10 : 0;
        }
        if (cameraDistance <= 2.5f)
        {
            cameraDistance = 2.5f;
        }
        if (translucencyTime > 0f)
        {
            translucencyTime -= Time.deltaTime;
        }
    }
    #endregion

    #region Update Procedures
    private void GroundedCameraProcedure()
    {
        float smoothingFactor = 1f - Mathf.Pow(0.1f, Time.deltaTime / (smoothTime));
        if (trackedPivot != null)
        {
            transform.position = trackedPivot.position;
            baseRotation = trackedPivot.rotation;
        }

        if (controller != null)
        {
            Vector3 target = new Vector3(0, 0, -cameraDistance);
            if (controller.FiringModeOn)
            {
                target += transform.InverseTransformPoint
                    (controller.GetFiringPivotPosition());
            }
            Camera.main.transform.localPosition = Vector3.Lerp(
                Camera.main.transform.localPosition, target, smoothingFactor);
        }

        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            AdjustCameraOffset();
        }
        if (rotationalOffset.x > 85f)
        {
            rotationalOffset.x = 85f;
        }
        else if (rotationalOffset.x < -85f)
        {
            rotationalOffset.x = -85f;
        }

        Quaternion targetRotation = baseRotation * Quaternion.Euler(rotationalOffset.x, rotationalOffset.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothingFactor);
    }

    private void FlyingCameraProcedure()
    {
        float smoothingFactor = 1f - Mathf.Pow(0.1f, Time.deltaTime / (smoothTime));
        if (trackedPivot != null)
        {
            transform.position = trackedPivot.position;
            if (fullRotationEnabled)
            {
                baseRotation = trackedPivot.rotation;
            }
        }

        if (controller != null)
        {
            Vector3 target = new Vector3(0, 0, -cameraDistance);
            if (controller.FiringModeOn)
            {
                target += transform.InverseTransformPoint
                    (controller.GetFiringPivotPosition());
                if (fullRotationEnabled)
                {
                    ClampRotationOffset();
                }
            }
            Camera.main.transform.localPosition = Vector3.Lerp(
                Camera.main.transform.localPosition, target, smoothingFactor);
        }

        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            AdjustCameraOffset();
        }

        Quaternion targetRotation = baseRotation * Quaternion.Euler(rotationalOffset.x, rotationalOffset.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothingFactor);
    }
    #endregion

    #region Private Helper Methods
    private void AdjustCameraOffset()
    {
        float rotationX = cameraSpeed * Input.GetAxis("Mouse X");
        float rotationY = -cameraSpeed * Input.GetAxis("Mouse Y");
        rotationalOffset += new Vector3(rotationY, rotationX, 0);
        if (rotationalOffset.x > 180f)
        {
            rotationalOffset.x -= 360f;
        }
        else if (rotationalOffset.x < -180f)
        {
            rotationalOffset.x += 360f;
        }
        if (rotationalOffset.y > 180f)
        {
            rotationalOffset.y -= 360f;
        }
        else if (rotationalOffset.y < -180f)
        {
            rotationalOffset.y += 360f;
        }
    }

    private void ToggleSmartRotate()
    {
        if (!isGrounded)
        {
            if (!fullRotationEnabled)
            {
                rotationalOffset = Vector3.zero;
            }
            else
            {
                rotationalOffset = transform.rotation.eulerAngles;
                rotationalOffset.z = 0;
                baseRotation = Quaternion.identity;
            }
        }
        fullRotationEnabled = !fullRotationEnabled;
    }

    private void ToggleTranslucency()
    {
        translucencyEnabled = !translucencyEnabled;
    }

    private void ClampRotationOffset()
    {
        if (rotationalOffset.x > 90f)
        {
            rotationalOffset.x = 90f;
        }
        else if (rotationalOffset.x < -90f)
        {
            rotationalOffset.x = -90f;
        }
        if (rotationalOffset.y > 90f)
        {
            rotationalOffset.y = 90f;
        }
        else if (rotationalOffset.y < -90f)
        {
            rotationalOffset.y = -90f;
        }
    }

    private bool CheckSight()
    {
        return CheckSight(Camera.main.transform.position, 
            Camera.main.transform.forward, 100f, layermask);
    }

    // Queries the centre and another random point to make the player translucent using 
    // the power of statistics
    private bool CheckSight(Vector3 origin, Vector3 direction, float distance, int layermask)
    {
        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, distance, layermask))
        {
            return true;
        }
        Vector3 newDirection;
        for (int i = 0; i < 20; i++)
        {
            float magnitude = 0.2f * Random.value;
            float angle = 2 * Mathf.PI * Random.value;
            newDirection = direction + Quaternion.FromToRotation(Vector3.forward, direction)
                * (magnitude * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
            newDirection.Normalize();
            ray = new Ray(origin, newDirection);
            if (Physics.Raycast(ray, distance, layermask))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the layermask that the layer collides with
    /// </summary>
    private int GetLayermask(int layer)
    {
        int output = 0;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(layer, i))
            {
                output = output | 1 << i;
            }
        }
        return output;
    }
    #endregion

    #region Public Methods
    public void TrackObject(Transform obj)
    {
        transform.position = obj.position;
        trackedPivot = obj;
        baseRotation = Quaternion.identity;
        rotationalOffset = Vector3.zero;
        controller = obj.GetComponent<MovementController>();

        layermask = GetLayermask(obj.gameObject.layer);
        int eventZoneLayer = LayerMask.NameToLayer("Event Zone");
        layermask = layermask & ~(1 << eventZoneLayer);
    }
    #endregion
}
