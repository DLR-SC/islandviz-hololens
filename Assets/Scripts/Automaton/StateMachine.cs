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
        {
            Command command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.None);

            if (RuntimeCache.Instance.CurrentFocus != null)
            {
                if(RuntimeCache.Instance.CurrentFocus.tag == "Untagged")
                    command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Island);
                else if (RuntimeCache.Instance.CurrentFocus.tag == "ExportDock")
                    command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ExportDock);
                else if (RuntimeCache.Instance.CurrentFocus.tag == "ImportDock")
                    command = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ImportDock);
            }

            if (CurrentState != null)
                CurrentState.ProcessCommand(eventArgs, command);
        }

        public override void OnOneHandDoubleTap(GestureInputEventArgs eventArgs)
        {
            Command command = new Command(GestureType.OneHandDoubleTap, KeywordType.Invariant, InteractableType.Invariant);

            if(CurrentState != null)
                CurrentState.ProcessCommand(eventArgs, command);
        }

        public override void OnOneHandManipStart(GestureInputEventArgs eventArgs)
        {
            Command command = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);

            if(CurrentState != null)
                CurrentState.ProcessCommand(eventArgs, command);
        }

        public override void OnTwoHandManipStart(GestureInputEventArgs eventArgs)
        {
            Command command = new Command(GestureType.TwoHandManipStart, KeywordType.Invariant, InteractableType.Invariant);

            if(CurrentState != null)
                CurrentState.ProcessCommand(eventArgs, command);
        }

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
    }
}
