using HoloToolkit.Unity.InputModule;
using HoloIslandVis.Input;
using HoloIslandVis.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public class StateMachine : InputReceiver
    {
        private Dictionary<string, State> _stateTable;

        public State CurrentState { get; private set; }

        public bool IsInitialized {
            get {
                if(CurrentState == null)
                    return false;

                return true;
            }
        } 

        public StateMachine() 
            : this(null) { }

        public StateMachine(State state)
        {
            _stateTable = new Dictionary<string, State>();

            if(state != null)
                Init(state);
        }

        public void IssueCommand(GestureInputEventArgs eventArgs, Command command)
        {
            if(!IsInitialized)
                return;

            CurrentState.ProcessCommand(eventArgs, command);
        }

        public void Init(State state)
        {
            if(!IsInitialized)
            {
                if(!_stateTable.ContainsKey(state.Name))
                    AddState(state);

                state.InitState();
                CurrentState = state;
            }
        }

        public void AddState(State state)
        {
            state.AddOpenAction((State value) => OnStateOpened(value));
            state.AddCloseAction((State value) => OnStateClosed(value));
            _stateTable.Add(state.Name, state);
        }

        public void OnStateClosed(State state)
        {
            // TODO   Notify user here if new state is not contained
            //        in state table.

            if(state == CurrentState)
                CurrentState = null;
        }

        public void OnStateOpened(State state)
        {
            if(state != CurrentState)
                CurrentState = state;
        }

        public override void OnOneHandTap(GestureInputEventArgs eventArgs)
            => ProcessGestureInputEvent(eventArgs);

        public override void OnOneHandDoubleTap(GestureInputEventArgs eventArgs)
            => ProcessGestureInputEvent(eventArgs);

        public override void OnOneHandManipStart(GestureInputEventArgs eventArgs)
            => ProcessGestureInputEvent(eventArgs);

        public override void OnTwoHandManipStart(GestureInputEventArgs eventArgs)
            => ProcessGestureInputEvent(eventArgs);

        public override void OnManipulationUpdate(GestureInputEventArgs eventArgs)
            => ProcessGestureInputEvent(eventArgs);

        public override void OnManipulationEnd(GestureInputEventArgs eventArgs)
            => ProcessGestureInputEvent(eventArgs);

        public override void OnSpeechResponse(SpeechInputEventArgs eventArgs)
        {
            KeywordType kt; 
            switch(eventArgs.intention)
            {
                case "find_entity_by_name_or_id":
                    kt = KeywordType.Find;
                    break;
                case "show_entity":
                    kt = KeywordType.Show;
                    break;
                case "hide_entity":
                    kt = KeywordType.Hide;
                    break;
                default:
                    kt = KeywordType.Invariant;
                    break;
            }

            if (CurrentState != null)
                CurrentState.ProcessCommand(eventArgs, new Command(GestureType.Invariant, kt, InteractableType.Invariant));
        }

        private void ProcessGestureInputEvent(GestureInputEventArgs eventArgs)
        {
            byte gestureType = (byte)eventArgs.GestureType;
            Command command = new Command((GestureType)gestureType);
            GameObject focusedObject = RuntimeCache.Instance.CurrentFocus;
            Interactable interactable = null;

            if(eventArgs.Target != null)
                interactable = eventArgs.Target.GetComponent<Interactable>();
            else if(focusedObject != null)
                interactable = focusedObject.GetComponent<Interactable>();

            if (interactable != null)
                command.FocusedObject = interactable.InteractableType;

            if (CurrentState != null)
                CurrentState.ProcessCommand(eventArgs, command);
        }
    }
}
