using HoloIslandVis.Utility;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IFocusable
{
    public void OnFocusEnter()
    {
        RuntimeCache.Instance.CurrentFocus = gameObject;
    }

    public void OnFocusExit()
    {
        RuntimeCache.Instance.CurrentFocus = null;
    }
}
