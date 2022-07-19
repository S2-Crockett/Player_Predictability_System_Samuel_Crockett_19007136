using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject actionCameraGameObject;

    private void Start()
    {
        BaseAction.OnActionStarted += BaseAction_OnActionStarted;
        BaseAction.OnActionCompleted += BaseAction_OnActionCompleted;
        
        HideActionCamera();
    }

    private void ShowActionCamera()
    {
        actionCameraGameObject.SetActive(true);
    }

    private void HideActionCamera()
    {
        actionCameraGameObject.SetActive(false);
    }

    private void BaseAction_OnActionStarted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                Unit shooterUnit = shootAction.GetUnit();
                Unit targetUnit = shootAction.GetTargetUnit();
                
                Vector3 cameracharacterHeight = Vector3.up * 1.7f;
                Vector3 shootDirection = (targetUnit.GetWorldPosition() - shooterUnit.GetWorldPosition()).normalized;

                float sholderOffsetAmount = 0.5f;
                Vector3 sholderOffset = Quaternion.Euler(0, 90, 0) * shootDirection * sholderOffsetAmount;

                Vector3 actionCameraPosition = shooterUnit.GetWorldPosition() + cameracharacterHeight + sholderOffset + -shootDirection;
                
                actionCameraGameObject.transform.position = actionCameraPosition;
                actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameracharacterHeight);
                
                ShowActionCamera();
                break;
        }
    }

    private void BaseAction_OnActionCompleted(object sender, EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                HideActionCamera();
                break;
        } 
    }
}
