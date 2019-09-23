using HoloIslandVis.Core;
using HoloIslandVis.Controller.NLU;
using HoloIslandVis.Input;
using HoloIslandVis.Input.Gesture;
using HoloIslandVis.Input.Speech;
using HoloIslandVis.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using HoloIslandVis.Sharing;
using UnityEngine.UI;

namespace HoloIslandVis.Controller
{
    public class StateManager : SingletonComponent<StateManager>, IInputReceiver
    {
        private Dictionary<string, State> _stateTable;
        public bool _isProcessing;

        public AppConfig AppConfig;
        public ContextManager ContextManager;
        public NLUServiceClient NLUServiceClient;

        public State CurrentState { get; private set; }

        public bool Initialized {
            get {
                if(CurrentState == null)
                    return false;

                return true;
            }
        } 

        protected override void Awake()
        {
            base.Awake();
            _stateTable = new Dictionary<string, State>();
        }

        public IEnumerator IssueCommand(InteractionEventArgs eventArgs, Command command)
        {
            if (!Initialized)
                yield break;

            yield return CurrentState.ProcessCommand(eventArgs, command);
        }

        public void Init(State state)
        {
            if(!Initialized)
            {
                this.SubscribeToInputEvents();
                if (!_stateTable.ContainsKey(state.Name))
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

        public void OnOneHandTap(GestureInputEventArgs inputData)
            => StartCoroutine(ProcessGestureInputEvent(inputData, GestureType.OneHandTap));

        public void OnOneHandDoubleTap(GestureInputEventArgs inputData)
            => StartCoroutine(ProcessGestureInputEvent(inputData, GestureType.OneHandDoubleTap));

        public void OnOneHandManipStart(GestureInputEventArgs inputData)
            => StartCoroutine(ProcessGestureInputEvent(inputData, GestureType.OneHandManipStart));

        public void OnTwoHandManipStart(GestureInputEventArgs inputData)
            => StartCoroutine(ProcessGestureInputEvent(inputData, GestureType.TwoHandManipStart));

        public void OnManipulationUpdate(GestureInputEventArgs inputData)
            => StartCoroutine(ProcessGestureInputEvent(inputData, GestureType.ManipulationUpdate));

        public void OnManipulationEnd(GestureInputEventArgs inputData)
            => StartCoroutine(ProcessGestureInputEvent(inputData, GestureType.ManipulationEnd));

        public void OnSpeechInputEvent(SpeechInputEventArgs inputData)
            => StartCoroutine(ProcessSpeechInput(inputData));

        private IEnumerator ProcessGestureInputEvent(GestureInputEventArgs inputData, GestureType gesture)
        {
            if (_isProcessing && gesture != GestureType.ManipulationEnd)
                yield break;

            if (_isProcessing && gesture == GestureType.ManipulationEnd)
                yield return null;

            _isProcessing = true;

            Context context = ContextManager.Instance.SafeContext;
            var eventArgs = new GestureInteractionEventArgs(inputData);
            eventArgs.Selected = context.Selected;
            eventArgs.Focused = context.Focused;
            eventArgs.Gesture = gesture;

            Command command = new Command(eventArgs);

            if(AppConfig.SharingEnabled && AppConfig.IsServerInstance)
            {
                if(gesture == GestureType.OneHandManipStart ||
                    gesture == GestureType.TwoHandManipStart ||
                    gesture == GestureType.ManipulationUpdate)
                {
                    // Do nothing.
                }
                else
                {
                    string sendCommand = ParseGestureCommand(command, eventArgs);
                    SendCommandToRemote(sendCommand);
                }
            }

            if (CurrentState != null)
                yield return CurrentState.ProcessCommand(eventArgs, command);

            _isProcessing = false;
        }

        private IEnumerator ProcessSpeechInput(SpeechInputEventArgs inputData)
        {
            Context context = ContextManager.Instance.SafeContext;
            var eventArgs = new SpeechInteractionEventArgs(inputData);

            if (eventArgs.Keyword == Interaction.KeywordType.None)
                yield break;

            eventArgs.Selected = context.Selected;
            eventArgs.Focused = context.Focused;

            Command command = new Command(eventArgs);

            if (AppConfig.SharingEnabled && AppConfig.IsServerInstance)
            {
                string sendCommand = ParseSpeechCommand(command, eventArgs);
                SendCommandToRemote(sendCommand);
            }

            if (CurrentState != null)
                yield return CurrentState.ProcessCommand(eventArgs, command);

            yield break;
        }

        private string ParseGestureCommand(Command command, GestureInteractionEventArgs eventArgs)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("GESTURE_COMMAND ");
            builder.Append(command.Gesture.ToString() + " ");
            builder.Append(command.Keyword.ToString() + " ");
            builder.Append(command.Focused.ToString() + " ");
            builder.Append(command.Selected.ToString() + " ");
            builder.Append(command.Item.ToString() + " ");

            builder.Append(eventArgs.IsTwoHanded + " ");
            builder.Append(eventArgs.HandOnePos.x + " ");
            builder.Append(eventArgs.HandOnePos.y + " ");
            builder.Append(eventArgs.HandOnePos.z + " ");

            if (eventArgs.IsTwoHanded)
            {
                builder.Append(eventArgs.HandTwoPos.x + " ");
                builder.Append(eventArgs.HandTwoPos.y + " ");
                builder.Append(eventArgs.HandTwoPos.z + " ");
            }

            builder.Append(eventArgs.Gesture + " ");
            builder.Append(eventArgs.Focused.name.Replace(' ', '_') + " ");
            builder.Append(eventArgs.Selected.name.Replace(' ', '_'));

            return builder.ToString();
        }

        private string ParseSpeechCommand(Command command, SpeechInteractionEventArgs eventArgs)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("VOICE_COMMAND ");
            builder.Append(command.Gesture.ToString() + " ");
            builder.Append(command.Keyword.ToString() + " ");
            builder.Append(command.Focused.ToString() + " ");
            builder.Append(command.Selected.ToString() + " ");
            builder.Append(command.Item.ToString() + " ");

            builder.Append(eventArgs.Keyword + " ");
            builder.Append(eventArgs.Input.Replace(' ', '_') + " ");
            builder.Append(eventArgs.Focused.name.Replace(' ', '_') + " ");
            builder.Append(eventArgs.Selected.name.Replace(' ', '_'));

            builder.Append("//DataPayload//" + eventArgs.Data);

            return builder.ToString();
        }

        private void SendCommandToRemote(string sendCommand)
            => AppStateSynchronizer.Instance.Command.Value = sendCommand;
    }
}
