using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    #region Public Settings
    public static SettingsManager instance;
    public bool menuActive = false;
    public int translucencyEnabled = 0;
    public float cameraDistance = 2.5f;
    public int invertFlyingYAxis = 1;
    #endregion

    #region Delegated Public Settings
    // This is adjusted in CameraController
    public bool fullRotationEnabled = false;
    #endregion

    #region Runtime Variables
    private UIManager uimanager;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        uimanager = transform.Find("UI").GetComponent<UIManager>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTranslucency();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ToggleFlyingYAxis();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleCameraRotation();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUIDisplay();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleMenu();
        }
        if (!menuActive)
        {
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
    }
    #endregion

    #region Settings Adjusting Methods
    public void ToggleTranslucency()
    {
        instance.translucencyEnabled++;
        if (instance.translucencyEnabled > 2)
        {
            instance.translucencyEnabled = 0;
        }
        uimanager.UpdateControlPanelElements();
    }

    public void ToggleFlyingYAxis()
    {
        instance.invertFlyingYAxis *= -1;
        uimanager.UpdateControlPanelElements();
    }

    public void ToggleCameraRotation()
    {
        Camera.main.transform.root.GetComponent<CameraController>().ToggleCameraRotate();
        uimanager.UpdateControlPanelElements();
    }

    public void ToggleMenu()
    {
        if (!instance.menuActive)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        instance.menuActive = !instance.menuActive;
        uimanager.ToggleUIMenu();
    }

    public void ToggleUIDisplay()
    {
        uimanager.ToggleUIDisplay();
    }

    public void SetCameraDistance(float value)
    {
        instance.cameraDistance = value;
    }
    #endregion
}
