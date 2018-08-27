using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public enum GestureType
    {
        Invariant = 0,
        OneHandTap = 1,
        OneHandDoubleTap = 2,
        ManipulationStart = 4,
        ManipulationEnd = 8
    }

    public enum KeywordType
    {
        Invariant = 0
    }

    public enum InteractableType
    {
        Invariant = 0
    }

    public class Command
    {
        public GestureType Gesture { get; private set; }
        public KeywordType Keyword { get; private set; }
        public InteractableType FocusedObject { get; private set; }

        public Command()
        {

        }

        public Command(GestureType gesture, KeywordType keyword, InteractableType focusedObject)
        {
            Gesture = gesture;
            Keyword = keyword;
            FocusedObject = focusedObject;
        }

        public override bool Equals(object obj)
        {
            if(!GetType().Equals(obj.GetType()))
                return false;

            Command other = (Command) obj;

            if (Gesture != other.Gesture && other.Gesture != GestureType.Invariant && Gesture != GestureType.Invariant)
                return false;

            if (Keyword != other.Keyword && other.Keyword != KeywordType.Invariant && Keyword != KeywordType.Invariant)
                return false;

            if (FocusedObject != other.FocusedObject && other.FocusedObject != InteractableType.Invariant 
                && FocusedObject != InteractableType.Invariant)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            // TODO     Implement!
            return 0;
        }
    }
}
