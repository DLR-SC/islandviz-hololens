using HoloIslandVis.Interaction.Input;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public abstract class DiscreteInteractionTask : InteractionTask
    {
        public DiscreteInteractionTask()
        {

        }

        public abstract void Perform(InputEventArgs eventArgs);
    }
}
