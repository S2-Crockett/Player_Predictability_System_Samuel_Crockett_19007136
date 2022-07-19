using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionPlanner : MonoBehaviour
{

    private enum TurnState
    {
        PlayerTurn,
        TakingTurn,
        TakingAction
    }

    private TurnState turnState;
    private float timer;

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnchanged;
        turnState = TurnState.PlayerTurn;
    }

    void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn()) return;

        switch (turnState)
        {
            case TurnState.PlayerTurn:
                break;
            case TurnState.TakingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    if (CanTakeAIAction(SetTakingTurnState)) turnState = TurnState.TakingAction;
                    else TurnSystem.Instance.NextTurn();
                }
                break;
            case TurnState.TakingAction:
                break;
        }
    }

    private void SetTakingTurnState()
    {
        timer = 0.5f;
        turnState = TurnState.TakingTurn;
    }

    private void TurnSystem_OnTurnchanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {       
            turnState = TurnState.TakingTurn; 
            timer = 2f;
        }
    }

    private bool CanTakeAIAction(Action onEnemyAIActionComplete)
    {
        foreach (Unit enemyUnit in UnitManager.Instance.GetAIUnitList()) if(CanTakeAIAction(enemyUnit, onEnemyAIActionComplete)) return true;
        return false;
    }

    private bool CanTakeAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        AIAction bestAIAction = null;
        BaseAction bestBaseAction = null;
        foreach (var baseAction in enemyUnit.GetBaseActionArray())
        {
            if (!enemyUnit.HasActionPoinsToTakeAction(baseAction)) continue;
            
            if (bestAIAction == null)
            {
                bestAIAction = baseAction.GetHighestPriorityEnemyAIAction();
                bestBaseAction = baseAction;
            }
            else
            {
                AIAction testAIAction = baseAction.GetHighestPriorityEnemyAIAction();
                if (testAIAction != null && testAIAction.actionPriority > bestAIAction.actionPriority)
                {
                    bestAIAction = testAIAction;
                    bestBaseAction = baseAction;
                }
            }

            baseAction.GetHighestPriorityEnemyAIAction();
        }

        if (bestAIAction != null && enemyUnit.TestActionPointsToTakeAction(bestBaseAction))
        {
            bestBaseAction.TakeAction(bestAIAction.gridPosition, onEnemyAIActionComplete);
            return true;
        }
        return false;
    }
    
}
