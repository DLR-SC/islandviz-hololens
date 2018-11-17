using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public enum GestureType
    {
        None = 0,
        Invariant = 1,
        OneHandTap = 2,
        OneHandDoubleTap = 4,
        OneHandManipStart = 8,
        TwoHandManipStart = 16,
        ManipulationEnd = 32
    }

    public enum KeywordType
    {
        None = 0,
        Invariant = 1,
        Find = 2,
        Show = 4,
        Hide = 8
    }

    public enum InteractableType
    {
        None = 0,
        Invariant = 1,
        Island = 2,
        ExportDock = 4,
        ImportDock = 8
    }

    public struct Command
    {
        public GestureType Gesture { get; private set; }
        public KeywordType Keyword { get; private set; }
        public InteractableType FocusedObject { get; private set; }

        public Command(GestureType gesture, KeywordType keyword, InteractableType focusedObject)
        {
            Gesture = gesture;
            Keyword = keyword;
            FocusedObject = focusedObject;
        }

        public override bool Equals(object obj)
        {
            if (!GetType().Equals(obj.GetType()))
                return false;

            Command other = (Command)obj;

            if (GestureType.Invariant != Gesture &&
                GestureType.Invariant != other.Gesture)
            {
                if (Gesture != other.Gesture)
                    return false;
            }

            if (KeywordType.Invariant != Keyword &&
                KeywordType.Invariant != other.Keyword)
            {
                if (Keyword != other.Keyword)
                    return false;
            }

            if (InteractableType.Invariant != FocusedObject &&
                InteractableType.Invariant != other.FocusedObject)
            {
                if (FocusedObject != other.FocusedObject)
                    return false;
            }

            return true;
        }
    }
}
