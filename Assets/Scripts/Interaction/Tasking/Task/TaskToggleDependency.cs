using HoloIslandVis.Core.Metaphor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskToggleDependency : DiscreteGestureInteractionTask
    {
        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            DependencyDock dock = eventArgs.Focused.GetComponent<DependencyDock>();
            Toggle(dock);
            yield break;
        }

        public void Toggle(DependencyDock dock)
        {
            if (dock.expanded) dock.hideAllDependencies();
            else dock.showAllDependencies();
        }
    }
}
