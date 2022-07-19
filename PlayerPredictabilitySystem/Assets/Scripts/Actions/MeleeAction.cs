using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAction : BaseAction
{
    public static event EventHandler OnAnyMeleeHit;
    public event EventHandler OnMeleeActionStarted;
    public event EventHandler OnMeleeActionCompleted;
    
    private enum State
    {
        SwingSwordBeforeHit,
        SwingSwordAfterHit
    }
    
    private int maxMeleeDistance = 1;
    
    private State state;
    private float stateTimer;
    private Unit targetUnit;
    

    private void Update()
    {
        if (!isActionActive) return;
        
        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.SwingSwordBeforeHit:
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                float rotateSpeed = 10f;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.SwingSwordAfterHit:

                break;
        }

        if (stateTimer <= 0)
        {
            NextState();
        }
    }
    
    private void NextState()
    {
        switch (state)
        {
            case State.SwingSwordBeforeHit:
                state = State.SwingSwordAfterHit;
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;
                
                targetUnit.Damage(100);
                OnAnyMeleeHit?.Invoke(this, EventArgs.Empty);
                
                break;
            case State.SwingSwordAfterHit:
                unit.SetPreviousAction(this);
                OnMeleeActionCompleted?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;
        }
    }

    public override string GetActionName()
    {
        return "Melee";
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtPosition(gridPosition);
        
        state = State.SwingSwordBeforeHit;
        float beforeHitStateTime = 0.7f;
        stateTimer = beforeHitStateTime;

        OnMeleeActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetPositionsList(int offset = 0)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxMeleeDistance; x <= maxMeleeDistance; x++)
        {
            for (int z = -maxMeleeDistance; z <= maxMeleeDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidActionPosition(testGridPosition)) continue;
                if (!LevelGrid.Instance.HasUnitAtPosition(testGridPosition)) continue;

                Unit targetUnit = LevelGrid.Instance.GetUnitAtPosition(testGridPosition);
                if(targetUnit.IsEnemy() == unit.IsEnemy()) continue;
                
                validGridPositionList.Add(testGridPosition);
            }
        }
        
        return validGridPositionList;
    }

    public override AIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new AIAction()
        {
            gridPosition = gridPosition,
            actionPriority = 200
        };
    }

    public int GetMaxMeleeDistance()
    {
        return maxMeleeDistance;
    }
}
