using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingUpdater : MonoBehaviour
{
    private void Start()
    {
        DestructableCrate.OnAnyDestroyed += DestructableCrate_OnAnyDestroyed;
    }

    private void DestructableCrate_OnAnyDestroyed(object sender, EventArgs e)
    {
        DestructableCrate crate = sender as DestructableCrate;
        Pathfinding.Instance.SetIsWalkable(crate.GetGridPosition(), true);
    }
}
