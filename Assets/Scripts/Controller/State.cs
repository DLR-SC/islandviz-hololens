using HoloIslandVis.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HoloIslandVis.Controller
{
    public class State
    {
        private Action<State> _openAction = delegate { };
        private Action<State> _closeAction = delegate { };

        private Dictionary<Command, State> _stateTransitionTable;
        private Dictionary<Command, InteractionTask> _interactionTaskTable;
        private InteractionTask _activeInteraction;

        public string Name { get; private set; }

        public State(string name)
        {
            Name = name;
            _stateTransitionTable = new Dictionary<Command, State>();
            _interactionTaskTable = new Dictionary<Command, InteractionTask>();

            // Add entries for Manipulation Update and Manipulation End.
            // Value "null" is equivalent to there being no currently active
            // interaction task to update or end.
            _interactionTaskTable.Add(new Command(GestureType.ManipulationUpdate), null);
            _interactionTaskTable.Add(new Command(GestureType.ManipulationEnd), null);
        }

        public void AddStateTransition(Command command, State state)
            => _stateTransitionTable.Add(command, state);

        public void AddInteractionTask(Command command, InteractionTask task)
            => _interactionTaskTable.Add(command, task);

        public void AddOpenAction(Action<State> action)
            => _openAction += action;

        public void AddCloseAction(Action<State> action)
            => _closeAction += action;

        public IEnumerator ProcessCommand(InteractionEventArgs eventArgs, Command command)
        {
            if (_stateTransitionTable.ContainsKey(command))
            {
                if (_interactionTaskTable.ContainsKey(command))
                    yield return ProcessInteractionTask(eventArgs, command);

                ProcessStateTransition(eventArgs, command);
                yield break;
            }

            if (_interactionTaskTable.ContainsKey(command))
                yield return ProcessInteractionTask(eventArgs, command);
        }

        public void InitState()
            => _openAction(this);

        private void ProcessStateTransition(InteractionEventArgs eventArgs, Command command)
        {
            State newState = _stateTransitionTable[command];
            MoveNext(newState);
        }

        private IEnumerator ProcessInteractionTask(InteractionEventArgs eventArgs, Command command)
        {
            _activeInteraction = _interactionTaskTable[command];

            if(command.Gesture == GestureType.OneHandManipStart || 
               command.Gesture == GestureType.TwoHandManipStart)
            {
                _interactionTaskTable[new Command(GestureType.ManipulationUpdate)] = _activeInteraction;
                _interactionTaskTable[new Command(GestureType.ManipulationEnd)] = _activeInteraction;
            }

            if (_activeInteraction != null)
                yield return _activeInteraction.Perform(eventArgs);

            if (command.Gesture == GestureType.ManipulationEnd)
            {
                _interactionTaskTable[new Command(GestureType.ManipulationUpdate)] = null;
                _interactionTaskTable[new Command(GestureType.ManipulationEnd)] = null;
                _activeInteraction = null;
            }
        }

        private void MoveNext(State newState)
        {
            _closeAction.Invoke(this);
            newState.InitState();
        }
    }
}
