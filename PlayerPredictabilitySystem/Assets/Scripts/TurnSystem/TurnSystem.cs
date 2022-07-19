using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurnSystem : MonoBehaviour
{
    private int turnNumber = 1;
    private bool isPlayerTurn = true;
    
    public static TurnSystem Instance { get; private set; }

    public event EventHandler OnTurnChanged;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one TurnSystem " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void NextTurn()
    {
        turnNumber++;
        isPlayerTurn = !isPlayerTurn;

        if (!isPlayerTurn)
        {
            foreach (var enemyUnit in UnitManager.Instance.GetAIUnitList())
            {
                if (enemyUnit.TryGetComponent(out MoveAction moveAction))
                {
                    moveAction.SetHasMoved(false);
                }
                if (enemyUnit.TryGetComponent(out GrenadeAction grenadeAction))
                {
                    grenadeAction.SetHasPreformed(false);
                }
            }
        }

        FlankingActionProbability.Instance.SetHasFlanked(false);
        
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
}
