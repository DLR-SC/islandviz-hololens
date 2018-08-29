using HoloIslandVis;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
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

        private Dictionary<IInputSource, GestureSource> _gestureSources;
        private Dictionary<short, Action<GestureInputEventArgs>> _gestureEventTable;
        private int _inputTimeout;
        private bool _timerSet;

        protected override void Awake()
        {
            base.Awake();
            _gestureEventTable = new Dictionary<short, Action<GestureInputEventArgs>>()
            {
                { Convert.ToInt16("0000000010001001", 2), eventArgs => OneHandTap(eventArgs) },
                { Convert.ToInt16("1000100110001001", 2), eventArgs => TwoHandTap(eventArgs) },
                { Convert.ToInt16("0000000010010010", 2), eventArgs => OneHandDoubleTap(eventArgs) },
                { Convert.ToInt16("1001001010010010", 2), eventArgs => TwoHandDoubleTap(eventArgs) },
                { Convert.ToInt16("0000000011000001", 2), eventArgs => OneHandManipStart(eventArgs) },
                { Convert.ToInt16("1100000111000001", 2), eventArgs => TwoHandManipStart(eventArgs) },
                { Convert.ToInt16("0000000010001000", 2), eventArgs => ManipulationEnd(eventArgs) },
                { Convert.ToInt16("1000100010001000", 2), eventArgs => ManipulationEnd(eventArgs) },
                { Convert.ToInt16("1000000010001000", 2), eventArgs => ManipulationEnd(eventArgs) },
                { Convert.ToInt16("1000100010000000", 2), eventArgs => ManipulationEnd(eventArgs) }
            };

            _gestureSources = new Dictionary<IInputSource, GestureSource>(2);
            InputManager.Instance.AddGlobalListener(gameObject);
            _inputTimeout = 250;
            _timerSet = false;
        }

        private void Update()
        {
            foreach(GestureSource source in _gestureSources.Values)
            {
                if(source.IsManipulating && !source.IsEvaluating)
                {
                    GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
                    short gestureUpdate = Convert.ToInt16("1111111111111111", 2);
                    _gestureSources.Values.CopyTo(gestureSources, 0);

                    ManipulationUpdate(new GestureInputEventArgs(gestureUpdate, gestureSources));
                }
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if (_gestureSources.ContainsKey(inputSource))
                return;

            _gestureSources.Add(inputSource, new GestureSource(inputSource));
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if (!_gestureSources.ContainsKey(inputSource))
                return;

            _gestureSources.Remove(inputSource);
        }

        public void OnInputDown(InputEventData eventData)
        {
            _gestureSources[eventData.InputSource].InputDown++;

            if (!_timerSet)
            {
                _timerSet = true;
                new Task(() => processInput(eventData)).Start();
            }
        }

        public void OnInputUp(InputEventData eventData)
        {
            _gestureSources[eventData.InputSource].InputUp++;

            if (!_timerSet)
            {
                _timerSet = true;
                new Task(() => processInput(eventData)).Start();
            }
        }

        private async void processInput(InputEventData eventData)
        {
            await Task.Delay(_gestureSources[eventData.InputSource].InputTimeout);

            GestureSource[] gestureSources = new GestureSource[_gestureSources.Count];
            _gestureSources.Values.CopyTo(gestureSources, 0);

            short inputData = 0;
            for (int i = 0; i < gestureSources.Length; i++)
                inputData += (short) (gestureSources[i].Evaluate() << i * 8);

            _timerSet = false;
            Action<GestureInputEventArgs> action;
            if(_gestureEventTable.TryGetValue(inputData, out action))
            {
                GestureInputEventArgs eventArgs = new GestureInputEventArgs(inputData, gestureSources);
                UnityMainThreadDispatcher.Instance.Enqueue(action, eventArgs);
            }
        }
    }
}