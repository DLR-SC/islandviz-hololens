using HoloIslandVis.Controller;
using HoloIslandVis.Interaction;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioHandler : MonoBehaviour
{

    public static float scenarioStartTime;
    public static float scenarioEndTime;

    private UIComponent _visualization;

    public void SetupScenario(string scenarioName, bool adjust_view)
    {
        Debug.Log(scenarioName);
        _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);
        switch (scenarioName)
        {
            case "1st scenario":
                init_scenarioOne(adjust_view);
                break;
            case "2nd scenario":
                init_scenarioTwo(adjust_view);
                break;
            case "3rd scenario":
                init_scenarioThree();
                break;
            default:
                break;
        }
        scenarioStartTime = Time.time;
    }

    private void init_scenarioOne(bool adjust_view)
    {
        if (adjust_view) {
            AdjustVisalization();
        }
        GameObject.Find("RCE Cluster Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
    }

    private void init_scenarioTwo(bool adjust_view)
    {
        if (adjust_view)
        {
            AdjustVisalization();
        }
    }

    private void init_scenarioThree()
    {
        GestureInteractionEventArgs eventArgs = new GestureInteractionEventArgs();
        //AdjustVisalization();
        eventArgs.Focused = GameObject.Find("RCE Cluster Component Execution").GetComponent<Interactable>();
        eventArgs.Focused._focused = true;
        eventArgs.Focused._selected = false;
        eventArgs.Gesture = GestureType.OneHandTap;
        eventArgs.HandOnePos = new Vector3(-0.4f, 1.4f, -5.3f);
        eventArgs.HandTwoPos = new Vector3(0f, 0f, 0f);
        eventArgs.IsTwoHanded = false;
        eventArgs.Selected = GameObject.Find("NoneInteractableProxy").GetComponent<Interactable>();
        eventArgs.Selected._focused = false;
        eventArgs.Selected._selected = false;

        Command command = new Command(GestureType.OneHandTap, KeywordType.None, InteractableType.Bundle);
        StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));
    }

    public void AdjustVisalization()
    {
        float newScale = 0.00225f;
        _visualization.transform.localScale = new Vector3(newScale, newScale, newScale);
        _visualization.transform.position = _visualization.transform.position + new Vector3(0.06f, 0f, 0.06f);
    }
}
