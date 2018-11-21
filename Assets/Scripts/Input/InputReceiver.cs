using HoloIslandVis.Automaton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Input
{
    public abstract class InputReceiver
    {
        public InputReceiver()
        {
            SpeechInputListener.Instance.SpeechResponse += OnSpeechResponse;
            GestureInputListener.Instance.OneHandTap += OnOneHandTap;
            GestureInputListener.Instance.OneHandDoubleTap += OnOneHandDoubleTap;
            GestureInputListener.Instance.OneHandManipStart += OnOneHandManipStart;
            GestureInputListener.Instance.TwoHandManipStart += OnTwoHandManipStart;
            GestureInputListener.Instance.ManipulationUpdate += OnManipulationUpdate;
            GestureInputListener.Instance.ManipulationEnd += OnManipulationEnd;
                
        }

        public abstract void OnSpeechResponse(SpeechInputEventArgs eventArgs);
        public abstract void OnOneHandTap(GestureInputEventArgs eventArgs);
        public abstract void OnOneHandDoubleTap(GestureInputEventArgs eventArgs);
        public abstract void OnOneHandManipStart(GestureInputEventArgs eventArgs);
        public abstract void OnTwoHandManipStart(GestureInputEventArgs eventArgs);
        public abstract void OnManipulationUpdate(GestureInputEventArgs eventArgs);
        public abstract void OnManipulationEnd(GestureInputEventArgs eventArgs);
    }
}
