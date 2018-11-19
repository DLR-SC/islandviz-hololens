using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Input;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public abstract class InteractionTask
    {
        public abstract void Perform(InputEventArgs eventArgs, Command command);
    }
}
