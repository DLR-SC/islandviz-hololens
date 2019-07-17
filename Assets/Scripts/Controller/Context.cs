using HoloIslandVis.Interaction;

namespace HoloIslandVis.Controller
{
    public class Context
    {
        public Interactable Selected { get; private set; }
        public Interactable Focused { get; private set; }
        public Interactable RemoteFocused { get; private set; }

        public InteractableType SelectedType { get; private set; }
        public InteractableType FocusedType { get; private set; }

        public Context(Interactable selected, Interactable focused)
        {
            Selected = selected;
            Focused = focused;

            SelectedType = selected.Type;
            FocusedType = focused.Type;
        }

        public Context(Interactable selected, Interactable focused, Interactable remoteFocused)
        {
            Selected = selected;
            Focused = focused;
            RemoteFocused = remoteFocused;

            SelectedType = selected.Type;
            FocusedType = focused.Type;
        }
    }
}