using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class ContinuousInteractionTask : InteractionTask
    {
        private Action<BaseInputEventData> _startInteraction = delegate { };
        private Action<BaseInputEventData> _updateInteraction = delegate { };
        private Action<BaseInputEventData> _endInteraction = delegate { };

        public ContinuousInteractionTask()
        {

        }

        public override void Pass(BaseInputEventData eventData)
            => _startInteraction.Invoke(eventData);

        public void SetStartInteraction(Action<BaseInputEventData> action)
            => _startInteraction = action;

        public void SetUpdateInteraction(Action<BaseInputEventData> action)
            => _updateInteraction = action;

        public void SetEndInteraction(Action<BaseInputEventData> action)
            => _endInteraction = action;

        private void onUpdateInteraction(BaseInputEventData eventData)
            => _updateInteraction.Invoke(eventData);

        private void onEndInteraction(BaseInputEventData eventData)
            => _endInteraction.Invoke(eventData);
    }

}