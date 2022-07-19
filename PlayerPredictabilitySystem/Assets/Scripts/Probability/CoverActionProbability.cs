using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverActionProbability : ActionProbabilityBase
{
    public static CoverActionProbability Instance { get; private set; }
    
    [SerializeField] private float pathLength;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one CoverActionProbability " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        
        if (!unit.IsEnemy())
        {
            unit.GetAction<MoveAction>().OnStopMoving += MoveAction_OnStopMoving;
        }
    }
    
    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        if (UnitManager.Instance.GetEnemyInCombatUnitList().Count == 0) return;
        
        MoveAction moveAction = sender as MoveAction;
        List<Vector3> posList = moveAction.GetPositionList();

        GridPosition startGridPosition = LevelGrid.Instance.GetGridPosition(posList[0]);
        GridPosition endGridPosition = LevelGrid.Instance.GetGridPosition(posList[posList.Count - 1]);
        
        Unit unit = LevelGrid.Instance.GetUnitAtPosition(LevelGrid.Instance.GetGridPosition(posList[posList.Count - 1]));

        if (unit.GetCoverType() != CoverType.None)
        {
            pathLength = (float)Pathfinding.Instance.GetPathLength(startGridPosition, endGridPosition);
            actionProbability += (pathLength * (float)unit.GetCoverType() / 4)/30;
        }
    }

    public override string GetActionName()
    {
        return "Cover";
    }
}
