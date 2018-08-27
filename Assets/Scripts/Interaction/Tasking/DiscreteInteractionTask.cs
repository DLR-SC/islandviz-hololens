using HoloIslandVis.Interaction.Input;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class DiscreteInteractionTask : InteractionTask
    {
        private Action<GestureInputEventArgs> _executeAction = delegate { };

        public DiscreteInteractionTask() { }
        public DiscreteInteractionTask(Action<GestureInputEventArgs> action)
        {
            _executeAction = action;
        }

        public override void Pass(GestureInputEventArgs eventData)
            => _executeAction.Invoke(eventData);

        public void SetInteraction(Action<GestureInputEventArgs> action)
            => _executeAction = action;
    }
}
