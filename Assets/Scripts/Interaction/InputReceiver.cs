using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputReceiver
{
    public InputReceiver()
    {
        SpeechInputListener.Instance.SpeechResponse += OnSpeechResponse;
        GestureInputListener.Instance.OneHandTap += OnOneHandTap;
        GestureInputListener.Instance.OneHandDoubleTap += OnOneHandDoubleTap;
        GestureInputListener.Instance.OneHandManipStart += OnOneHandManipStart;
        GestureInputListener.Instance.OneHandManipUpdate += OnOneHandManipUpdate;
        GestureInputListener.Instance.OneHandManipEnd += OnOneHandManipEnd;
        GestureInputListener.Instance.TwoHandManipStart += OnTwoHandManipStart;
        GestureInputListener.Instance.TwoHandManipUpdate += OnTwoHandManipUpdate;
        GestureInputListener.Instance.TwoHandManipEnd += OnTwoHandManipEnd;
    }

    public abstract void OnSpeechResponse(SpeechInputEventArgs eventArgs);
    public abstract void OnOneHandTap(GestureInputEventArgs eventArgs);
    public abstract void OnOneHandDoubleTap(GestureInputEventArgs eventArgs);
    public abstract void OnOneHandManipStart(GestureInputEventArgs eventArgs);
    public abstract void OnOneHandManipUpdate(GestureInputEventArgs eventArgs);
    public abstract void OnOneHandManipEnd(GestureInputEventArgs eventArgs);
    public abstract void OnTwoHandManipStart(GestureInputEventArgs eventArgs);
    public abstract void OnTwoHandManipUpdate(GestureInputEventArgs eventArgs);
    public abstract void OnTwoHandManipEnd(GestureInputEventArgs eventArgs);
}
