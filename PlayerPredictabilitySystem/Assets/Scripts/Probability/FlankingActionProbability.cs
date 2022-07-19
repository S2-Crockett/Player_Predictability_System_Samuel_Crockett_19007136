using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class FlankingActionProbability : ActionProbabilityBase
{
    public static FlankingActionProbability Instance { get; private set; }

    private bool hasFlanked;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one FlankingActionProbability " + transform + " - " + Instance);
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
            unit.GetAction<MoveAction>().OnStopMoving += MoveAction_OnStopMoving;
        }
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        if (UnitManager.Instance.GetEnemyInCombatUnitList().Count == 0) return;
        
        bool isFlanked = false;
        bool isUnitFlanking = false;
        
        MoveAction moveAction = sender as MoveAction;
        List<Vector3> posList = moveAction.GetPositionList();

        Unit unit = LevelGrid.Instance.GetUnitAtPosition(
            LevelGrid.Instance.GetGridPosition(posList[posList.Count - 1]));


        foreach (var enemyUnit in UnitManager.Instance.GetAIUnitList())
        {
            isFlanked = false;
            isUnitFlanking = false;
            foreach (var friendlyUnit in UnitManager.Instance.GetPlayerUnitList())
            {
                if(isFlanked) continue;
                if (enemyUnit.GetCoverType() == CoverType.None) continue;

                List<Transform> coverObjects = LevelGrid.Instance.GetUnitCoverObject(enemyUnit.GetGridPosition());
                int hitObjects = 0;
                for (int i = 0; i < coverObjects.Count; i++)
                {
                    float distance = Vector3.Distance(friendlyUnit.GetWorldPosition(), enemyUnit.GetWorldPosition());
                    Vector3 direction = (enemyUnit.GetWorldPosition() - friendlyUnit.GetWorldPosition()).normalized;
                    Vector3 origin = friendlyUnit.GetWorldPosition() + Vector3.up * 1f;

                    RaycastHit[] hitInfo = Physics.SphereCastAll(origin, 1.2f, direction, distance);

                    for (int x = 0; x < hitInfo.Length; x++)
                    {
                        if (coverObjects[i].name == hitInfo[x].collider.gameObject.name)
                        {
                            if (friendlyUnit == unit) isUnitFlanking = true;
                                hitObjects++;
                            break;
                        }
                    }
                }
                if (hitObjects > 0)
                {
                    friendlyUnit.GetFlankingUnit().Remove(enemyUnit);
                    enemyUnit.SetIsFlanked(false);
                }
                else
                {
                    friendlyUnit.GetFlankingUnit().Remove(enemyUnit);
                    enemyUnit.SetIsFlanked(true);
                    isFlanked = true;
                    if (!isUnitFlanking && !hasFlanked)
                    {
                        float pathLength;
                        GridPosition startGridPosition = LevelGrid.Instance.GetGridPosition(posList[0]);
                        GridPosition endGridPosition = LevelGrid.Instance.GetGridPosition(posList[posList.Count - 1]);
                        pathLength = (float)Pathfinding.Instance.GetPathLength(startGridPosition, endGridPosition);
                        actionProbability += pathLength/30;
                        hasFlanked = true;
                    }
                    break;
                }
            }
        }
    }
    
    
    public bool IsGridPositionFlanked(GridPosition gridPosition)
    {
        int hitObjects = 0;

        foreach (var friendlyUnit in UnitManager.Instance.GetPlayerUnitList())
        {
            hitObjects = 0;
            if (LevelGrid.Instance.GetUnitCoverType(gridPosition) == CoverType.None) continue;

            List<Transform> coverObjects = LevelGrid.Instance.GetUnitCoverObject(gridPosition);

            for (int i = 0; i < coverObjects.Count; i++)
            {
                float distance = Vector3.Distance(friendlyUnit.GetWorldPosition(), LevelGrid.Instance.GetWorldPosition(gridPosition));
                Vector3 direction = (LevelGrid.Instance.GetWorldPosition(gridPosition) - friendlyUnit.GetWorldPosition()).normalized;
                Vector3 origin = friendlyUnit.GetWorldPosition() + Vector3.up * 1f;

                RaycastHit[] hitInfo = Physics.SphereCastAll(origin, 1.2f, direction, distance);

                for (int x = 0; x < hitInfo.Length; x++)
                {
                    if (coverObjects[i].name == hitInfo[x].collider.gameObject.name)
                    {
                        hitObjects++;
                        break;
                    }
                }
            }
            if (hitObjects == 0)
            {
                return true;
            }
        }
        return false;
    }

    public void SetHasFlanked(bool hasFlanked)
    {
        this.hasFlanked = hasFlanked;
    }


    public override string GetActionName()
    {
        return "Flanking";
    }
}
