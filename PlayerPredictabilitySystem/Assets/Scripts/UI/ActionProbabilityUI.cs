using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ActionProbabilityUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionNameText, actionProbabilityText;
    [SerializeField] private Image probabilityBarImage;
    private float initialProbabilityBase, actionMax;
    private BaseAction baseAction;
    private ActionProbabilityBase actionProbabilityBase;

    public void SetBaseAction(ActionProbabilityBase actionProbabilityBase, float actionMax)
    {
        this.actionProbabilityBase = actionProbabilityBase;
        actionNameText.text = actionProbabilityBase.GetActionName();
        initialProbabilityBase = actionProbabilityBase.GetActionProbability();
        this.actionMax = actionMax;
        actionProbabilityText.text = Mathf.Round((initialProbabilityBase / actionMax) * 100).ToString() + "%";

        probabilityBarImage.fillAmount = actionProbabilityBase.GetActionProbability() / actionMax;
    }

    public void UpdateSelectedVisual(float actionIncrease, float actionMax)
    {
        initialProbabilityBase = actionIncrease;
        actionProbabilityText.text = Mathf.Round((initialProbabilityBase / actionMax) * 100).ToString() + "%";
        probabilityBarImage.fillAmount = initialProbabilityBase / actionMax;
    }
    

    public ActionProbabilityBase GetActionProbabilityBase()
    {
        return actionProbabilityBase;
    }
}
