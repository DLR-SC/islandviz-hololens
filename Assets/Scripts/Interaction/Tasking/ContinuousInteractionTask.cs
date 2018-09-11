using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction.Input;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public abstract class ContinuousInteractionTask : InteractionTask
    {
        public override void Perform(InputEventArgs eventArgs, Command command)
        {
            StartInteraction((GestureInputEventArgs) eventArgs);

            GestureInputListener.Instance.ManipulationUpdate += OnManipulationUpdate;
            GestureInputListener.Instance.ManipulationEnd += OnManipulationEnd;
        }

        private void OnManipulationUpdate(GestureInputEventArgs eventArgs)
        {
            UpdateInteraction(eventArgs);
        }

        private void OnManipulationEnd(GestureInputEventArgs eventArgs)
        {
            GestureInputListener.Instance.ManipulationUpdate -= OnManipulationUpdate;
            GestureInputListener.Instance.ManipulationEnd -= OnManipulationEnd;
            EndInteraction(eventArgs);
        }

        public abstract void StartInteraction(GestureInputEventArgs eventArgs);
        public abstract void UpdateInteraction(GestureInputEventArgs eventArgs);
        public abstract void EndInteraction(GestureInputEventArgs eventArgs);
    }
}