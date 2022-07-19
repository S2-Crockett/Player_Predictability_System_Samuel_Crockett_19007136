using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    public event EventHandler OnAnyUnitMovedGridPosition;
    
    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width, height;
    [SerializeField] private float cellSize;
    
    private GridSystem<GridObject> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one LevelGrid " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        
        gridSystem = new GridSystem<GridObject>(width, height, cellSize, 
            (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));
    }

    private void Start()
    {
        Pathfinding.Instance.Setup(width, height, cellSize);
    }
    

    public void AddUnitToGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);
        AddUnitToGridPosition(toGridPosition, unit);
        
        OnAnyUnitMovedGridPosition?.Invoke(this, EventArgs.Empty);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
    
    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);
    
    public bool IsValidActionPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public int GetWidth() => gridSystem.GetWidth();
    
    public int GetHeight() => gridSystem.GetHeight();

    public bool HasUnitAtPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }
    
    public Unit GetUnitAtPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }
    
    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    public void SetCoverTypeAtGridPosition(GridPosition gridPosition, CoverType coverType)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetCoverType(coverType);
    }

    public void SetCoverObjectAtGridPosition(GridPosition gridPosition, Transform coverObject)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetCoverObject(coverObject);
    }

    public List<Transform> GetUnitCoverObject(GridPosition gridPosition)
    {
        Unit unit = GetUnitAtPosition(gridPosition);

        bool hasLeft = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x - 1, gridPosition.z));
        bool hasRight = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x + 1, gridPosition.z));
        bool hasFront = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x, gridPosition.z + 1));
        bool hasBack = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x, gridPosition.z - 1));

        Transform leftCover, rightCover, frontCover, backCover;
        leftCover = rightCover = frontCover = backCover = null;

        if (hasLeft)
            leftCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x - 1, gridPosition.z)).GetCoverObject();
        if (hasRight)
            rightCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x + 1, gridPosition.z)).GetCoverObject();
        if (hasFront)
            frontCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x, gridPosition.z + 1)).GetCoverObject();
        if (hasBack)
            backCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x, gridPosition.z - 1)).GetCoverObject();

        List<Transform> coverObjects = new List<Transform>();

        if (leftCover != null) coverObjects.Add(leftCover);
        if (rightCover != null) coverObjects.Add(rightCover);
        if (frontCover != null) coverObjects.Add(frontCover);
        if (backCover != null) coverObjects.Add(backCover);

        return coverObjects;
    }

    public CoverType GetUnitCoverType(GridPosition gridPosition)
    {
        bool hasLeft = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x - 1, gridPosition.z));
        bool hasRight = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x + 1, gridPosition.z));
        bool hasFront = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x, gridPosition.z + 1));
        bool hasBack = gridSystem.IsValidGridPosition(new GridPosition(gridPosition.x, gridPosition.z - 1));

        CoverType leftCover, rightCover, frontCover, backCover;
        leftCover = rightCover = frontCover = backCover = CoverType.None;

        if (hasLeft)
            leftCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x - 1, gridPosition.z)).GetcoverType();
        if (hasRight)
            rightCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x + 1, gridPosition.z)).GetcoverType();
        if (hasFront)
            frontCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x, gridPosition.z + 1)).GetcoverType();
        if (hasBack)
            backCover = gridSystem.GetGridObject(new GridPosition(gridPosition.x, gridPosition.z - 1)).GetcoverType();

        if (leftCover == CoverType.Full ||
            rightCover == CoverType.Full ||
            frontCover == CoverType.Full ||
            backCover == CoverType.Full) return CoverType.Full;
        
        if (leftCover == CoverType.Half ||
            rightCover == CoverType.Half ||
            frontCover == CoverType.Half ||
            backCover == CoverType.Half) return CoverType.Half;

        return CoverType.None;
    }
    



}
