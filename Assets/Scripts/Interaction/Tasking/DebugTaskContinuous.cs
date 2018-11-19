using HoloIslandVis.Interaction;
using HoloIslandVis.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Input.Tasking
{
    public class DebugTaskContinuous : ContinuousInteractionTask
    {
        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            Debug.Log("InteractionStart");
        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            Debug.Log("InteractionUpdate");
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            Debug.Log("InteractionEnd");
        }
    }
}