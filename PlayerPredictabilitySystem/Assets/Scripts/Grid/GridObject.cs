using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{

    private GridSystem<GridObject> gridSystem;
    private GridPosition gridPosition;
    private List<Unit> unitList;
    private IInteractable interactable;
    private CoverType coverType;
    private Transform coverObject;

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this.gridSystem   = gridSystem;
        this.gridPosition = gridPosition;
        unitList = new List<Unit>();
    }
    
    public override string ToString()
    {
        string unitString = "";
        foreach (var unit in unitList)
        {
            unitString += unit + "\n";
        }
        return gridPosition.ToString() + "\n" + unitString;
    }

    public void AddUnit(Unit unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }
        else
        {
            return null;
        }
    }

    public IInteractable GetInteractable()
    {
        return interactable;
    }

    public void SetInteractable(IInteractable interactable)
    {
        this.interactable = interactable;
    }

    public CoverType GetcoverType()
    {
        return coverType;
    }

    public void SetCoverType(CoverType coverType)
    {
        this.coverType = coverType;
    }

    public Transform GetCoverObject()
    {
        return coverObject;
    }

    public void SetCoverObject(Transform coverObject)
    {
        this.coverObject = coverObject;
    }
}
