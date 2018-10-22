using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowArrowTask : DiscreteInteractionTask
{
    public override void Perform(InputEventArgs eventArgs, Command command)
    {
        GameObject currentFocus = RuntimeCache.Instance.CurrentFocus;
        DependencyDock dependencyDock = currentFocus.GetComponent<DependencyDock>();
        if (!dependencyDock.expanded)
            dependencyDock.showAllDependencies();
        else
            dependencyDock.hideAllDependencies();

        Debug.Log("Showing/Hiding dependencies.");
    }
}
