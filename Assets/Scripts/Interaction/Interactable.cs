using HoloIslandVis.Controller;
using HoloIslandVis.Sharing;
using HoloIslandVis.UI;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public enum GestureType
    {
        None,
        OneHandTap,
        TwoHandTap,
        OneHandDoubleTap,
        TwoHandDoubleTap,
        OneHandManipStart,
        TwoHandManipStart,
        ManipulationUpdate,
        ManipulationEnd,
        Invariant
    }

    public enum KeywordType
    {
        None,
        Select,
        Deselect,
        Find,
        Show,
        Hide,
        Utter,
        ShowAllNodes,
        Invariant
    }

    public enum InteractableType
    {
        None,
        Bundle,
        Package,
        CompilationUnit,
        ExportDock,
        ImportDock,
        ContentPane,
        Widget,
        Invariant
    }

    public class Interactable : MonoBehaviour, IFocusable
    {
        public Guid guid;

        public Action ObjectBehaviour;
        public Action SelectBehaviour;
        public Action DeselectBehaviour;
        public Action FocusBehaviour;
        public Action DefocusBehaviour;

        public Highlight Highlight;
        public InteractableType Type;
        public bool Selectable;
        public bool Focusable;

        private bool _focused;
        private bool _selected;

        public void Start()
        {
            guid = new Guid();
        }

        // TODO: Decouple Interaction from Controller.

        public void Behaviour()
        {
            if (ObjectBehaviour != null)
                ObjectBehaviour.Invoke();
        }

        public void OnFocusEnter()
        {
            if (Focusable)
            {
                //if(AppStateSynchronizer.Instance.AppConfig.IsServerInstance)
                //    AppStateSynchronizer.Instance.Focused.Value = "FOCUSED " + gameObject.name;

                if (FocusBehaviour != null && !_selected)
                    FocusBehaviour.Invoke();

                _focused = true;
                ContextManager.Instance.Focused = this;
            }
        }

        public void OnFocusExit()
        {
            //if (AppStateSynchronizer.Instance.AppConfig.IsServerInstance)
            //    AppStateSynchronizer.Instance.Focused.Value = "DEFOCUSED " + gameObject.name;

            if (Focusable)
            {
                if (DefocusBehaviour != null && !_selected)
                    DefocusBehaviour.Invoke();

                _focused = false;
                ContextManager.Instance.Focused = null;
            }
        }

        public void OnSelect()
        {
            if (Selectable)
            {
                if (SelectBehaviour != null)
                    SelectBehaviour.Invoke();

                _selected = true;
                ContextManager.Instance.Selected = this;
            }
        }

        public void OnDeselect()
        {
            if (Selectable)
            {
                if (DeselectBehaviour != null)
                    DeselectBehaviour.Invoke();

                _selected = false;
                ContextManager.Instance.Selected = null;
            }
        }
    }
}
