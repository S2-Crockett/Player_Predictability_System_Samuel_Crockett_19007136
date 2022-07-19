using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public abstract class ActionProbabilityBase : MonoBehaviour
{

    [SerializeField] protected float actionProbability = 2;

    public abstract string GetActionName();
    
    public void UpdateActionProbability(float probabilityChange)
    {
        actionProbability += probabilityChange;
    }

    public float GetActionProbability()
    {
        return actionProbability;
    }

}
