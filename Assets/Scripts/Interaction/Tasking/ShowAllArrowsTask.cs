using HoloIslandVis.Automaton;
using HoloIslandVis.Input;
using HoloIslandVis.Interaction;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class ShowAllArrowsTask : DiscreteInteractionTask
    {
        bool expanded;

        public ShowAllArrowsTask()
        {
            expanded = false;
        }

        public override void Perform(InputEventArgs eventArgs, Command command)
        {
            List<GameObject> docks = RuntimeCache.Instance.Docks;

            foreach(GameObject dock in docks)
            {
                DependencyDock dependencyDock = dock.GetComponent<DependencyDock>();

                if (dependencyDock != null)
                {
                    if (!expanded)
                        dependencyDock.showAllDependencies();
                    else
                        dependencyDock.hideAllDependencies();
                }
            }

            expanded = !expanded;
            Debug.Log("Showing/Hiding ALL dependencies.");
        }
    }
}
