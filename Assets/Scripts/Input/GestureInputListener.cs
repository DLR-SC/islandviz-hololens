using HoloIslandVis;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Utility;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis.Input
{
    public enum GestureType : byte
    {
        OneHandTap = 1,
        TwoHandTap = 2,
        OneHandDoubleTap = 4,
        TwoHandDoubleTap = 8,
        OneHandManipStart = 16,
        TwoHandManipStart = 32,
        ManipulationUpdate = 64,
        ManipulationEnd = 128
    }

    public class GestureInputListener : SingletonComponent<GestureInputListener>, 
        IInputHandler, ISourceStateHandler
    {
        public delegate void GestureInputHandler(GestureInputEventArgs eventData);

        public event GestureInputHandler OneHandTap             = delegate { };
        public event GestureInputHandler TwoHandTap             = delegate { };
        public event GestureInputHandler OneHandDoubleTap       = delegate { };
        public event GestureInputHandler TwoHandDoubleTap       = delegate { };

        public event GestureInputHandler OneHandManipStart      = delegate { };
        public event GestureInputHandler TwoHandManipStart      = delegate { };
        public event GestureInputHandler ManipulationUpdate     = delegate { };
        public event GestureInputHandler ManipulationEnd        = delegate { };

        private Dictionary<uint, GestureSource> _gestureSources;
        private Dictionary<short, GestureType> _gestureTypeTable;
        private Dictionary<GestureType, Action<GestureInputEventArgs>> _gestureEventTable;
        private int _inputTimeout;
        private bool _timerSet;

        protected override void Awake()
        {
            base.Awake();
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
            InputManager.Instance.AddGlobalListener(gameObject);
            _inputTimeout = 250;
            _timerSet = false;
        }

        private void Update()
        {
            int sourcesManipulating = 0;
            foreach(GestureSource source in _gestureSources.Values)
            {
                if (source.IsManipulating && !source.IsEvaluating)
                    sourcesManipulating++;
            }

            if (sourcesManipulating != 0)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                _gestureSources.Values.CopyTo(gestureSources, 0);

                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    ManipulationUpdate(new GestureInputEventArgs(GestureType.ManipulationUpdate, gestureSources)));
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if (_gestureSources.ContainsKey(eventData.SourceId))
                return;

            _gestureSources.Add(eventData.SourceId, new GestureSource(inputSource, eventData.SourceId));
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            if (_gestureSources[eventData.SourceId].IsManipulating)
            {
                GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                _gestureSources.Values.CopyTo(gestureSources, 0);

                UnityMainThreadDispatcher.Instance.Enqueue(() => 
                    ManipulationEnd(new GestureInputEventArgs(GestureType.ManipulationEnd, gestureSources)));
            }

            _gestureSources.Remove(eventData.SourceId);
        }

        public void OnInputDown(InputEventData eventData)
        {
            if(!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            _gestureSources[eventData.SourceId].InputDown++;

            if (!_timerSet)
            {
                _timerSet = true;
                new Task(() => processInput(eventData)).Start();
            }
        }

        public void OnInputUp(InputEventData eventData)
        {
            if(!_gestureSources.ContainsKey(eventData.SourceId))
                return;

            _gestureSources[eventData.SourceId].InputUp++;

            if(!_timerSet)
            {
                _timerSet = true;
                new Task(() => processInput(eventData)).Start();
            }
        }

        public void ResetSubscriptions()
        {
            ManipulationUpdate = delegate { };
            ManipulationEnd = delegate { };
        }

        public void InvokeGestureInputEvent(GestureInputEventArgs eventArgs)
        {
            Action<GestureInputEventArgs> action = _gestureEventTable[eventArgs.GestureType];
            UnityMainThreadDispatcher.Instance.Enqueue(action, eventArgs);
        }

        private async void processInput(InputEventData eventData)
        {
            await Task.Delay(_gestureSources[eventData.SourceId].InputTimeout);

            GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
            _gestureSources.Values.CopyTo(gestureSources, 0);

            short inputData = 0;
            for(int i = 0; i < gestureSources.Length; i++)
                inputData += (short) (gestureSources[i].Evaluate() << i * 8);

            _timerSet = false;
            GestureType gestureType;

            if(_gestureTypeTable.TryGetValue(inputData, out gestureType))
            {
                GestureInputEventArgs eventArgs = new GestureInputEventArgs(gestureType, gestureSources);
                InvokeGestureInputEvent(eventArgs);
            }
        }
    }
}