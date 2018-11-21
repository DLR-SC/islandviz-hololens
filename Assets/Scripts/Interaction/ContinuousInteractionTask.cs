using HoloIslandVis.Automaton;
using HoloIslandVis.Input;
using HoloIslandVis.Interaction;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GestureType = HoloIslandVis.Automaton.GestureType;

namespace HoloIslandVis.Interaction
{
    public abstract class ContinuousInteractionTask : InteractionTask
    {
        private Dictionary<GestureType, Action<GestureInputEventArgs>> _interactionTable;

        protected ContinuousInteractionTask()
        {
            _interactionTable = new Dictionary<GestureType, Action<GestureInputEventArgs>>()
            {
                { GestureType.OneHandManipStart , eventArgs => StartInteraction(eventArgs) },
                { GestureType.TwoHandManipStart , eventArgs => StartInteraction(eventArgs) },
                { GestureType.ManipulationUpdate, eventArgs => UpdateInteraction(eventArgs) },
                { GestureType.ManipulationEnd   , eventArgs => EndInteraction(eventArgs) },
            };
        }

        public override void Perform(InputEventArgs inputEventArgs, Command command)
        {
            GestureInputEventArgs eventArgs = (GestureInputEventArgs)inputEventArgs;
            _interactionTable[(GestureType)eventArgs.GestureType].Invoke(eventArgs);
        }

        public abstract void StartInteraction(GestureInputEventArgs eventArgs);
        public abstract void UpdateInteraction(GestureInputEventArgs eventArgs);
        public abstract void EndInteraction(GestureInputEventArgs eventArgs);
    }
}