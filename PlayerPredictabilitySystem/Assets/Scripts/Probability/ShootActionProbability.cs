using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootActionProbability : ActionProbabilityBase
{
    public static ShootActionProbability Instance { get; private set; }
    
    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one ShootActionProbability " + transform + " - " + Instance);
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
            unit.GetAction<ShootAction>().OnShoot += ShootAction_OnShoot;
        }
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        if (UnitManager.Instance.GetEnemyInCombatUnitList().Count == 0) return;
        float increase = 0f;
        increase = (1 - e.hitPercentage)/1.3f;

        actionProbability += increase;
    }


    public override string GetActionName()
    {
        return "Shoot";
    }
}
