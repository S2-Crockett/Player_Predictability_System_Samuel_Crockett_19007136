using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwatchActionProbability : ActionProbabilityBase
{
    public static OverwatchActionProbability Instance { get; private set; }
    
    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one OverwatchActionProbability " + transform + " - " + Instance);
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
            unit.GetAction<OverwatchAction>().OnEnterOverwatch += OverwatchAction_OnEnterOverwatch;
        }
    }

    private void OverwatchAction_OnEnterOverwatch(object sender, OverwatchAction.OnOverwatchEventArgs e)
    {
        if (UnitManager.Instance.GetEnemyInCombatUnitList().Count == 0) return;
        actionProbability += 0.3f;
    }


    public override string GetActionName()
    {
        return "Overwatch";
    }
}
