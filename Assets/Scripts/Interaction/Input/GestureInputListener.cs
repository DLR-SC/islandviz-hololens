using HoloIslandVis;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
    class SourceState
    {
        public IInputSource InputSource { get; private set; }
        public int InputDown { get; set; }
        public int InputUp { get; set; }

        public SourceState(IInputSource inputSource)
        {
            InputSource = inputSource;
            InputDown = 0;
            InputUp = 0;
        }
    }

    public class GestureInputListener : SingletonComponent<GestureInputListener>, 
        IInputHandler, ISourceStateHandler
    {
        private const float INPUT_WAIT = 0.25f;

        public delegate void GestureInputHandler(BaseInputEventData eventData);

        public event GestureInputHandler OneHandTap = delegate { };
        public event GestureInputHandler TwoHandTap = delegate { };
        public event GestureInputHandler OneHandDoubleTap = delegate { };
        public event GestureInputHandler TwoHandDoubleTap = delegate { };

        //public event GestureInputHandler OneHandManipulationStart;
        //public event GestureInputHandler TwoHandManipulationStart;
        //public event GestureInputHandler OneHandManipulationEnd;
        //public event GestureInputHandler TwoHandManipulationEnd;

        private bool _isEvaluating;
        private Dictionary<IInputSource, SourceState> _sourceStates;
        private Dictionary<int, Action<BaseInputEventData>> _gestureEventTable;

        protected override void Awake()
        {
            base.Awake();
            _isEvaluating = false;

            _sourceStates = new Dictionary<IInputSource, SourceState>(2);
            _gestureEventTable = new Dictionary<int, Action<BaseInputEventData>>()
            {
                { Convert.ToInt32("00000101", 2), eventData => OneHandTap(eventData) },
                { Convert.ToInt32("01010000", 2), eventData => OneHandTap(eventData) },
                { Convert.ToInt32("01010101", 2), eventData => TwoHandTap(eventData) },
                { Convert.ToInt32("00001010", 2), eventData => OneHandDoubleTap(eventData) },
                { Convert.ToInt32("10100000", 2), eventData => OneHandDoubleTap(eventData) },
                { Convert.ToInt32("10101010", 2), eventData => TwoHandDoubleTap(eventData) }
            };

            InputManager.Instance.AddGlobalListener(gameObject);
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if(_sourceStates.ContainsKey(inputSource))
                return;

            _sourceStates.Add(inputSource, new SourceState(inputSource));
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            IInputSource inputSource = eventData.InputSource;
            if(!_sourceStates.ContainsKey(inputSource))
                return;

            _sourceStates.Remove(inputSource);
        }

        public void OnInputDown(InputEventData eventData)
        {
            _sourceStates[eventData.InputSource].InputDown++;

            if(!_isEvaluating)
                StartCoroutine(handleInputDown(eventData));
        }

        public void OnInputUp(InputEventData eventData)
        {
            SourceState sourceState = _sourceStates[eventData.InputSource];

            if(sourceState.InputDown > sourceState.InputUp)
                sourceState.InputUp++;
        }

        private IEnumerator handleInputDown(BaseInputEventData eventData)
        {
            _isEvaluating = true;
            yield return new WaitForSeconds(INPUT_WAIT);
            byte inputData = getInputData(_sourceStates);

            Action<BaseInputEventData> action;
            if (_gestureEventTable.TryGetValue(inputData, out action))
                action.Invoke(eventData);

            resetSourceStates();
            _isEvaluating = false;
        }

        private byte getInputData(Dictionary<IInputSource, SourceState> sourceStates)
        {
            byte inputData = 0;
            SourceState[] statesArr = new SourceState[sourceStates.Values.Count];
            sourceStates.Values.CopyTo(statesArr, 0);

            for (int i = 0; i < statesArr.Length; i++)
            {
                inputData += (byte) (Mathf.Min(2, statesArr[i].InputDown) << i*4);
                inputData += (byte) (Mathf.Min(2, statesArr[i].InputUp) << i*4+2);
            }

            return inputData;
        }

        private void resetSourceStates()
        {
            foreach(var sourceState in _sourceStates.Values)
            {
                sourceState.InputDown = 0;
                sourceState.InputUp = 0;
            }
        }
    }
}