using System;
using HoloIslandVis.Automaton;
using HoloIslandVis.Utility;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IFocusable
{
    public InteractableType InteractableType {
        get {
            InteractableType interactableType;
            Enum.TryParse(gameObject.tag, out interactableType);
            return interactableType;
        }
    }

    public void OnFocusEnter()
    {
        RuntimeCache.Instance.CurrentFocus = gameObject;
        Debug.Log("Current focus: " + gameObject.tag);
    }

    public void OnFocusExit()
    {
        RuntimeCache.Instance.CurrentFocus = null;
    }
}
