using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeActionProbability : ActionProbabilityBase
{
    public static GrenadeActionProbability Instance { get; private set; }
    
    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GrenadeActionProbability " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        GrenadeProjectile.OnAnyGrenadeExploded += GrenadeProjectile_OnAnyGrenadeExploded;
    }
    
    private void GrenadeProjectile_OnAnyGrenadeExploded(object sender, EventArgs e)
    {
        if (UnitManager.Instance.GetEnemyInCombatUnitList().Count == 0) return;
        
        GrenadeProjectile grenade = sender as GrenadeProjectile;

        float totalObjects = grenade.GetNumberOfUnitsAndCrates();
        Unit unit = grenade.GetUnit();
        
        if(totalObjects > 0 && !unit.IsEnemy()) actionProbability += 0.7f/totalObjects;
    }

    public override string GetActionName()
    {
        return "Grenade";
    }
}
