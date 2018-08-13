using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
    public class GestureInputEventArgs : EventArgs
    {
        public IInputSource[] InputSources;

        public GestureInputEventArgs(GestureSource[] gestureSources)
        {
            InputSources = new IInputSource[gestureSources.Length];
            for(int i = 0; i < gestureSources.Length; i++)
                InputSources[i] = gestureSources[i].InputSource;
        }
    }
}