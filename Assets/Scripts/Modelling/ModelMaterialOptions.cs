﻿using UnityEngine;
using UnityEditor;

public enum ModelMaterials
{
    Body, Broom, Stick, Clothes, ClothesAccent, EyeWhite, Hair, Mouth, Teeth, LeftEye, RightEye,
    Stocking, StockingTop, Shorts
}

/// <summary>
/// Contains options to manipulate model materials
/// </summary>
public class ModelMaterialOptions : MonoBehaviour
{
    [SerializeField]
    private GameObject[] parts;
    public Material body, broom, stick, clothes, clothesAccent, eyeWhite, hair, mouth, teeth, leftEye,
        rightEye, stocking, stockingTop, shorts, translucent;
    [SerializeField]
    private GameObject hairObject;
    public Mesh hairstyle;
    [SerializeField]
    private GameObject hatObject;
    public Vector3 hatLocalOffset;
    public ParticleSystem broomParticles;

    public void UpdateMaterials()
    {
        foreach (GameObject part in parts)
        {
            MaterialHandler m = part.GetComponent<MaterialHandler>();
            m.UpdateMaterials(this);
        }
        hairObject.GetComponent<MeshFilter>().mesh = hairstyle;
        hatObject.transform.localPosition = hatLocalOffset;
    }

    public void TranslucentMaterials()
    {
        foreach (GameObject part in parts)
        {
            MaterialHandler m = part.GetComponent<MaterialHandler>();
            m.TranslucentMaterials(this);
        }
    }

    // Assumes that order of ModelMaterials is as shown
    public Material Dispatcher(ModelMaterials material)
    {
        Material[] materials = {body, broom, stick, clothes, clothesAccent, eyeWhite, hair, mouth, teeth, leftEye,
        rightEye, stocking, stockingTop, shorts};
        return materials[(int)material];
    }

    private void OnEnable()
    {
        UpdateMaterials();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ModelMaterialOptions))]
public class ModelOptionsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ModelMaterialOptions myScript = (ModelMaterialOptions)target;
        if (GUILayout.Button("Update Materials"))
        {
            myScript.UpdateMaterials();
        }
        if (GUILayout.Button("Test Transparency"))
        {
            myScript.TranslucentMaterials();
        }
    }
}
#endif