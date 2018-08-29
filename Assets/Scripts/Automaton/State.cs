using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Automaton
{
    public class State
    {
        private Action<State> _openAction   = delegate { };
        private Action<State> _closeAction  = delegate { };

        private Dictionary<Command, State> _stateTransitionTable;
        private Dictionary<Command, InteractionTask> _interactionTaskTable;

        public string Name { get; private set; }

        public State(string name)
        {
            Name = name;
            _stateTransitionTable = new Dictionary<Command, State>();
            _interactionTaskTable = new Dictionary<Command, InteractionTask>();

            _closeAction += new Action<State>((State value) => {
                foreach (Delegate del in _openAction.GetInvocationList()) {
                    _openAction -= (Action<State>) del;
                }

                foreach (Delegate del in _openAction.GetInvocationList())
                {
                    _closeAction -= (Action<State>) del;
                }
            });
        }

        public void AddStateTransition(Command command, State state)
            => _stateTransitionTable.Add(command, state);

        public void AddInteractionTask(Command command, InteractionTask task)
            => _interactionTaskTable.Add(command, task);

        public void AddOpenAction(Action<State> action)
            => _openAction += action;

        public void AddCloseAction(Action<State> action)
            => _closeAction += action;

        public bool ProcessCommand(GestureInputEventArgs eventArgs, Command command)
        {
            if (_stateTransitionTable.ContainsKey(command))
            {
                processStateTransition(eventArgs, command);
                return true;
            }
            else if (_interactionTaskTable.ContainsKey(command))
            {
                processInteractionTask(eventArgs, command);
                return true;
            }

            // Command not found.
            return false;
        }

        public void InitState()
            => _openAction(this);

        private void processStateTransition(GestureInputEventArgs eventArgs, Command command)
        {
            State newState = _stateTransitionTable[command];
            moveNext(newState);
        }

        private void processInteractionTask(GestureInputEventArgs eventArgs, Command command)
        {
            InteractionTask interactionTask = _interactionTaskTable[command];
            interactionTask.Pass(eventArgs);
        }

        private void moveNext(State newState)
        {
            _closeAction(this);
            newState.InitState();
        }
    }
}
