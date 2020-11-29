using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAttackHandler : MonoBehaviour
{
    private Animator animator;
    private AnimatorOverrideController overrideController;

    private AnimationUpdater currentUpdater;
    private AnimationHandlerEvent eventListener;
    [SerializeField]
    private RuntimeAnimatorController defaultController;
    public AnimationUpdater test;

    public void UpdateAnimations(AnimationUpdater updater)
    {
        // To do: reset everything first
        eventListener.RemoveAllListeners();
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
        overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = defaultController;
        foreach (OverridePair p in updater.overridePair)
        {
            overrideController[p.name] = p.clip;
        }
        animator.runtimeAnimatorController = overrideController;
    }

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        eventListener = new AnimationHandlerEvent();
        UpdateAnimations(test);
    }
}
