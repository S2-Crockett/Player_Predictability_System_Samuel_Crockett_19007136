using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private bool invert;
    
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (invert)
        {
            Vector3 directionToCamera = (cameraTransform.position - transform.position).normalized;
            transform.LookAt(transform.position + -directionToCamera);
        }
        else
        {
            transform.LookAt(cameraTransform);
        }
    }
}
