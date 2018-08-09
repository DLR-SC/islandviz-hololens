using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class DiscreteInteractionTask : InteractionTask
    {
        private Action<BaseInputEventData> _executeAction = delegate { };

        public DiscreteInteractionTask()
        {

        }

        public override void Pass(BaseInputEventData eventData)
            => _executeAction.Invoke(eventData);

        public void SetInteraction(Action<BaseInputEventData> action)
            => _executeAction = action;
    }
}
