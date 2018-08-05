using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public class StateMachine
    {
        private BaseState _currentState;
        private Dictionary<string, BaseState> _stateTable;

        public BaseState CurrentState {
            get { return _currentState; }
        }

        public bool IsInitialized {
            get {
                if(_currentState == null)
                    return false;

                return true;
            }
        } 

        public StateMachine() 
            : this(null) { }

        public StateMachine(BaseState state)
        {
            _stateTable = new Dictionary<string, BaseState>();

            if(state != null)
                Init(state);
        }

        public void IssueCommand(Command command)
        {
            if(!IsInitialized)
                return;

            _currentState.ProcessCommand(command);
        }

        public void Init(BaseState state)
        {
            if(!IsInitialized)
            {
                AddState(state);
                _currentState = state;
            }
        }

        public void AddState(BaseState state)
        {
            state.StateClosed += onStateClosed;
            state.StateOpened += onStateOpened;
            _stateTable.Add(state.Name, state);
        }

        private void onStateClosed(object state, EventArgs args)
        {
            // TODO   Notify user here if new state is not contained
            //        in state table.

            state = (BaseState) state;
            if(state == _currentState)
                _currentState = null;
        }

        private void onStateOpened(object state, EventArgs args)
        {
            if(state != _currentState)
                _currentState = (BaseState) state;
        }
    }
}
