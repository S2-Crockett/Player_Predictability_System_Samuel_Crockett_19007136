using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Rendering;
using UnityEngine;

public class MeleeActionProbability : ActionProbabilityBase
{
    public static MeleeActionProbability Instance { get; private set; }

    private MoveAction moveAction;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one MeleeActionProbability " + transform + " - " + Instance);
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
            moveAction = unit.GetAction<MoveAction>();
            unit.GetAction<MeleeAction>().OnMeleeActionStarted += MeleeAction_OnMeleeActionStarted;
        }
    }

    public bool IsUnitInMeleeMoveRange(GridPosition gridPosition)
    {
        foreach (var unit in UnitManager.Instance.GetPlayerUnitList())
        {
            if (unit.TryGetComponent(out MoveAction moveAction))
            {
                if (moveAction.GetPositionsList().Contains(gridPosition))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void MeleeAction_OnMeleeActionStarted(object sender, EventArgs e)
    {
        if (UnitManager.Instance.GetEnemyInCombatUnitList().Count == 0) return;
        
        MeleeAction meleeAction = sender as MeleeAction;
        Unit unit = meleeAction.GetUnit();
        
        List<Vector3> posList = moveAction.GetPositionList();
        
        if (unit.GetPreviousAction().GetActionName() == moveAction.GetActionName())
        {
            float pathLength = 0f;
            GridPosition startGridPosition = LevelGrid.Instance.GetGridPosition(posList[0]);
            GridPosition endGridPosition = LevelGrid.Instance.GetGridPosition(posList[posList.Count - 1]);
            pathLength = Pathfinding.Instance.GetPathLength(startGridPosition, endGridPosition);
            actionProbability += pathLength/65;
        }
        else
        {
            actionProbability += 0.4f;
        }
    }


    public override string GetActionName()
    {
        return "Melee";
    }
}
