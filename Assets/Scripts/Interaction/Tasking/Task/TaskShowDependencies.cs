using HoloIslandVis.Core.Metaphor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskShowDependencies : DiscreteGestureInteractionTask
    {
        private bool _isShowing = false;

        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            DependencyDock[] docks = GameObject.FindObjectsOfType<DependencyDock>();
            Toggle(docks);
            yield break;
        }

        public void Toggle(DependencyDock[] docks)
        {
            if (_isShowing)
            {
                foreach (DependencyDock dock in docks)
                    dock.hideAllDependencies();

                _isShowing = false;
            }
            else
            {
                foreach (DependencyDock dock in docks)
                    dock.showAllDependencies();

                _isShowing = true;
            }
        }
    }
}
