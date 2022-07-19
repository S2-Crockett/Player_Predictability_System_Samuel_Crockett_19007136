using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    [SerializeField] private CoverType coverType;

    private GridPosition gridPosition;
    
    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetCoverTypeAtGridPosition(gridPosition, coverType);
        LevelGrid.Instance.SetCoverObjectAtGridPosition(gridPosition, GetComponent<Transform>());
    }

    public CoverType GetCoverType()
    {
        return coverType;
    }
}

public enum CoverType
{
    None, 
    Half,
    Full
}
