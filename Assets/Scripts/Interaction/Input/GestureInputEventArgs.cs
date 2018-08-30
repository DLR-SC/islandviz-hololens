using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
    public class GestureInputEventArgs : InputEventArgs
    {
        public short Gesture;
        public IInputSource[] InputSources;
        public uint[] SourceIds;

        public GestureInputEventArgs(short gesture, GestureSource[] gestureSources)
        {
            Gesture = gesture;
            InputSources = new IInputSource[gestureSources.Length];
            SourceIds = new uint[gestureSources.Length];

            for(int i = 0; i < gestureSources.Length; i++)
            {
                InputSources[i] = gestureSources[i].InputSource;
                SourceIds[i] = gestureSources[i].SourceId;
            }
                
        }
    }
}