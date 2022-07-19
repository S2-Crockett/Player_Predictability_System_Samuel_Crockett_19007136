using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnSystemUI : MonoBehaviour
{
    
    [SerializeField] private Button endTurnButton;
    [SerializeField] private GameObject enemyTurnGameObject;

    private void Start()
    {
        endTurnButton.onClick.AddListener(() =>
        {
            TurnSystem.Instance.NextTurn();
        });

        TurnSystem.Instance.OnTurnChanged += Turnsystem_OnTurnChanged;
        
        UpdateEnemyTurnVisual();
        UpdateEndTurnVisibility();
    }

    private void Turnsystem_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateEnemyTurnVisual();
        UpdateEndTurnVisibility();
    }

    private void UpdateEnemyTurnVisual()
    {
        enemyTurnGameObject.SetActive(!TurnSystem.Instance.IsPlayerTurn());    
    }

    private void UpdateEndTurnVisibility()
    {
        endTurnButton.gameObject.SetActive(TurnSystem.Instance.IsPlayerTurn());
    }
}
