using HoloIslandVis.Core;
using HoloIslandVis.Input.Gesture;
using HoloIslandVis.Interaction;
using UnityEngine;

namespace HoloIslandVis.Controller
{
    public class ContextManager : SingletonComponent<ContextManager>
    {
        public AppConfig AppConfig;

        public Context SafeContext { get; private set; }
        public Context Context {
            get {
                    if (AppConfig.SharingEnabled)
                        return new Context(_selected, _focused, _remoteFocused);
                    else
                        return new Context(_selected, _focused);
            }
        }

        private Interactable _none;
        private Interactable _selected;
        private Interactable _focused;
        private Interactable _remoteFocused;

        public Interactable Selected {
            set {
                if (value == null)  _selected = _none;
                else                _selected = value;

                if (!GestureInputListener.Instance.IsProcessing)
                {
                    if (AppConfig.SharingEnabled)
                        SafeContext = new Context(_selected, _focused, _remoteFocused);
                    else
                        SafeContext = new Context(_selected, _focused);
                }
            }
        }

        public Interactable Focused {
            set {
                if (value == null)  _focused = _none;
                else                _focused = value;

                if (!GestureInputListener.Instance.IsProcessing)
                {
                    if (AppConfig.SharingEnabled)
                        SafeContext = new Context(_selected, _focused, _remoteFocused);
                    else
                        SafeContext = new Context(_selected, _focused);
                }
            }
        }

        public Interactable RemoteFocused {
            set {
                if (value == null) _remoteFocused = _none;
                else _remoteFocused = value;

                if (!GestureInputListener.Instance.IsProcessing)
                {
                    if (AppConfig.SharingEnabled)
                        SafeContext = new Context(_selected, _focused, _remoteFocused);
                    else
                        SafeContext = new Context(_selected, _focused);
                }
            }
        }

        void Start()
        {
            GameObject none = GameObject.Find("NoneInteractableProxy");
            _none = none.GetComponent<Interactable>();
            _selected = _none;
            _focused = _none;
            _remoteFocused = _none;

            if(AppConfig.SharingEnabled)
                SafeContext = new Context(_selected, _focused, _remoteFocused);
            else
                SafeContext = new Context(_selected, _focused);
        }
    }
}
