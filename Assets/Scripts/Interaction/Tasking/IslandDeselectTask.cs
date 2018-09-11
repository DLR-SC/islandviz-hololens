using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandDeselectTask : DiscreteInteractionTask
{
    public override void Perform(InputEventArgs eventArgs, Command command)
    {
        foreach(GameObject islandGameObject in RuntimeCache.Instance.IslandGameObjects)
        {
            Transform[] transforms = islandGameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform trans in transforms)
            {
                if (trans.gameObject.name == "Highlight")
                    trans.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
