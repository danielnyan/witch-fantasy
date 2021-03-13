// Modified from https://answers.unity.com/questions/8201/how-to-make-bones-visible-in-editor-mode-.html

using UnityEngine;

public class ViewSkeleton : MonoBehaviour
{
    public Transform rootNode;

    void OnDrawGizmos()
    {
        if (rootNode != null)
        {
            if (rootNode == transform)
            {
                //list includes the root, if root then larger, green cube
                Gizmos.color = Color.green;
                Gizmos.DrawCube(transform.position, new Vector3(.1f, .1f, .1f));
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.parent.position);
                Gizmos.DrawCube(transform.position, new Vector3(.01f, .01f, .01f));
            }
        }
    }
}