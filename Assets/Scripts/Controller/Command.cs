using HoloIslandVis.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Controller
{
    public class Command : IEquatable<Command>
    {
        public GestureType Gesture { get; set; }
        public KeywordType Keyword { get; set; }
        public InteractableType Focused { get; set; }
        public InteractableType Selected { get; set; }
        public StaticItem Item { get; set; }

        public Command() { }

        public Command(GestureType gesture)
            : this(gesture, KeywordType.Invariant) { }

        public Command(GestureType gesture, KeywordType keyword)
            : this(gesture, keyword, InteractableType.Invariant) { }

        public Command(GestureType gesture, KeywordType keyword, InteractableType focused)
            : this(gesture,
                   keyword,
                   focused,
                   InteractableType.Invariant,
                   StaticItem.Invariant)
        { }

        public Command(GestureType gesture, KeywordType keyword, InteractableType focused, InteractableType selected)
            : this(gesture,
                   keyword,
                   focused,
                   selected,
                   StaticItem.Invariant)
        { }

        public Command(StaticItem item)
            : this(GestureType.OneHandTap,
                   KeywordType.Invariant,
                   InteractableType.Widget,
                   InteractableType.Invariant,
                   item)
        { }

        public Command(GestureType gesture,
                       KeywordType keyword,
                       InteractableType focused,
                       InteractableType selected,
                       StaticItem item)
        {
            Gesture = gesture;
            Keyword = keyword;
            Focused = focused;
            Selected = selected;
            Item = item;
        }

        public Command(GestureInteractionEventArgs eventArgs)
        {
            Gesture = eventArgs.Gesture;
            Keyword = KeywordType.None;
            Selected = eventArgs.Selected.Type;
            Focused = eventArgs.Focused.Type;
            Item = StaticItem.None;

            if (Focused == InteractableType.Widget)
                Item = eventArgs.Focused.GetComponent<StaticInteractable>().Item;
        }

        public Command(SpeechInteractionEventArgs eventArgs)
        {
            Gesture = GestureType.None;
            Keyword = eventArgs.Keyword;
            Selected = eventArgs.Selected.Type;
            Focused = eventArgs.Focused.Type;
            Item = StaticItem.None;

            if (Focused == InteractableType.Widget)
                Item = eventArgs.Focused.GetComponent<StaticInteractable>().Item;
        }

        public bool Equals(Command other)
        {
            if (other.Gesture != Gesture && GestureType.Invariant != Gesture)
                return false;

            if (other.Keyword != Keyword && KeywordType.Invariant != Keyword)
                return false;

            if (other.Focused != Focused && InteractableType.Invariant != Focused)
                return false;

            if (other.Selected != Selected && InteractableType.Invariant != Selected)
                return false;

            if (other.Item != Item && StaticItem.Invariant != Item)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            // TODO     Better solution?
            return 0;
        }
    }
}
