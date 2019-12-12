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

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTranslucency();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            invertFlyingYAxis *= -1;
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
    private void ToggleTranslucency()
    {
        translucencyEnabled++;
        if (translucencyEnabled > 2)
        {
            translucencyEnabled = 0;
        }
    }
    #endregion
}
