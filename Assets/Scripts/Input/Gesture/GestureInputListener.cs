using HoloIslandVis.Core;
using HoloIslandVis.Utilities;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Input.Gesture
{
    // The GestureInputListener is responsible managing active gesture sources, evaluating
    // gesture inputs and passing the corresponding input events to the InputHandler.
    public class GestureInputListener : SingletonComponent<GestureInputListener>, 
        IInputHandler, ISourceStateHandler
    {
        // Gesture type enum used as key for gesture event table.
        public enum GestureType : byte
        {
            None = 0,
            OneHandTap = 1,
            TwoHandTap = 2,
            OneHandDoubleTap = 4,
            TwoHandDoubleTap = 8,
            OneHandManipStart = 16,
            TwoHandManipStart = 32,
            ManipulationUpdate = 64,
            ManipulationEnd = 128
        }

        // Gesture input events invoked within Action delegates. These
        // Action delegates are the gesture event table's values and are 
        // accessible via the corresponding GestureType key.
        public delegate void GestureInputHandler(GestureInputEventArgs eventData);

        public event GestureInputHandler OneHandTap             = delegate { };
        public event GestureInputHandler TwoHandTap             = delegate { };
        public event GestureInputHandler OneHandDoubleTap       = delegate { };
        public event GestureInputHandler TwoHandDoubleTap       = delegate { };

        public event GestureInputHandler OneHandManipStart      = delegate { };
        public event GestureInputHandler TwoHandManipStart      = delegate { };
        public event GestureInputHandler ManipulationUpdate     = delegate { };
        public event GestureInputHandler ManipulationEnd        = delegate { };

        public bool IsProcessing { get; private set; }
        [Range(0.0f, 1.0f)] public float InputTimeout;

        private Dictionary<uint, GestureSource> _gestureSources;
        private Dictionary<short, GestureType> _gestureTypeTable;
        private Dictionary<GestureType, Action<GestureInputEventArgs>> _gestureEventTable;
        private bool _timerSet;

        void Start()
        {
            IsProcessing = false;

            // Map the gesture source state (number of presses and releases, whether it
            // is manipulating) to a particular gesture type. The most significant byte corresponds
            // to the second present gesture source, while the least siginificant byte 
            // corresponds to the first present gesture source.
            _gestureTypeTable = new Dictionary<short, GestureType>()
            {
                { Convert.ToInt16("0000000010001001", 2), GestureType.OneHandTap },
                { Convert.ToInt16("1000100110001001", 2), GestureType.TwoHandTap },
                { Convert.ToInt16("0000000010010010", 2), GestureType.OneHandDoubleTap },
                { Convert.ToInt16("1001001010010010", 2), GestureType.TwoHandDoubleTap },
                { Convert.ToInt16("0000000011000001", 2), GestureType.OneHandManipStart },
                { Convert.ToInt16("1100000111000001", 2), GestureType.TwoHandManipStart },
                { Convert.ToInt16("0000000010001000", 2), GestureType.ManipulationEnd },
                { Convert.ToInt16("1000100010001000", 2), GestureType.ManipulationEnd },
                { Convert.ToInt16("1000000010001000", 2), GestureType.ManipulationEnd },
                { Convert.ToInt16("1000100010000000", 2), GestureType.ManipulationEnd }
            };

            // Map a gesture type to an Action invoking a gesture input event. This action
            // is then passed to the InputHandler for invocation.
            _gestureEventTable = new Dictionary<GestureType, Action<GestureInputEventArgs>>()
            {
                { GestureType.OneHandTap            , eventArgs => OneHandTap(eventArgs) },
                { GestureType.TwoHandTap            , eventArgs => TwoHandTap(eventArgs) },
                { GestureType.OneHandDoubleTap      , eventArgs => OneHandDoubleTap(eventArgs) },
                { GestureType.TwoHandDoubleTap      , eventArgs => TwoHandDoubleTap(eventArgs) },
                { GestureType.OneHandManipStart     , eventArgs => OneHandManipStart(eventArgs) },
                { GestureType.TwoHandManipStart     , eventArgs => TwoHandManipStart(eventArgs) },
                { GestureType.ManipulationUpdate    , eventArgs => ManipulationEnd(eventArgs) },
                { GestureType.ManipulationEnd       , eventArgs => ManipulationEnd(eventArgs) }
            };

            _gestureSources = new Dictionary<uint, GestureSource>(2);

            // Add GestureInputListener as global listener to the HoloToolkit input manager
            // in order for it to receive source state and input up/down events.
            InputManager.Instance.AddGlobalListener(gameObject);
            _timerSet = false;
        }

        private void Update()
        {
            int sourcesManipulating = 0;

            DebugLog.Instance.SetText("Gesture sources present: " + _gestureSources.Count);

            // Check whether any sources are currently in a manipulating state.
            foreach(GestureSource source in _gestureSources.Values)
            {
                if (source.IsManipulating && !source.IsEvaluating)
                    sourcesManipulating++;
            }

            // If there is any gesture source currently manipulating, invoke a
            // ManipulationUpdate event.
            if (sourcesManipulating != 0)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                _gestureSources.Values.CopyTo(gestureSources, 0);

                var eventArgs = new GestureInputEventArgs(gestureSources);
                ManipulationUpdate(eventArgs);
            }
        }

        // Add gesture source to gesture source dictionary when detected.
        public void OnSourceDetected(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;

            // If dictionary already contains source with this id, return.
            if (_gestureSources.ContainsKey(eventData.SourceId))
                return;

            var gestureSource = new GestureSource(inputSource, eventData.SourceId);
            _gestureSources.Add(eventData.SourceId, gestureSource);
        }

        // Remove gesture source from gesture source dictionary when lost.
        public void OnSourceLost(SourceStateEventData eventData)
        {
            // If dictionary contains no source with this id, return.
            if (!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            // If the lost gesture source was currently manipulating invoke a 
            // ManipulationEnd event.
            if (_gestureSources[eventData.SourceId].IsManipulating)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                _gestureSources.Values.CopyTo(gestureSources, 0);

                var eventArgs = new GestureInputEventArgs(gestureSources);
                ManipulationEnd(eventArgs);
            }

            _gestureSources.Remove(eventData.SourceId);
        }

        public void OnInputDown(InputEventData eventData)
        {
            // If dictionary contains no source with this id, return.
            if (!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            // Increment number of times this gesture source was pressed.
            _gestureSources[eventData.SourceId].InputDown++;

            // If gesture input listener is not currently running the coroutine
            // ProcessInput, invoke it.
            if (!_timerSet)
            {
                _timerSet = true;
                StartCoroutine(ProcessInput(eventData));
            }
        }

        public void OnInputUp(InputEventData eventData)
        {
            // If dictionary contains no source with this id, return.
            if (!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            // Increment number of times this gesture source was released.
            _gestureSources[eventData.SourceId].InputUp++;

            // If gesture input listener is not currently processing any 
            // input, start processing.
            if (!_timerSet)
            {
                _timerSet = true;
                StartCoroutine(ProcessInput(eventData));
            }
        }

        private void ResetSubscriptions()
        {
            ManipulationUpdate = delegate { };
            ManipulationEnd = delegate { };
        }

        // This coroutine is invoked anytime an InputUp or InputDown event is received.
        // After it has been invoked, it waits for the duration set as InputTimeout until 
        // evaluating the input. During this time, additional InputUp and InputDown events
        // can be received and accumulated. For example, two InputDown and two InputUp 
        // events received for one gesture source during the timeout period will be evaluated 
        // to a OneHandDoubleTap event.
        private IEnumerator ProcessInput(InputEventData eventData)
        {
            IsProcessing = true;

            // Wait and accumulate additional InputDown and InputUp events.
            yield return new WaitForSeconds(InputTimeout);

            // Get all present gesture sources.
            GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
            _gestureSources.Values.CopyTo(gestureSources, 0);

            // Evaluate the InputUp and InputDown count for each gesture source and
            // encode the input data into a short. Afterwards, reset input sources.
            short inputData = 0;
            for(int i = 0; i < gestureSources.Length; i++)
            {
                inputData += (short)(gestureSources[i].Evaluate() << i * 8);
                gestureSources[i].ResetInputData();
            }

            // Unset the timer, so the GestureInputListener can process new input events.
            _timerSet = false;
            GestureType gestureType;

            // Check whether the encoded input data corresponds to a gesture. If so, retrieve
            // the corresponding Action delegate from the gesture event table and pass it to
            // the input handler for invocation.
            if (_gestureTypeTable.TryGetValue(inputData, out gestureType))
            {
                GestureInputEventArgs eventArgs = new GestureInputEventArgs(gestureSources);
                Action<GestureInputEventArgs> action = _gestureEventTable[gestureType];
                InputHandler.Instance.InvokeGestureInputEvent(action, eventArgs);
            }

            IsProcessing = false;
        }
    }
}