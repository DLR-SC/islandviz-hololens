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
        public void Pass(InputEventArgs eventArgs)
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


                //UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "EventArgs: " + Convert.ToString(gestureEventArgs.Gesture, 2);

                if (gestureEventArgs.Gesture == Convert.ToInt16("0000000011000001", 2))
                {
                    UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TryStartManipulation.";
                    continuousInteractionTask.StartInteraction(gestureEventArgs);
                }

                if (gestureEventArgs.Gesture == Convert.ToInt16("1100000111000001", 2))
                {
                    UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TryStartManipulation.";
                    continuousInteractionTask.StartInteraction(gestureEventArgs);
                }

                if (gestureEventArgs.Gesture == Convert.ToInt16("1111111111111111", 2))
                    continuousInteractionTask.UpdateInteraction(gestureEventArgs);

                if (gestureEventArgs.Gesture == Convert.ToInt16("0000000010001000", 2))
                    continuousInteractionTask.EndInteraction(gestureEventArgs);

                if (gestureEventArgs.Gesture == Convert.ToInt16("1000100010001000", 2))
                    continuousInteractionTask.EndInteraction(gestureEventArgs);
            }
        }
    }
}
