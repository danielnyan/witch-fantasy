using UnityEngine;
using UnityEditor;

public class ViewSkeletonAdder : MonoBehaviour
{
    public void AddSkeleton()
    {
        if (!Application.isPlaying)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.GetComponent<ViewSkeleton>() == null)
                {
                    child.gameObject.AddComponent<ViewSkeleton>();
                }
                child.GetComponent<ViewSkeleton>().rootNode = GetComponent<ViewSkeleton>().rootNode;
            }
        }
    }

    public void RemoveSkeleton()
    {
        if (!Application.isPlaying)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.GetComponent<ViewSkeleton>() != null)
                {
                    DestroyImmediate(child.GetComponent<ViewSkeleton>());
                }
            }
        }
    }

    public void EditLayer()
    {
        if (!Application.isPlaying)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Bone");
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ViewSkeletonAdder))]
public class ViewSkeletonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ViewSkeletonAdder myScript = (ViewSkeletonAdder)target;
        if (GUILayout.Button("Populate Skeleton Gizmos"))
        {
            myScript.AddSkeleton();
        }
        if (GUILayout.Button("Remove Skeleton Gizmos"))
        {
            myScript.RemoveSkeleton();
        }
        if (GUILayout.Button("Change Layers To Bone"))
        {
            myScript.EditLayer();
        }
    }
}
#endif
