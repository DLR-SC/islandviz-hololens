using HoloIslandVis.Interaction.Input;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class ContinuousInteractionTask : InteractionTask
    {
        private Action<GestureInputEventArgs> _startInteraction = delegate { };
        private Action<GestureInputEventArgs> _updateInteraction = delegate { };
        private Action<GestureInputEventArgs> _endInteraction = delegate { };

        public ContinuousInteractionTask()
        {

        }

        public override void Pass(GestureInputEventArgs eventData)
            => _startInteraction.Invoke(eventData);

        public void SetStartInteraction(Action<GestureInputEventArgs> action)
            => _startInteraction = action;

        public void SetUpdateInteraction(Action<GestureInputEventArgs> action)
            => _updateInteraction = action;

        public void SetEndInteraction(Action<GestureInputEventArgs> action)
            => _endInteraction = action;

        private void onUpdateInteraction(GestureInputEventArgs eventData)
            => _updateInteraction.Invoke(eventData);

        private void onEndInteraction(GestureInputEventArgs eventData)
            => _endInteraction.Invoke(eventData);
    }

}