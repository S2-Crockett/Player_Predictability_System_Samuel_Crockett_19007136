using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class MoveAction : BaseAction
{
    private List<Vector3> positionList;
    private int positionIndex;

    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    [SerializeField] private int maxDistance = 6;
    
    private bool isPaused, hasMoved;

    private void Update()
    {
        if (!isActionActive) return;
        
        Vector3 targetPosition = positionList[positionIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        
        float rotSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotSpeed);
        
        float stoppingDistance = 0.1f;
        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance && !isPaused)
        {
            float moveSpeed = 5f;
            transform.position += moveDirection * Time.deltaTime * moveSpeed;
        }
        else
        {
            positionIndex++;
            if (positionIndex >= positionList.Count)
            {
                unit.SetPreviousAction(this);
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }
        }
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);
        
        unit.SetMovingGridPosition(gridPosition);
        
        positionIndex = 0;
        positionList = new List<Vector3>();

        foreach (var pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        OnStartMoving?.Invoke(this, EventArgs.Empty);        
        
        ActionStart(onActionComplete);
    }
    

    public override List<GridPosition> GetPositionsList(int offset = 0)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxDistance; x <= maxDistance; x++)
        {
            for (int z = -maxDistance; z <= maxDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidActionPosition(testGridPosition)) continue;
                if (unitGridPosition == testGridPosition) continue;
                if (LevelGrid.Instance.HasUnitAtPosition(testGridPosition)) continue;
                if (!Pathfinding.Instance.GetIsWalkable(testGridPosition)) continue;
                if (!Pathfinding.Instance.GetHasPath(unitGridPosition, testGridPosition)) continue;

                if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Flanking" && unit.IsEnemy())
                    if (FlankingActionProbability.Instance.IsGridPositionFlanked(testGridPosition)) continue;
                
                if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Grenade" && unit.IsEnemy()) 
                    if (IsUnitInRangeOfGrenade(testGridPosition)) continue;
                
                if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Shoot" && unit.IsEnemy()) 
                    if (!DoesGridPositionHaveCover(testGridPosition)) continue;
                
                int pathfindingDistanceMultiplier = 10;
                if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxDistance * pathfindingDistanceMultiplier) continue;
                
                validGridPositionList.Add(testGridPosition);
            }
        }

        if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Melee" && 
            unit.IsEnemy())
        {
            foreach (var gridPosition in RemovePlayerMeleePositions(validGridPositionList))
            {
                validGridPositionList.Remove(gridPosition);
            }
        }

        if (unit.IsEnemy() && !UnitManager.Instance.GetEnemyInCombatUnitList().Contains(unit))
        {
            List<GridPosition> currentPos = new List<GridPosition>();
            
            currentPos.Add(unit.GetGridPosition());
            
            return currentPos;
        }

        return validGridPositionList;
    }

    private bool IsUnitInRangeOfGrenade(GridPosition gridPosition)
    {
        float damageRadius = GrenadeProjectile.GetGrenadeRadius();
        Vector3 targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        
        Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);
        
        foreach (var collider in colliderArray)
        {
            if (collider.TryGetComponent(out Unit targetUnit))
            {
                if (targetUnit.IsEnemy() && targetUnit != unit) return true;
            }
        }
        return false;
    }

    private bool DoesGridPositionHaveCover(GridPosition gridPosition)
    {
        List<Transform> coverObjects = new List<Transform>();
        coverObjects = LevelGrid.Instance.GetUnitCoverObject(gridPosition);

        foreach (var objects in coverObjects)
        {
            if (objects.GetComponent<Cover>().GetCoverType() == CoverType.Full)
            {
                return true;
            }
        }
        
        return false;
    }
    

    private List<GridPosition> RemovePlayerMeleePositions(List<GridPosition> validGridPositionList)
    {
        List<GridPosition> removeGridPositions = new List<GridPosition>();
        if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Melee" & unit.IsEnemy())
        {
            foreach (var validGridPosition in validGridPositionList)
            {
                if (MeleeActionProbability.Instance.IsUnitInMeleeMoveRange(validGridPosition))
                {
                    GridPosition left, topLeft, bottomLeft, right, topRight, bottomRight, front, back;
                    left = new GridPosition(validGridPosition.x - 1, validGridPosition.z);
                    topLeft = new GridPosition(validGridPosition.x - 1, validGridPosition.z + 1);
                    bottomLeft = new GridPosition(validGridPosition.x - 1, validGridPosition.z - 1);
                    right = new GridPosition(validGridPosition.x + 1, validGridPosition.z);
                    topRight = new GridPosition(validGridPosition.x + 1, validGridPosition.z + 1);
                    bottomRight = new GridPosition(validGridPosition.x + 1, validGridPosition.z - 1);
                    front = new GridPosition(validGridPosition.x, validGridPosition.z + 1);
                    back = new GridPosition(validGridPosition.x, validGridPosition.z - 1);

                    if (LevelGrid.Instance.IsValidActionPosition(left)) removeGridPositions.Add(left);
                    if (LevelGrid.Instance.IsValidActionPosition(topLeft)) removeGridPositions.Add(topLeft);
                    if (LevelGrid.Instance.IsValidActionPosition(bottomLeft)) removeGridPositions.Add(bottomLeft);
                    if (LevelGrid.Instance.IsValidActionPosition(right)) removeGridPositions.Add(right);
                    if (LevelGrid.Instance.IsValidActionPosition(topRight)) removeGridPositions.Add(topRight);
                    if (LevelGrid.Instance.IsValidActionPosition(bottomRight)) removeGridPositions.Add(bottomRight);
                    if (LevelGrid.Instance.IsValidActionPosition(front)) removeGridPositions.Add(front);
                    if (LevelGrid.Instance.IsValidActionPosition(back)) removeGridPositions.Add(back);
                }
            }
        }

        return removeGridPositions;
    }
    
    public override string GetActionName()
    {
        return "Move";
    }

    public override AIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);

        if (unit.IsEnemy() && !UnitManager.Instance.GetEnemyInCombatUnitList().Contains(unit))
        {
            return new AIAction
            {
                gridPosition = gridPosition,
                actionPriority = 100
            };
        }
        
        if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Flanking" ||
            PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Melee" ||  
            PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Grenade" ||
            PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Shoot" &&
            !hasMoved)
        {
            hasMoved = true;
            return new AIAction
            {
                gridPosition = gridPosition,
                actionPriority = targetCountAtGridPosition + 1 * 100
            };
        }

        if (PlayerActionProbability.Instance.GetHighestProbabilityAction().GetActionName() == "Overwatch")
        {
            return new AIAction
            {
                gridPosition = gridPosition,
                actionPriority = 0
            };
        }

        return new AIAction
        {
            gridPosition = gridPosition,
            actionPriority = 109
        };
    }
    
    public void SetIsPaused(bool isPaused)
    {
        this.isPaused = isPaused;
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }

    public List<Vector3> GetPositionList()
    {
        return positionList;
    }

    public void SetHasMoved(bool hasMoved)
    {
        this.hasMoved = hasMoved;
    }

}
