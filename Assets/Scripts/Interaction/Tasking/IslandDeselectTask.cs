using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Input;
using HoloIslandVis.Interaction;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class IslandDeselectTask : DiscreteInteractionTask
    {
        public override void Perform(InputEventArgs eventArgs, Command command)
        {
            foreach (Island island in RuntimeCache.Instance.Islands)
            {
                Transform[] transforms = island.transform.GetComponentsInChildren<Transform>(true);
                foreach (Transform trans in transforms)
                {
                    if (trans.gameObject.name.Contains("_Highlight"))
                    {
                        trans.gameObject.GetComponent<MeshRenderer>().enabled = false;
                        trans.gameObject.SetActive(false);
                        UserInterface.Instance.Panel.SetActive(false);
                    }
                }
            }
        }
    }
}