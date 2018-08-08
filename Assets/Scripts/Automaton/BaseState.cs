using HoloIslandVis.Interaction.Task;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public class BaseState
    {
        public event EventHandler StateOpened = delegate { };
        public event EventHandler StateClosed = delegate { };

        private Action<BaseState> _openAction = delegate { };
        private Action<BaseState> _closeAction = delegate { };

        private string _stateName;
        private Dictionary<Command, BaseState> _stateTransitionTable;
        private Dictionary<Command, InteractionTask> _interactionTaskTable;

        public string Name {
            get { return _stateName; }
        }

        public BaseState(string stateName)
        {
            _stateName = stateName;
            _stateTransitionTable = new Dictionary<Command, BaseState>();
            _interactionTaskTable = new Dictionary<Command, InteractionTask>();

            // This may cause problems because call order of methods causes state to be null
            // before subsequent methods are invoked.
            _openAction += (BaseState state) => StateOpened(state, EventArgs.Empty);
            _closeAction += (BaseState state) => StateClosed(state, EventArgs.Empty);
        }

        public void AddStateTransition(Command command, BaseState state)
            => _stateTransitionTable.Add(command, state);

        public void AddInteractionTask(Command command, InteractionTask task)
            => _interactionTaskTable.Add(command, task);

        public bool ProcessCommand(Command command)
        {
            if (_stateTransitionTable.ContainsKey(command))
            {
                processStateTransition(command);
                return true;
            }
            else if (_interactionTaskTable.ContainsKey(command))
            {
                processInteractionTask(command);
                return true;
            }

            // Command not found.
            return false;
        }

        public void InitState()
            => _openAction(this);

        private void processStateTransition(Command command)
        {
            BaseState newState = _stateTransitionTable[command];
            moveNext(newState);
        }

        private void processInteractionTask(Command command)
        {
            InteractionTask interactionTask = _interactionTaskTable[command];
            //interactionTask.Perform()
        }

        private void moveNext(BaseState newState)
        {
            _closeAction(this);
            newState.InitState();
        }
    }
}
