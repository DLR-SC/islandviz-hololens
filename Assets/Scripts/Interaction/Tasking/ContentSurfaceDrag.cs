using HoloIslandVis;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public class ContentSurfaceDrag : ContinuousInteractionTask
    {


        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            Debug.Log("ContentSurfaceDrag Start");
        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            Debug.Log("ContentSurfaceDrag Update");
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            Debug.Log("ContentSurfaceDrag End");
        }
    }
}
