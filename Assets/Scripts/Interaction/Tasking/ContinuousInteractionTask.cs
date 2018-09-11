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
        public ContinuousInteractionTask()
        {
            GestureInputListener.Instance.ManipulationUpdate += UpdateInteraction;
            GestureInputListener.Instance.ManipulationEnd += EndInteraction;
        }

        public abstract void StartInteraction(GestureInputEventArgs eventArgs);
        public abstract void UpdateInteraction(GestureInputEventArgs eventArgs);
        public abstract void EndInteraction(GestureInputEventArgs eventArgs);
    }
}