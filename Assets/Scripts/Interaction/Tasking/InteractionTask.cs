using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction.Input;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public abstract class InteractionTask
    {
        public void Pass(InputEventArgs eventArgs, Command command)
        {
            if(this is DiscreteInteractionTask)
                ((DiscreteInteractionTask) this).Perform(eventArgs);

            if(this is ContinuousInteractionTask)
            {
                GestureInputEventArgs gestureEventArgs;
                ContinuousInteractionTask continuousInteractionTask = (ContinuousInteractionTask) this;

                if(!(eventArgs is GestureInputEventArgs))
                    return;

                // TODO: Find other solution for unsubscribing prior Update and End events.
                GestureInputListener.Instance.ResetSubscriptions();

                gestureEventArgs = (GestureInputEventArgs) eventArgs;
                continuousInteractionTask.StartInteraction(gestureEventArgs);

                GestureInputListener.Instance.ManipulationUpdate += continuousInteractionTask.UpdateInteraction;
                GestureInputListener.Instance.ManipulationEnd += continuousInteractionTask.EndInteraction;
            }
        }
    }
}
