using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridPosition gridPosition;
    private int gCost, hCost, fCost;
    private PathNode previousNode;
    private bool isWalkable = true;
    
    public PathNode(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }
    
    public override string ToString()
    {
        return gridPosition.ToString();
    }

    public int GetGCost()
    {
        return gCost;
    }
    
    public int GetHCost()
    {
        return hCost;
    }
    
    public int GetFCost()
    {
        return fCost;
    }

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
    }
    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
    }

    public void CalculateFCost()
    {
        fCost = hCost + gCost;
    }

    public void ResetPreviousNode()
    {
        previousNode = null;
    }
    public void SetPreviousNode(PathNode pathNode)
    {
        previousNode = pathNode;
    }
    public PathNode GetPreviousNode()
    {
        return previousNode;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }

}
