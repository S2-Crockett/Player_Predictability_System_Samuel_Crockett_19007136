using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerActionProbability : MonoBehaviour
{
    public static PlayerActionProbability Instance { get; private set; }

    private List<Unit> playerUnits = new List<Unit>();
    private List<ActionProbabilityUI> actionProbabilityUIList = new List<ActionProbabilityUI>();
    
    [SerializeField] private Transform actionProbabilityPrefab, actionProbabilityContainerTransform;
    [SerializeField] private List<ActionProbabilityBase> actionProbabilityBases;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one PlayerActionProbability " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var actionProbabilityBase in GetComponentsInChildren<ActionProbabilityBase>())
        {
            actionProbabilityBases.Add(actionProbabilityBase);
        }
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
        BaseAction.OnActionCompleted += BaseAction_OnActionCompleted;
        
        CreateDebugVisuals();
    }


    private void BaseAction_OnActionCompleted(object sender, EventArgs e)
    {
        BaseAction baseAction = sender as BaseAction;
        Unit unit = baseAction.GetUnit();

        if (unit.IsEnemy()) return;

        float actionMax = 0f;

        foreach (var actionProbability in actionProbabilityBases)
        {
            actionMax += actionProbability.GetActionProbability();
        }

        foreach (var actionProbabilityUIElement in actionProbabilityUIList)
        {
            actionProbabilityUIElement.UpdateSelectedVisual(
                actionProbabilityUIElement.GetActionProbabilityBase().GetActionProbability(), actionMax);
        }
    }

    private void CreateDebugVisuals()
    {
        float actionMax = 0f;

        foreach (Transform buttonTransform in actionProbabilityContainerTransform)
        {
            Destroy(buttonTransform.gameObject);
        }
        
        actionProbabilityUIList.Clear();
        
        foreach (var actionProbability in actionProbabilityBases)
        {
            actionMax += actionProbability.GetActionProbability();
        }
        
        foreach (var baseAction in actionProbabilityBases)
        {
            Transform actionButtonTransform = Instantiate(actionProbabilityPrefab, actionProbabilityContainerTransform);
            ActionProbabilityUI actionButtonUI = actionButtonTransform.GetComponent<ActionProbabilityUI>();
            actionButtonUI.SetBaseAction(baseAction, actionMax);
            
            actionProbabilityUIList.Add(actionButtonUI);
        }
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        playerUnits = UnitManager.Instance.GetPlayerUnitList();
    }
    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        playerUnits = UnitManager.Instance.GetPlayerUnitList();
    }

    public ActionProbabilityBase GetHighestProbabilityAction()
    {
        ActionProbabilityBase highestProbabilityAction = actionProbabilityBases[0];

        foreach (var action in actionProbabilityBases)
        {
            if (action.GetActionProbability() > highestProbabilityAction.GetActionProbability())
            {
                highestProbabilityAction = action;
            }
        }
        return highestProbabilityAction;
    }
    
    
    
}
