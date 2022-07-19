using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public static event EventHandler OnAnyGrenadeExploded;

    private Vector3 targetPosition, positionXZ;
    private Action OnGrenadeBehaviourcomplete;
    private float totalDistance, numberOfUnitsAndCrates;
    private static float damageRadius = 3f;
    private Unit unit;

    [SerializeField] private Transform grenadeExplodeVfxPrefab;
    [SerializeField] private TrailRenderer grenadeTrailRenderer;
    [SerializeField] private AnimationCurve arcYAnimationCurve;

    private void Update()
    {
        Vector3 moveDirection = (targetPosition - positionXZ).normalized;
        
        float moveSpeed = 15f;
        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1 - distance / totalDistance;
        float maxHeight = totalDistance / 4f;
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;

        positionXZ += moveDirection * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);
        
        float reachedTargetPosition = 0.2f;
        if (Vector3.Distance(transform.position, targetPosition) < reachedTargetPosition)
        {
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);

            foreach (var collider in colliderArray)
            {
                if (collider.TryGetComponent(out Unit targetUnit))
                {
                    targetUnit.Damage(30);
                    if (targetUnit.IsEnemy()) numberOfUnitsAndCrates++;
                }
                if (collider.TryGetComponent(out DestructableCrate crate))
                {
                    crate.Damage();
                    numberOfUnitsAndCrates++;
                }
            }
            
            OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

            grenadeTrailRenderer.transform.parent = null;
            Instantiate(grenadeExplodeVfxPrefab, targetPosition + Vector3.up, Quaternion.identity);
            Destroy(gameObject);

            OnGrenadeBehaviourcomplete();
        }
    }

    public float GetNumberOfUnitsAndCrates()
    {
        return numberOfUnitsAndCrates;
    }

    public Unit GetUnit()
    {
        return unit;
    }

    public void Setup(GridPosition targetGridPosition, Action OnGrenadeBehaviourcomplete, Unit unit)
    {
        this.OnGrenadeBehaviourcomplete = OnGrenadeBehaviourcomplete;
        this.unit = unit;
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        positionXZ = transform.position;
        positionXZ.y = 0;
        
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }

    public static float GetGrenadeRadius()
    {
        return damageRadius;
    }
}
