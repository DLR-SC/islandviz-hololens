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

                gestureEventArgs = (GestureInputEventArgs) eventArgs;
                continuousInteractionTask.StartInteraction(gestureEventArgs);
            }
        }
    }
}
