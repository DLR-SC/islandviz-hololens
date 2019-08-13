using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public abstract class ContinuousInteractionTask : InteractionTask
    {
        private float time_last_time_triggered = 0;

        private Dictionary<GestureType, Func<GestureInteractionEventArgs, IEnumerator>> _interactionTable;

        protected ContinuousInteractionTask()
        {
            _interactionTable = new Dictionary<GestureType, Func<GestureInteractionEventArgs, IEnumerator>>()
            {
                { GestureType.OneHandManipStart , eventArgs => StartInteraction(eventArgs) },
                { GestureType.TwoHandManipStart , eventArgs => StartInteraction(eventArgs) },
                { GestureType.ManipulationUpdate, eventArgs => UpdateInteraction(eventArgs) },
                { GestureType.ManipulationEnd   , eventArgs => EndInteraction(eventArgs) },
            };
        }

        public override IEnumerator Perform(InteractionEventArgs eventArgs)
        {
            float current_time = Time.time;
            if (current_time - time_last_time_triggered > 0.05f)
            {
                ScenarioHandler.IncrementCounterGestureControl();
            }
            time_last_time_triggered = Time.time;

            var casted = (GestureInteractionEventArgs) eventArgs;
            var action = _interactionTable[casted.Gesture];
            yield return action.Invoke(casted);
        }

        public abstract IEnumerator StartInteraction(GestureInteractionEventArgs eventArgs);
        public abstract IEnumerator UpdateInteraction(GestureInteractionEventArgs eventArgs);
        public abstract IEnumerator EndInteraction(GestureInteractionEventArgs eventArgs);
    }
}
