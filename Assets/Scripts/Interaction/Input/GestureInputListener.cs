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

        public event GestureInputHandler Tap                    = delegate { };
        public event GestureInputHandler DoubleTap              = delegate { };

        public event GestureInputHandler ManipulationStart      = delegate { };
        public event GestureInputHandler ManipulationEnd        = delegate { };
        public event GestureInputHandler ManipulationUpdate     = delegate { };

        private Dictionary<IInputSource, GestureSource> _gestureSources;
        private Dictionary<byte, Action<GestureInputEventArgs>> _gestureEventTable;

        protected override void Awake()
        {
            base.Awake();
            _gestureEventTable = new Dictionary<byte, Action<GestureInputEventArgs>>()
            {
                { Convert.ToByte("10001001", 2), eventArgs => Tap(eventArgs) },
                { Convert.ToByte("10010010", 2), eventArgs => DoubleTap(eventArgs) },
                { Convert.ToByte("11000001", 2), eventArgs => ManipulationStart(eventArgs) },
                { Convert.ToByte("10001000", 2), eventArgs => ManipulationEnd(eventArgs) }
            };

            _gestureSources = new Dictionary<IInputSource, GestureSource>(2);
            InputManager.Instance.AddGlobalListener(gameObject);
        }

        private void Update()
        {
            foreach(GestureSource source in _gestureSources.Values)
            {
                if(source.IsManipulating && !source.IsEvaluating)
                    ManipulationUpdate(new GestureInputEventArgs());
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
            StartCoroutine(processInput(eventData));
        }

        public void OnInputUp(InputEventData eventData)
        {
            _gestureSources[eventData.InputSource].InputUp++;
            StartCoroutine(processInput(eventData));
        }

        private IEnumerator processInput(BaseInputEventData eventData)
        {
            if(!_gestureSources.ContainsKey(eventData.InputSource))
                yield break;

            GestureSource gestureSource = _gestureSources[eventData.InputSource];
            if(gestureSource.IsEvaluating)
                yield break;

            yield return StartCoroutine(gestureSource.Evaluate());

            Action<GestureInputEventArgs> action;
            if(_gestureEventTable.TryGetValue(gestureSource.InputData, out action))
                action.Invoke(new GestureInputEventArgs());
        }
    }
}