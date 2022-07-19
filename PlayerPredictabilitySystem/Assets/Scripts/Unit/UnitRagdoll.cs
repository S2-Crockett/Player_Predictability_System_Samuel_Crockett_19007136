using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollRootBone;

    public void Setup(Transform originalRootBone)
    {
        SetChildTransforms(originalRootBone, ragdollRootBone);
    }

    private void SetChildTransforms(Transform root, Transform clone)
    {
        foreach (Transform child in root)
        {
            Transform cloneChild = clone.Find(child.name);
            if (cloneChild)
            {
                cloneChild.position = child.position;
                cloneChild.rotation = child.rotation;
                SetChildTransforms(child, cloneChild);
            }
        }
    }
}
