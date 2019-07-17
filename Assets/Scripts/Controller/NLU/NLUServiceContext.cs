using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Controller.NLU
{
    public class NLUServiceContext
    {
        public string Focused { get; private set; }
        public string FocusedType { get; private set; }
        public string Selected { get; private set; }
        public string SelectedType { get; private set; }
        public string GestureType { get; private set; }

        public NLUServiceContext(string focused, string focusedType, string selected, string selectedType, string gestureType)
        {
            Focused = focused;
            FocusedType = focusedType;
            Selected = selected;
            SelectedType = SelectedType;
            GestureType = gestureType;
        }
    }
}
