using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnActionStarted;
    public static event EventHandler OnActionCompleted;
    
    protected Unit unit;
    protected bool isActionActive;
    protected Action onActionComplete;
    [SerializeField] protected float actionProbability = 2f;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract string GetActionName();

    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    public bool IsValidActionPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetPositionsList();
        return validGridPositionList.Contains(gridPosition);
    }

    public abstract List<GridPosition> GetPositionsList(int offset = 0);

    public int GetActionPointsCost()
    {
        return 1;
    }

    protected void ActionStart(Action onActionComplete)
    {
        isActionActive = true;
        this.onActionComplete = onActionComplete;
        
        OnActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        isActionActive = false;
        onActionComplete();
        
        OnActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit()
    {
        return unit;
    }

    public float GetActionProbability()
    {
        return actionProbability;
    }

    public void SetActionProbability(float actionProbabilityChange)
    {
        actionProbability += actionProbabilityChange;
    }

    public AIAction GetHighestPriorityEnemyAIAction()
    {
        List<AIAction> enemyAIActionList = new List<AIAction>();

        List<GridPosition> validActionPositions = GetPositionsList();

        foreach (var gridPosition in validActionPositions)
        {
            AIAction aiAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(aiAction);
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((AIAction a, AIAction b) => b.actionPriority - a.actionPriority);
            return enemyAIActionList[0];
        }
        return null;
    }

    public abstract AIAction GetEnemyAIAction(GridPosition gridPosition);
}


public class AIAction
{
    public GridPosition gridPosition;
    public int actionPriority;
}
