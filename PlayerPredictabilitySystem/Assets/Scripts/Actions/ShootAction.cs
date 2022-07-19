using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public class ShootAction : BaseAction
{
    public static event EventHandler<OnShootEventArgs> OnAnyShoot;
    public event EventHandler<OnShootEventArgs> OnShoot;

    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public bool hit;
        public float hitPercentage;
    }
    
    private enum State
    {
        Aiming,
        Shooting,
        Finish
    }

    [SerializeField] private LayerMask obstaclesLayerMask;

    private State state;
    private int maxShootDistnace = 7;
    private float stateTimer, maxAccuracy = 0.8f;
    private Unit targetUnit;
    private bool canShootBullet;
    
    private void Update()
    {
        if (!isActionActive) return;
        
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aiming:
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotateSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Finish:
                break;
        }

        if (stateTimer <= 0)
        {
            NextState();
        }
    }

    private void Shoot()
    {
        float hitPercentage = CalculateHitPercentage(targetUnit);
        float randomChance = UnityEngine.Random.Range(0, 1f);
        bool hit = randomChance < hitPercentage;

        OnAnyShoot?.Invoke(this, new OnShootEventArgs()
        {
            targetUnit = targetUnit,
        });
        OnShoot?.Invoke(this, new OnShootEventArgs()
        {
            targetUnit = targetUnit,
            hit = hit,
            hitPercentage = hitPercentage
        });

        if (hit) targetUnit.Damage(40);
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Finish;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Finish:
                unit.SetPreviousAction(this);
                ActionComplete();
                break;
        }
    }
    
    public override string GetActionName()
    {
        return "Shoot";
    }

    public float CalculateHitPercentage(Unit shootUnit)
    {
        float hitPercentage = maxAccuracy;

        float shootDistance = CalculateShootDistance(shootUnit.GetGridPosition());
        float fullAcuracyShootDistance = 3f;
        float remainingShootDistance = Mathf.Max(0, shootDistance - fullAcuracyShootDistance);
        hitPercentage -= 0.05f * remainingShootDistance;

        switch (shootUnit.GetCoverType())
        {
            case CoverType.Full:
                if(!unit.GetIsFlanked()) hitPercentage -= 0.3f;
                break;
            case CoverType.Half:
                if(!unit.GetIsFlanked()) hitPercentage -= 0.1f;
                break;
        }

        return hitPercentage;
    }

    private float CalculateShootDistance(GridPosition gridPosition)
    {
        GridPosition currentGridPosition = unit.GetGridPosition();
        Vector2 shootVector = new Vector2(gridPosition.x - currentGridPosition.x, 
                                          gridPosition.z - currentGridPosition.z);

        float shootDistance = Mathf.Abs(shootVector.x) + Mathf.Abs(shootVector.y);
        return shootDistance;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtPosition(gridPosition);

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;

        canShootBullet = true;      
        
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetPositionsList(int offset = 0)
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetActionPositionList(unitGridPosition);
    }

    public List<GridPosition> GetActionPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxShootDistnace; x <= maxShootDistnace; x++)
        {
            for (int z = -maxShootDistnace; z <= maxShootDistnace; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidActionPosition(testGridPosition)) continue;

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxShootDistnace) continue;

                if (!LevelGrid.Instance.HasUnitAtPosition(testGridPosition)) continue;

                Unit targetUnit = LevelGrid.Instance.GetUnitAtPosition(testGridPosition);
                if(targetUnit.IsEnemy() == unit.IsEnemy()) continue;

                float unitShoulderHeight = 1.7f;
                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                
                if(Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight, shootDirection,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstaclesLayerMask)) continue;

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public int GetMaxShootDistance()
    {
        return maxShootDistnace;
    }
    
    public override AIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtPosition(gridPosition);

        return new AIAction
        {
            gridPosition = gridPosition,
            actionPriority = 10 + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100f),
        };
    }
    
    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetActionPositionList(gridPosition).Count;
    }
}
