using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class Unit : MonoBehaviour
{
    private const int ACTION_COST_MAX = 2;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;
    public event EventHandler OnUnitMoved;

    [SerializeField] private bool isEnemy, isFlanked;
    
    private GridPosition gridPosition, movingGridPosition;
    private HealthSystem healthSystem;
    private BaseAction[] baseActionArray;
    private CoverType coverType;
    private List<Unit> flankingUnits = new List<Unit>();
    private BaseAction previousAction;


    private int actionPoints = ACTION_COST_MAX;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        baseActionArray = GetComponents<BaseAction>();
        previousAction = GetComponent<OverwatchAction>();
    }

    private void Start()
    { 
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitToGridPosition(gridPosition, this);

        TurnSystem.Instance.OnTurnChanged += Turnsystem_OnTurnChanged;
        healthSystem.OnDead += HealthSystem_OnDead;
        
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
        
        UpdateCoverType();
    }

    private void Update()
    {
        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition)
        {
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
            OnUnitMoved?.Invoke(this, EventArgs.Empty);
            UpdateCoverType();
        }
    }

    public T GetAction<T>() where T : BaseAction
    {
        foreach (var baseAction in baseActionArray)
        {
            if (baseAction is T) return (T)baseAction;
        }
        return null;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    public bool TestActionPointsToTakeAction(BaseAction baseAction)
    {
        if (HasActionPoinsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        return false;
    }

    public bool HasActionPoinsToTakeAction(BaseAction baseAction)
    {
        return actionPoints >= baseAction.GetActionPointsCost();
    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }
    
    private void Turnsystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) ||
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = ACTION_COST_MAX;
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    public void Damage(int damageAmount)
    {
        healthSystem.Damage(damageAmount);
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        LevelGrid.Instance.RemoveUnitAtGridPosition(gridPosition, this);
        Destroy(gameObject);
        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
    }

    private void UpdateCoverType()
    {
        coverType = LevelGrid.Instance.GetUnitCoverType(GetGridPosition());
    }

    public CoverType GetCoverType()
    {
        return coverType;
    }

    public GridPosition GetMovingGridPosition()
    {
        return movingGridPosition;
    }

    public void SetMovingGridPosition(GridPosition gridPosition)
    {
        movingGridPosition = gridPosition;
    }

    public bool GetIsFlanked()
    {
        return isFlanked;
    }

    public void SetIsFlanked(bool isFlanked)
    {
        this.isFlanked = isFlanked;
    }
    public List<Unit> GetFlankingUnit()
    {
        return flankingUnits;
    }

    public void SetCoverType()
    {
        coverType = LevelGrid.Instance.GetUnitCoverType(GetGridPosition());
    }

    public void SetPreviousAction(BaseAction baseAction)
    {
        previousAction = baseAction;
    }

    public BaseAction GetPreviousAction()
    {
        return previousAction;
    }
}
