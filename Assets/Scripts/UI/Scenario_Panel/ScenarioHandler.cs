using HoloIslandVis.Controller;
using HoloIslandVis.Core;
using HoloIslandVis.Interaction;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScenarioHandler : SingletonComponent<ScenarioHandler>
{

    public static float scenarioStartTime;
    public static float scenarioEndTime;
    public static int counterActionsGestureControl = 0;
    public static int counterActionsSpeechControl = 0;
    public static ArrayList keywordsGesture = new ArrayList();
    public static ArrayList keywordsSpeech = new ArrayList();


    public static string current_bundle;
    public static string current_package;
    public static string current_compilationUnit;

    public static Scenario_type scenario;
    public static Control_type control;

    private Vector3 scale;
    private Vector3 position;

    private bool first_time = true;
    public static string name_highlighted_island = "";

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

    public void init_stuff()
    {
        _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);

        float newScale = 0.00225f;
        scale = new Vector3(newScale, newScale, newScale);
        position = new Vector3(0.06f, 0f, 0.06f);
    }

    public void SetupScenario(
        string scenarioName,
        bool adjust_view,
        bool useGestureControl
        )
    {
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
                name_highlighted_island = "RCE Components Switch GUI";
                GameObject.Find("RCE Components Switch GUI").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
            case Control_type.VOICE:
                name_highlighted_island = "RCE Database Component Execution";
                GameObject.Find("RCE Database Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
        }

        UIManager.Instance.Activate(UIElement.StopScenarioPanel);
        GameObject stopScenarioPanel = GameObject.Find("StopScenarioPanel");
        GameObject contentPane = GameObject.Find("ContentPane");
        stopScenarioPanel.GetComponent<HoloToolkit.Unity.Tagalong>().enabled = false;
        stopScenarioPanel.transform.position = new Vector3(
            contentPane.transform.position.x - 0.4f,
            contentPane.transform.position.y + 1.05f,
            contentPane.transform.position.z - 0.94f
        );

    }

    private void init_scenarioTwo(bool adjust_view)
    {
        if (adjust_view)
        {
            AdjustVisalization();

            UIManager.Instance.Activate(UIElement.StopScenarioPanel);
            GameObject stopScenarioPanel = GameObject.Find("StopScenarioPanel");
            GameObject contentPane = GameObject.Find("ContentPane");
            stopScenarioPanel.GetComponent<HoloToolkit.Unity.Tagalong>().enabled = false;
            stopScenarioPanel.transform.position = new Vector3(
                contentPane.transform.position.x - 0.4f,
                contentPane.transform.position.y + 1.05f,
                contentPane.transform.position.z - 0.94f
            );
        }

        switch (control)
        {
            case Control_type.GESTURE:
                name_highlighted_island = "RCE XML Loader Component GUI";
                GameObject.Find("RCE XML Loader Component GUI").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
            case Control_type.VOICE:
                name_highlighted_island = "RCE Excel Component Execution";
                GameObject.Find("RCE Excel Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(true);
                break;
        }
    }

    private void init_scenarioThree(bool adjust_view)
    {
        if (adjust_view)
        {
            Interactable GameObject_focused;
            switch (control)
            {
                case Control_type.GESTURE:
                    name_highlighted_island = "RCE Input Provider Component GUI";
                    GameObject_focused = GameObject.Find("RCE Input Provider Component GUI").GetComponent<Interactable>();
                    break;
                case Control_type.VOICE:
                    name_highlighted_island = "RCE XML Merger Component Execution";
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
            AdjustVisalization();
            //StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));

            UIManager.Instance.Activate(UIElement.StopScenarioPanel);
            GameObject stopScenarioPanel = GameObject.Find("StopScenarioPanel");
            GameObject contentPane = GameObject.Find("ContentPane");
            stopScenarioPanel.GetComponent<HoloToolkit.Unity.Tagalong>().enabled = false;
            stopScenarioPanel.transform.position = new Vector3(
                contentPane.transform.position.x - 0.4f,
                contentPane.transform.position.y + 1.05f,
                contentPane.transform.position.z - 0.94f
            );
        }
    }

    public void AdjustVisalization()
    {
        if (first_time)
        {
            _visualization.transform.localScale = scale;
            _visualization.transform.position = _visualization.transform.position + position;
            first_time = false;
        } else
        {
            _visualization.transform.localScale = scale;
            _visualization.transform.localPosition = position;
            Debug.Log(_visualization.transform.position);
        }
    }

    private void ResetHighlights()
    {
        ScenarioHandler.name_highlighted_island = "";
        GameObject.Find("RCE XML Loader Component GUI").GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
        GameObject.Find("RCE Excel Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
        GameObject.Find("RCE Components Switch GUI").GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
        GameObject.Find("RCE Database Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
        GameObject.Find("RCE Input Provider Component GUI").GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
        GameObject.Find("RCE XML Merger Component Execution").GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
    }

    public static void IncrementCounterGestureControl()
    {
        counterActionsGestureControl++;
    }

    public static void IncrementCounterVoiceControl()
    {
        counterActionsSpeechControl++;
    }

    public void FinishScenario()
    {
        scenarioEndTime = Time.time;
        StartCoroutine(FetchData());
        StartCoroutine(BackToBlack());
    }

    private IEnumerator FetchData()
    {
        string putData = BuildPutData();
        byte[] bytes = Encoding.UTF8.GetBytes(putData);

        keywordsGesture = new ArrayList();
        keywordsSpeech = new ArrayList();

        string ServiceAdress = "192.168.1.100";
        string ServicePort = "5000";
        string serverEndpoint = "http://" + ServiceAdress + ":" + ServicePort + "/";
        
        var webRequest = UnityWebRequest.Put(serverEndpoint + "scenario", bytes);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        yield return webRequest.SendWebRequest();
        
        if (webRequest.isNetworkError)
        {
            Debug.Log("Error While Sending: " + webRequest.error);
            //_errorState = true;
        }
        else Debug.Log("Received: " + webRequest.downloadHandler.text);
        string _latestResponse = webRequest.downloadHandler.text;
        Debug.Log(_latestResponse);

        yield return null;
    }

    private string BuildPutData()
    {
        StringBuilder keywordsGestureString = new StringBuilder();
        keywordsGestureString.Append("[");
        foreach (string keyword in keywordsGesture)
        {
            keywordsGestureString.Append("\"" + keyword + "\", ");
        }

        if (keywordsGestureString.Length > 2)
        {
            keywordsGestureString.Remove(keywordsGestureString.Length - 2, 2);
        }
        keywordsGestureString.Append("]");

        StringBuilder keywordsSpeechString = new StringBuilder();
        keywordsSpeechString.Append("[");
        foreach (string keyword in keywordsSpeech)
        {
            keywordsSpeechString.Append("\"" + keyword + "\", ");
        }
        if (keywordsSpeechString.Length > 2)
        {
            keywordsSpeechString.Remove(keywordsSpeechString.Length - 2, 2);
        }
        keywordsSpeechString.Append("]");

        StringBuilder builder = new StringBuilder();

        builder.Append("{\"NumberScenario\": \"" + ScenarioHandler.scenario + "\",");
        builder.Append("\"TypeScenario\": \"" + ScenarioHandler.control + "\",");
        builder.Append("\"StartTime\": \"" + ScenarioHandler.scenarioStartTime + "\",");
        builder.Append("\"EndTime\": \"" + ScenarioHandler.scenarioEndTime + "\",");
        builder.Append("\"NumberActivitiesGesture\": \"" + ScenarioHandler.counterActionsGestureControl + "\",");
        builder.Append("\"ActivitiesGestures\": " + keywordsGestureString.ToString() + ",");
        builder.Append("\"NumberActivitiesVoice\": \"" + ScenarioHandler.counterActionsSpeechControl + "\",");
        builder.Append("\"SpeechGestures\": " + keywordsSpeechString.ToString() + "}");

        Debug.Log(builder.ToString());

        return builder.ToString();
    }

    private IEnumerator BackToBlack()
    {
        while(StateManager.Instance._isProcessing)
        {
            yield return new WaitForSeconds(0.1f);
        }
        State current_state = StateManager.Instance.CurrentState;

        GestureInteractionEventArgs eventArgs = new GestureInteractionEventArgs();
        eventArgs.Focused = GameObject.Find("Water").GetComponent<Interactable>();
        eventArgs.Focused._focused = true;
        eventArgs.Focused._selected = false;
        eventArgs.Gesture = GestureType.OneHandTap;
        eventArgs.HandOnePos = new Vector3(-0.4f, 1.4f, -5.3f);
        eventArgs.HandTwoPos = new Vector3(0f, 0f, 0f);
        eventArgs.IsTwoHanded = false;
        Command command = new Command(GestureType.OneHandTap, KeywordType.None, InteractableType.Bundle);

        switch(current_state.Name)
        {
            case "inspect building":
                eventArgs.Selected = GameObject.Find(current_compilationUnit).GetComponent<Interactable>();
                eventArgs.Selected._focused = false;
                eventArgs.Selected._selected = true;
                command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane, InteractableType.CompilationUnit, StaticItem.None);

                StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));
                while (current_state.Name == "inspect building")
                {
                    yield return new WaitForSeconds(0.1f);
                    current_state = StateManager.Instance.CurrentState;
                }
                break;
            case "inspect region":
                eventArgs.Selected = GameObject.Find(current_package).GetComponent<Interactable>();
                eventArgs.Selected._focused = false;
                eventArgs.Selected._selected = true;
                command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane, InteractableType.Package, StaticItem.None);

                StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));
                while (current_state.Name == "inspect region")
                {
                    yield return new WaitForSeconds(0.1f);
                    current_state = StateManager.Instance.CurrentState;
                }
                break;
            case "inspect island":
                Debug.Log("inspect island");
                eventArgs.Selected = GameObject.Find(current_bundle).GetComponent<Interactable>();
                eventArgs.Selected._focused = false;
                eventArgs.Selected._selected = true;
                command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane, InteractableType.Bundle, StaticItem.None);

                StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));
                while (current_state.Name == "inspect island")
                {
                    yield return new WaitForSeconds(0.1f);
                    current_state = StateManager.Instance.CurrentState;
                }
                break;
            default:
                break;
        }
        command = new Command(StaticItem.Reset);
        ResetHighlights();
        StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));

        while (current_state.Name == "main")
        {
            yield return new WaitForSeconds(0.1f);
            current_state = StateManager.Instance.CurrentState;
            Debug.Log(current_state.Name);
        }
        
        AppManager.first_time_opened = true;
        Debug.Log("End");

        yield return null;
    }
}
