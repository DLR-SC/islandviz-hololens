using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public enum GestureType : byte
    {
        None = 0,
        OneHandTap = 1,
        TwoHandTap = 2,
        OneHandDoubleTap = 4,
        TwoHandDoubleTap = 8,
        OneHandManipStart = 16,
        TwoHandManipStart = 32,
        ManipulationUpdate = 64,
        ManipulationEnd = 128,
        Invariant = 255
    }

    public enum KeywordType : byte
    {
        None = 0,
        Find = 1,
        Show = 2,
        Hide = 4,
        Invariant = 255
    }

    public enum InteractableType : byte
    {
        None = 0,
        Island = 1,
        ExportDock = 2,
        ImportDock = 4,
        Invariant = 255
    }

    public struct Command : IEquatable<Command>
    {
        public GestureType Gesture { get; set; }
        public KeywordType Keyword { get; set; }
        public InteractableType FocusedObject { get; set; }

        public Command(GestureType gesture)
            : this(gesture, KeywordType.Invariant) { }

        public Command(GestureType gesture, KeywordType keyword)
            : this(gesture, keyword, InteractableType.Invariant) { }

        public Command(GestureType gesture, KeywordType keyword, InteractableType focusedObject)
        {
            Gesture = gesture;
            Keyword = keyword;
            FocusedObject = focusedObject;
        }

        public bool Equals(Command other)
        {
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

        public override int GetHashCode()
        {
            // TODO     Better solution?
            return 0;
        }
    }
}
