using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OverwatchAction : BaseAction
{
    public event EventHandler<OnOverwatchEventArgs> OnShoot;
    public event EventHandler<OnOverwatchEventArgs> OnEnterOverwatch;

    public class OnOverwatchEventArgs : EventArgs
    {
        public Unit targetUnit;
        public bool hit;
    }

    private enum State
    {
        Aiming,
        Shooting,
        Finish
    }

    [SerializeField] private LayerMask obstaclesLayerMask;

    private State state;
    private int maxShootDistnace = 7;
    private float stateTimer, maxAccuracy = 0.8f;
    private Unit targetUnit;
    private bool canShootBullet;
    private bool passiveActive;


    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        UnitManager.Instance.OnAnyUnitMovedGridPosition += UnitManager_OnAnyUnitMovedGridPosition;
    }

    private void Update()
    {
        if (!isActionActive) return;

        ActionComplete();
    }
    
    private void UnitManager_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {
        if (passiveActive) {
            targetUnit = sender as Unit;
            if (IsValidActionPosition(targetUnit.GetGridPosition()))
            {
                StartCoroutine(HandleOverwatchAction());
            }
        }
    }

    private IEnumerator HandleOverwatchAction()
    {
        yield return new WaitForSeconds(0);
        while (passiveActive)
        {
            switch (state)
            {
                case State.Aiming:
                    Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                    float rotateSpeed = 10f;
                    transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);

                    if (targetUnit.TryGetComponent(out UnitAnimator animator))
                    {
                        animator.PauseAnimation();
                    }
                    if (targetUnit.TryGetComponent(out MoveAction moveAction))
                    {
                        moveAction.SetIsPaused(true);
                    }
                    yield return new WaitForSeconds(stateTimer);
                    state = State.Shooting;
                    float shootingStateTime = 0.1f;
                    stateTimer = shootingStateTime;
                    
                    break;
                case State.Shooting:
                    if (canShootBullet)
                    {
                        Shoot();
                        canShootBullet = false;
                    }
                    
                    yield return new WaitForSeconds(stateTimer);
                    state = State.Finish;
                    float coolOffStateTime = 0.05f;
                    stateTimer = coolOffStateTime;
                    
                    break;
                case State.Finish:
                    unit.SetPreviousAction(this);
                    if (targetUnit.TryGetComponent(out UnitAnimator _animator))
                    {
                        _animator.ResumeAnimation();
                    }
                    if (targetUnit.TryGetComponent(out MoveAction _moveAction))
                    {
                        _moveAction.TakeAction(targetUnit.GetMovingGridPosition(), onActionComplete);
                        _moveAction.SetIsPaused(false);
                    }
                    passiveActive = false;
                    break;
            }
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn() && GetUnit().IsEnemy() ||
            TurnSystem.Instance.IsPlayerTurn() && !GetUnit().IsEnemy())
        {
            passiveActive = false;
        }
    }


    private void Shoot()
    {
        float hitPercentage = GetHitPercentage(targetUnit);
        float randomChance = UnityEngine.Random.Range(0, 1f);
        bool hit = randomChance < hitPercentage;
        
        OnShoot?.Invoke(this, new OnOverwatchEventArgs()
        {
            targetUnit   = targetUnit,
            hit = hit
        });
        if (hit) targetUnit.Damage(40);
    }
    
    public float GetHitPercentage(Unit shootUnit)
    {
        float hitPercentage = maxAccuracy;

        float shootDistance = GetShootDistance(shootUnit.GetGridPosition());
        float fullAcuracyShootDistance = 3f;
        float remainingShootDistance = Mathf.Max(0, shootDistance - fullAcuracyShootDistance);
        hitPercentage -= 0.05f * remainingShootDistance;

        switch (shootUnit.GetCoverType())
        {
            case CoverType.Full:
                hitPercentage -= 0.3f;
                break;
            case CoverType.Half:
                hitPercentage -= 0.1f;
                break;
        }

        return hitPercentage;
    }
    
    private float GetShootDistance(GridPosition gridPosition)
    {
        GridPosition currentGridPosition = unit.GetGridPosition();
        Vector2 shootVector = new Vector2(gridPosition.x - currentGridPosition.x, 
            gridPosition.z - currentGridPosition.z);

        float shootDistance = Mathf.Abs(shootVector.x) + Mathf.Abs(shootVector.y);
        return shootDistance;
    }
    

    public override string GetActionName()
    {
        return "Overwatch";
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        state = State.Aiming;
        float aimingStateTime = 0.1f;
        stateTimer = aimingStateTime;
        canShootBullet = true;
        
        passiveActive = true;   
        
        OnEnterOverwatch?.Invoke(this, new OnOverwatchEventArgs()
        {
            targetUnit   = targetUnit,
        });
        
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetPositionsList(int offset = 0)
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }
    
    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxShootDistnace; x <= maxShootDistnace; x++)
        {
            for (int z = -maxShootDistnace; z <= maxShootDistnace; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidActionPosition(testGridPosition)) continue;

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxShootDistnace) continue;

                if (!LevelGrid.Instance.HasUnitAtPosition(testGridPosition)) continue;

                Unit targetUnit = LevelGrid.Instance.GetUnitAtPosition(testGridPosition);
                if(targetUnit.IsEnemy() == unit.IsEnemy()) continue;
                
                float unitShoulderHeight = 1.7f;
                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                
                if(Physics.Raycast(unitWorldPosition + Vector3.up * unitShoulderHeight, shootDirection,
                       Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                       obstaclesLayerMask)) continue;

                if(!validGridPositionList.Contains(testGridPosition)) validGridPositionList.Add(testGridPosition);
            }
        }
        return validGridPositionList;
    }

    public override AIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Melee")
        {
            return new AIAction
            {
                gridPosition = gridPosition,
                actionPriority = 110
            };
        }
        return new AIAction
        {
            gridPosition = gridPosition,
            actionPriority = 7
        };
    }
}
