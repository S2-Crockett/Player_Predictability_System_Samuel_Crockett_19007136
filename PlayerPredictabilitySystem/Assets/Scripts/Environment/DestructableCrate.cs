using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableCrate : MonoBehaviour
{

    public static event EventHandler OnAnyDestroyed;

    private GridPosition gridPosition;

    [SerializeField] private Transform cratedestroyedPrefab;

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }
    public void Damage()
    {
        Transform crateDestroyedTransform = Instantiate(cratedestroyedPrefab, transform.position, transform.rotation);
        ApplyExplosionToCrate(crateDestroyedTransform, 150f, transform.position, 10);
        Destroy(gameObject);
        OnAnyDestroyed?.Invoke(this, EventArgs.Empty);
        LevelGrid.Instance.SetCoverTypeAtGridPosition(GetGridPosition(), CoverType.None);
        LevelGrid.Instance.SetCoverObjectAtGridPosition(GetGridPosition(), null);
        foreach (var unit in UnitManager.Instance.GetUnitList())
        {
            unit.SetCoverType();
        }
    }
    
    private void ApplyExplosionToCrate(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform child in root)
        {
            if(child.TryGetComponent<Rigidbody>(out Rigidbody childRigidBody))
            {
                childRigidBody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
            }
            
            ApplyExplosionToCrate(child, explosionForce, explosionPosition, explosionRange);
        }
    }
}
