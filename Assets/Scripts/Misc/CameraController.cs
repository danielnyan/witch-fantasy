using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Private Serializable Fields
    [SerializeField]
    private float cameraSpeed = 3f;
    [SerializeField]
    private Transform trackedPivot = null;
    [SerializeField]
    private bool smartRotationEnabled = true;
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
                    if (smartRotationEnabled)
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
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleSmartRotate();
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
            if (smartRotationEnabled)
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
                if (smartRotationEnabled)
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
            if (!smartRotationEnabled)
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
        smartRotationEnabled = !smartRotationEnabled;
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
    #endregion

    #region Public Methods
    public void TrackObject(Transform obj)
    {
        transform.position = obj.position;
        trackedPivot = obj;
        baseRotation = Quaternion.identity;
        rotationalOffset = Vector3.zero;
        controller = obj.GetComponent<MovementController>();
    }
    #endregion
}
