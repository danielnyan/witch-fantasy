using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains options to manipulate model shape keys / emulate shape key drivers.
/// </summary>
[ExecuteAlways]
public class ModelShapekeyOptions : MonoBehaviour
{
    #region Shapekey variables
    [Range(-30f, 30f)]
    public float eyeAngleX = 0f;
    [Range(-30f, 30f)]
    public float eyeAngleY = 0f;
    [Range(0f, 1f)]
    public float mouthOpen = 0f;
    [Range(0f, 1f)]
    public float mouthShape = 1f;
    [Range(0f, 1f)]
    public float teethOpen = 1f;
    [Range(0f, 1f)]
    public float eyeOpenL = 1f;
    [Range(0f, 1f)]
    public float eyeOpenR = 1f;
    #endregion

    [SerializeField]
    private SkinnedMeshRenderer body, upperTeeth, lowerTeeth, leftEye, rightEye;
    [SerializeField]
    private Transform eyeballL, eyeballR, head;
    public bool trackEyeball = false;
    public Transform trackedObject;

    #region Helper methods
    private Transform FindDeepChild(string name)
    {
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(transform);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == name)
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        Debug.LogWarning("The child of name " + name + " cannot be found in " + this.name, this);
        return null;
    }
    #endregion

    #region MonoBehaviour Callbacks
    private void Reset()
    {
        body = FindDeepChild("Body").GetComponent<SkinnedMeshRenderer>();
        upperTeeth = FindDeepChild("Upper_Teeth").GetComponent<SkinnedMeshRenderer>();
        lowerTeeth = FindDeepChild("Lower_Teeth").GetComponent<SkinnedMeshRenderer>();
        leftEye = FindDeepChild("Left_Eye").GetComponent<SkinnedMeshRenderer>();
        rightEye = FindDeepChild("Right_Eye").GetComponent<SkinnedMeshRenderer>();
        eyeballL = FindDeepChild("Eyeball_L");
        eyeballR = FindDeepChild("Eyeball_R");
        head = FindDeepChild("Head");
    }

    private void Update()
    {
        if (trackEyeball)
        {
            if (trackedObject != null)
            {
                LookTowardsCurrentTarget();
            }
            else
            {
                trackEyeball = false;
            }
        }

        body.SetBlendShapeWeight(0, 100 * (1 - eyeOpenL));
        body.SetBlendShapeWeight(1, 100 * (1 - eyeOpenR));
        body.SetBlendShapeWeight(2, 100 * mouthOpen);
        body.SetBlendShapeWeight(3, 100 * (1 - mouthShape));
        lowerTeeth.SetBlendShapeWeight(0, 100 * (1 - teethOpen));
        lowerTeeth.SetBlendShapeWeight(1, 100 * (1 - Mathf.Clamp(mouthOpen * 2 - 1, 0, 1)));
        upperTeeth.SetBlendShapeWeight(0, 100 * (1 - teethOpen));
        upperTeeth.SetBlendShapeWeight(1, 100 * (1 - Mathf.Clamp(mouthOpen * 2 - 1, 0, 1)));
        leftEye.SetBlendShapeWeight(0, Mathf.Clamp(Mathf.Sin(eyeAngleX * Mathf.PI / 180f) * 200f, 0, 100));
        leftEye.SetBlendShapeWeight(1, Mathf.Clamp(Mathf.Sin(eyeAngleX * Mathf.PI / 180f) * -200f, 0, 100));
        leftEye.SetBlendShapeWeight(2, Mathf.Clamp(Mathf.Sin(eyeAngleY * Mathf.PI / 180f) * 200f, 0, 100));
        leftEye.SetBlendShapeWeight(3, Mathf.Clamp(Mathf.Sin(eyeAngleY * Mathf.PI / 180f) * -200f, 0, 100));
        rightEye.SetBlendShapeWeight(0, Mathf.Clamp(Mathf.Sin(eyeAngleX * Mathf.PI / 180f) * 200f, 0, 100));
        rightEye.SetBlendShapeWeight(1, Mathf.Clamp(Mathf.Sin(eyeAngleX * Mathf.PI / 180f) * -200f, 0, 100));
        rightEye.SetBlendShapeWeight(2, Mathf.Clamp(Mathf.Sin(eyeAngleY * Mathf.PI / 180f) * 200f, 0, 100));
        rightEye.SetBlendShapeWeight(3, Mathf.Clamp(Mathf.Sin(eyeAngleY * Mathf.PI / 180f) * -200f, 0, 100));
    }
    #endregion

    #region Private Methods
    // Adjusts the eye angle shapekeys according to the tracked object.
    private void LookTowardsCurrentTarget()
    {
        // In the model, the upwards direction of the head is malformed, so right points up.
        Vector3 headForward = head.up;
        Vector3 median = (eyeballL.position + eyeballR.position) / 2;
        Vector3 rotationData =
            Quaternion.FromToRotation(headForward, trackedObject.position - median).eulerAngles;
        eyeAngleX = -rotationData.y;
        eyeAngleY = -rotationData.x;
        if (eyeAngleX < -180) eyeAngleX += 360;
        if (eyeAngleY < -180) eyeAngleY += 360;
        eyeAngleX = Mathf.Clamp(eyeAngleX, -30, 30);
        eyeAngleY = Mathf.Clamp(eyeAngleY, -30, 30);
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Assigns a target for the model to look at. Set to null to de-assign.
    /// </summary>
    public void AssignLookTarget(Transform target)
    {
        trackedObject = target;
        if (target != null)
        {
            trackEyeball = true;
        }
    }
    #endregion
}
