using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandSelectTask : DiscreteInteractionTask
{
    public override void Perform(InputEventArgs eventArgs, Command command)
    {
        GameObject currentFocus = RuntimeCache.Instance.CurrentFocus;
        Debug.Log(currentFocus.name + "/Highlight");
        Transform[] transforms = currentFocus.transform.GetComponentsInChildren<Transform>(true);
        foreach(Transform trans in transforms)
        {
            if(trans.gameObject.name == "Highlight")
                trans.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
