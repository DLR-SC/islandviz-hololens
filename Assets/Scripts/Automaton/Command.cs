using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public enum GestureType
    {
        Tap = 1,
        DoubleTap = 2,
        ManipulationStart = 4,
        ManipulationEnd = 8
    }

    public enum KeywordType
    {
        Tap = 1,
        DoubleTap = 2,
        ManipulationStart = 4,
        ManipulationEnd = 8
    }

    public enum InteractableType
    {
        Island = 1,
        Panel = 2
    }

    public class Command
    {
        private GestureType _gesture;
        private KeywordType _keyword;
        private InteractableType _focusedObject;

        public Command(GestureType gesture, KeywordType keyword, InteractableType focusedObject)
        {
            _gesture = gesture;
            _keyword = keyword;
            _focusedObject = focusedObject;
        }
    }
}
