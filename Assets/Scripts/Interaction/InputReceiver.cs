using HoloIslandVis.Automaton;
using HoloIslandVis.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputReceiver
{
    public InputReceiver()
    {
        GestureInputListener.Instance.OneHandTap += OnOneHandTap;
        GestureInputListener.Instance.OneHandDoubleTap += OnOneHandDoubleTap;
    }

    public abstract void OnOneHandTap(GestureInputEventArgs eventArgs);
    public abstract void OnOneHandDoubleTap(GestureInputEventArgs eventArgs);
}
