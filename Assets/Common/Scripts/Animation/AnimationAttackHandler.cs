using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAttackHandler : MonoBehaviour
{
    private Animator animator;
    private AnimatorOverrideController overrideController;

    private AnimationUpdater currentUpdater;
    [SerializeField]
    private RuntimeAnimatorController defaultController;
    public AnimationUpdater test;
    public AnimationUpdater test2;

    public void UpdateAnimations(AnimationUpdater updater)
    {
        // To do: reset everything first
        if (currentUpdater != null)
        {
            foreach (AnimationData d in currentUpdater.animationData)
            {
                GameObject g;
                if (d.parent != "")
                {
                    g = transform.Find(d.parent + "/" + d.objectName).gameObject;
                }
                else
                {
                    g = transform.Find(d.objectName).gameObject;
                }
                Destroy(g);
            }
        }
        currentUpdater = updater;

        foreach (AnimationData d in updater.animationData)
        {
            Transform parent = transform;
            if (d.parent != "")
            {
                parent = transform.Find(d.parent);
            }
            GameObject o = Instantiate(d.gameObject);
            o.transform.parent = parent;
            o.transform.localPosition = d.position;
            o.transform.localRotation = Quaternion.Euler(d.rotation);
            // To do: implement scale feature
            o.transform.localScale = Vector3.one;
            o.name = d.objectName;
        }
        overrideController = new AnimatorOverrideController(defaultController);
        foreach (OverridePair p in updater.overridePair)
        {
            overrideController[p.name] = p.clip;
        }
        animator.runtimeAnimatorController = overrideController;

    }

    public void CallEvent(string name)
    {
        foreach (FunctionCalls f in currentUpdater.functionCalls)
        {
            if (f.name == name)
            {
                f.function.Invoke(gameObject);
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        UpdateAnimations(test);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateAnimations(test2);
        }
    }
}
