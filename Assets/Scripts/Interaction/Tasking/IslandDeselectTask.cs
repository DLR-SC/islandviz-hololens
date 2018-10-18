using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandDeselectTask : DiscreteInteractionTask
{
    public override void Perform(InputEventArgs eventArgs, Command command)
    {
        foreach(Island island in RuntimeCache.Instance.Islands)
        {
            Transform[] transforms = island.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform trans in transforms)
            {
                if (trans.gameObject.name == "Highlight")
                    trans.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
