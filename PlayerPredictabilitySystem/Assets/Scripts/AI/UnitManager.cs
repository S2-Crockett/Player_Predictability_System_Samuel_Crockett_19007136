using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance { get; private set; }

    public event EventHandler OnAnyUnitMovedGridPosition;

    private List<Unit> unitList, playerUnitList, aiUnitList, enemyInCombatUnitList;

    private void Awake()
    {
        unitList = new List<Unit>();
        playerUnitList = new List<Unit>();
        aiUnitList = new List<Unit>();
        enemyInCombatUnitList = new List<Unit>();
        
        if (Instance != null)
        {
            Debug.LogError("There is more than one UnitManager " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        unitList.Add(unit);

        unit.OnUnitMoved += Unit_OnAnyUnitMoved;
        AddUnitToCombatUnitList(unit);

        if (unit.IsEnemy())
        {
            aiUnitList.Add(unit);
        }
        else
        {
            playerUnitList.Add(unit);
        }
    }

    private void Unit_OnAnyUnitMoved(object sender, EventArgs e)
    {
        OnAnyUnitMovedGridPosition?.Invoke(sender, EventArgs.Empty);

        Unit unit = sender as Unit;

        AddUnitToCombatUnitList(unit);
    }

    private void AddUnitToCombatUnitList(Unit unit)
    {
        if (unit.IsEnemy())
        {
            foreach (var friendlyUnit in GetPlayerUnitList())
            {
                if(unit.TryGetComponent(out ShootAction shootAction))
                    if (shootAction.GetPositionsList().Contains(friendlyUnit.GetGridPosition()))
                        if(!enemyInCombatUnitList.Contains(unit)) enemyInCombatUnitList.Add(unit);
            }
        }
        else
        {
            foreach (var enemyUnit in GetAIUnitList())
            {
                if(unit.TryGetComponent(out ShootAction shootAction))
                    if (shootAction.GetPositionsList().Contains(enemyUnit.GetGridPosition()))
                        if(!enemyInCombatUnitList.Contains(enemyUnit)) enemyInCombatUnitList.Add(enemyUnit);
            }
        }
    }
    
    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        
        unitList.Remove(unit);
        
        if (unit.IsEnemy()) aiUnitList.Remove(unit);
        else playerUnitList.Remove(unit);

        if (enemyInCombatUnitList.Contains(unit)) enemyInCombatUnitList.Remove(unit);
    }

    public List<Unit> GetEnemyInCombatUnitList()
    {
        return enemyInCombatUnitList;
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }
    public List<Unit> GetPlayerUnitList()
    {
        return playerUnitList;
    }
    public List<Unit> GetAIUnitList()
    {
        return aiUnitList;
    }
}
