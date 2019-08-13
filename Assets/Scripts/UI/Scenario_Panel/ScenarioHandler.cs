using HoloIslandVis.Controller;
using HoloIslandVis.Interaction;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioHandler : MonoBehaviour
{

    public static float scenarioStartTime;
    public static float scenarioEndTime;
    public static int counterActions;

    public static Scenario_type scenario;
    public static Control_type control;

    public enum Scenario_type
    {
        FIRST,
        SECOND,
        THIRD
    }

    public enum Control_type
    {
        GESTURE,
        VOICE
    }

    private UIComponent _visualization;

    public void SetupScenario(
        string scenarioName,
        bool adjust_view,
        bool useGestureControl
        )
    {
        Debug.Log(scenarioName);
        _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);

        if (useGestureControl)
        {
            control = Control_type.GESTURE;
        } else
        {
            control = Control_type.VOICE;
        }

        switch (scenarioName)
        {
            case "1st scenario":
                scenario = Scenario_type.FIRST;
                init_scenarioOne(adjust_view);
                break;
            case "2nd scenario":
                scenario = Scenario_type.SECOND;
                init_scenarioTwo(adjust_view);
                break;
            case "3rd scenario":
                scenario = Scenario_type.THIRD;
                init_scenarioThree(adjust_view);
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
        switch (control)
        {
            case Control_type.GESTURE:
                GameObject.Find("RCE Component Switch GUI ").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
            case Control_type.VOICE:
                GameObject.Find("RCE Cluster Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
        }

    }

    private void init_scenarioTwo(bool adjust_view)
    {
        if (adjust_view)
        {
            Interactable GameObject_focused;
            switch (control)
            {
                case Control_type.GESTURE:
                    GameObject_focused = GameObject.Find("RCE Input Provider Component GUI").GetComponent<Interactable>();
                    break;
                case Control_type.VOICE:
                    GameObject_focused = GameObject.Find("RCE XML Merger Component Execution").GetComponent<Interactable>();
                    break;
                default:
                    GameObject_focused = null;
                    break;
            }

            GestureInteractionEventArgs eventArgs = new GestureInteractionEventArgs();
            eventArgs.Focused = GameObject_focused;
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
    }

    private void init_scenarioThree(bool adjust_view)
    {
        if (adjust_view)
        {
            AdjustVisalization();
        }
        switch (control)
        {
            case Control_type.GESTURE:
                GameObject.Find("RCE XML Loader Component GUI").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
            case Control_type.VOICE:
                GameObject.Find("RCE Excel Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
        }
    }

    public void AdjustIsland()
    {

        /*Interactable toolkitCommonModules = GameObject.Find("RCE Toolkit - Common Modules").GetComponent<Interactable>();
        Debug.Log("x Pos" + toolkitCommonModules.transform.position.x);
        Debug.Log("z Pos" + toolkitCommonModules.transform.position.z);
        toolkitCommonModules.transform.position = new Vector3(
            toolkitCommonModules.transform.position.x - 164.0f, 
            toolkitCommonModules.transform.position.y, 
            toolkitCommonModules.transform.position.z - 27.0f);
        Debug.Log("x Pos" + toolkitCommonModules.transform.position.x);
        Debug.Log("z Pos" + toolkitCommonModules.transform.position.z);*/
    }

    public void AdjustVisalization()
    {
        Debug.Log("Adjsut");
        float newScale = 0.00225f;
        _visualization.transform.localScale = new Vector3(newScale, newScale, newScale);
        _visualization.transform.position = _visualization.transform.position + new Vector3(0.06f, 0f, 0.06f);
    }
}
