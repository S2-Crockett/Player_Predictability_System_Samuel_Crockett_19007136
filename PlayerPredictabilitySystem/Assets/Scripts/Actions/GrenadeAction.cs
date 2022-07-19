using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction
{
    public event EventHandler OnGrenadeThrown;
    public event EventHandler OnGrenadeExploded;
    
    [SerializeField] private Transform grenadeProjecileprefab;
    [SerializeField] private LayerMask obstaclesLayerMask;
    
    private int maxThrowDistance = 5;
    private bool hasPreformed;

    public override string GetActionName()
    {
        return "Grenade";
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Transform grenadeProjectileTransform = Instantiate(grenadeProjecileprefab, unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeBehaviourComplete, unit);
        
        OnGrenadeThrown?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }

    private void OnGrenadeBehaviourComplete()
    {
        unit.SetPreviousAction(this);
        OnGrenadeExploded?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }
    
    public override List<GridPosition> GetPositionsList(int offset = 0)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxThrowDistance; x <= maxThrowDistance; x++)
        {
            for (int z = -maxThrowDistance; z <= maxThrowDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidActionPosition(testGridPosition)) continue;

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxThrowDistance) continue;
                
                if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Cover" && unit.IsEnemy())
                {
                    if (!IsPlayerInRangeOfGrenade(testGridPosition)) continue;
                }
                
                float unitShoulderHeight = 1.7f;
                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 testWorldPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);
                Vector3 shootDirection = (testWorldPosition - unit.GetWorldPosition()).normalized;

                if(Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight, shootDirection,
                       Vector3.Distance(unitWorldPosition,testWorldPosition),
                       obstaclesLayerMask)) continue;

                validGridPositionList.Add(testGridPosition);
            }
        }
        
        return validGridPositionList;
    }

    private bool IsPlayerInRangeOfGrenade(GridPosition gridPosition)
    {
        Unit unit = LevelGrid.Instance.GetUnitAtPosition(gridPosition);
        if (unit == null) return false;

        if (unit.GetCoverType() != CoverType.None && !unit.IsEnemy()) return true;

        return false;
    }

    public override AIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Cover" && !hasPreformed)
        {
            hasPreformed = true;
            return new AIAction()
            {
                gridPosition = gridPosition,
                actionPriority = 100,
            };
        }
        return new AIAction()
        {
            gridPosition = gridPosition,
            actionPriority = 5
        };
    }

    public void SetHasPreformed(bool hasMoved)
    {
        this.hasPreformed = hasMoved;
    }
}
