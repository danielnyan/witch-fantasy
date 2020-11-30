using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AnimationData
{
    public GameObject gameObject;
    public string objectName;
    public string parent;
    public Vector3 position;
    public Vector3 rotation;
}

[Serializable]
public struct OverridePair
{
    public string name;
    public AnimationClip clip;
}

[Serializable]
public struct FunctionCalls
{
    public string name;
    public AnimationHandlerEvent function;
}

[CreateAssetMenu(fileName ="Animation Updater", menuName ="ScriptableObject/AnimationUpdater")]
public class AnimationUpdater : ScriptableObject
{
    public AnimationData[] animationData;
    public OverridePair[] overridePair;
    public FunctionCalls[] functionCalls;
}
