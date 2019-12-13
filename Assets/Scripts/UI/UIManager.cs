using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class UIManager : MonoBehaviourPun, IOnEventCallback
{
    [SerializeField]
    private GameObject controlPanel;
    [SerializeField]
    private Slider cameraSlider;
    [SerializeField]
    private TextMeshProUGUI showUI, translucency, yaxis, cameramode, variableui, scoreboard;
    [SerializeField]
    private TextMeshProUGUI groundedInstructions, flyingInstructions, deathtext;
    [SerializeField]
    private CanvasGroup generalDisplay;
    [SerializeField]
    private Image cooldown;
    private bool generalDisplayEnabled = true;
    private bool coroutinePlaying = false;
    private float revivalTime = 0f;
    private MovementController trackedObject;

    private int score;

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == GameEvents.eventKillPlayer)
        {
            revivalTime = 10f;
        }
    }

    public void TrackObject(GameObject gameObject)
    {
        trackedObject = gameObject.GetComponent<MovementController>();
    }

    public void ToggleUIDisplay()
    {
        generalDisplayEnabled = !generalDisplayEnabled;
        if (!coroutinePlaying)
        {
            coroutinePlaying = true;
            StartCoroutine(toggleUIDisplay());
        }
    }

    private IEnumerator toggleUIDisplay()
    {
        while (true)
        {
            if (generalDisplayEnabled)
            {
                if (generalDisplay.alpha + 2f * Time.deltaTime > 1f)
                {
                    generalDisplay.alpha = 1f;
                    coroutinePlaying = false;
                    yield break;
                }
                else
                {
                    generalDisplay.alpha += 2f * Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                if (generalDisplay.alpha - 2f * Time.deltaTime < 0f)
                {
                    generalDisplay.alpha = 0f;
                    coroutinePlaying = false;
                    yield break;
                }
                else
                {
                    generalDisplay.alpha -= 2f * Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    public void ToggleUIMenu()
    {
        controlPanel.SetActive(!controlPanel.activeSelf);
        if (controlPanel.activeSelf)
        {
            UpdateControlPanelElements();
        }
    }

    public void UpdateControlPanelElements()
    {
        cameraSlider.value = SettingsManager.instance.cameraDistance;
        showUI.text = generalDisplayEnabled ? "Visible" : "Hidden";
        variableui.text = "Camera Mode (F): ";
        cameramode.text = !SettingsManager.instance.fullRotationEnabled ? "Free Camera" : "Track Rotation";
        variableui.text += cameramode.text;

        variableui.text += "\nTranslucency (T): ";
        switch (SettingsManager.instance.translucencyEnabled)
        {
            case 0:
                translucency.text = "Disabled";
                break;
            case 1:
                translucency.text = "Enabled";
                break;
            case 2:
                translucency.text = "Always";
                break;
        }
        variableui.text += translucency.text;

        variableui.text += "\nY Axis (Y): ";
        yaxis.text = SettingsManager.instance.invertFlyingYAxis == 1 ? "Normal" : "Inverted";
        variableui.text += yaxis.text;
    }

    public void IncrementScore(int amount)
    {
        score += amount;
        scoreboard.text = "Score: " + score.ToString();
    }

    CameraController cameracontroller;
    private void Start()
    {
        cameracontroller = Camera.main.transform.root.GetComponent<CameraController>();
        UpdateControlPanelElements();
    }

    private void Update()
    {
        if (generalDisplayEnabled)
        {
            if (cameracontroller.IsGrounded)
            {
                if (flyingInstructions.alpha > 0f)
                {
                    if (flyingInstructions.alpha - Time.deltaTime * 2 < 0f)
                    {
                        flyingInstructions.alpha = 0f;
                    }
                    else
                    {
                        flyingInstructions.alpha -= Time.deltaTime * 2f;
                    }
                }
                else if (groundedInstructions.alpha < 1f)
                {
                    if (groundedInstructions.alpha + Time.deltaTime * 2 > 1f)
                    {
                        groundedInstructions.alpha = 1f;
                    }
                    else
                    {
                        groundedInstructions.alpha += Time.deltaTime * 2f;
                    }
                }
            }
            else
            {
                if (groundedInstructions.alpha > 0f)
                {
                    if (groundedInstructions.alpha - Time.deltaTime * 2 < 0f)
                    {
                        groundedInstructions.alpha = 0f;
                    }
                    else
                    {
                        groundedInstructions.alpha -= Time.deltaTime * 2f;
                    }
                }
                else if (flyingInstructions.alpha < 1f)
                {
                    if (flyingInstructions.alpha + Time.deltaTime * 2 > 1f)
                    {
                        flyingInstructions.alpha = 1f;
                    }
                    else
                    {
                        flyingInstructions.alpha += Time.deltaTime * 2f;
                    }
                }
            }
        }
        if (revivalTime > 0f)
        {
            if (!deathtext.gameObject.activeSelf)
            {
                deathtext.gameObject.SetActive(true);
            }
            deathtext.text = "You will revive in " + ((int)revivalTime + 1).ToString() + " seconds";
            revivalTime -= Time.deltaTime;
        }
        else
        {
            if (deathtext.gameObject.activeSelf)
            {
                deathtext.gameObject.SetActive(false);
            }
        }
        if (trackedObject.isActiveAndEnabled)
        {
            cooldown.fillAmount = (5f - trackedObject.CurrentProjectileCooldown) / 5f;
            if (cooldown.fillAmount == 1f)
            {
                cooldown.color = Color.yellow;
            }
            else
            {
                cooldown.color = Color.white;
            }
        }
    }
}
