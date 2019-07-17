using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;

namespace HoloIslandVis.Input.Gesture
{
    public class GestureSource
    {
        private enum SourceState
        {
            Pressed,
            Released
        }

        private Dictionary<SourceState, Func<GestureSource, byte>> _stateBytes;

        public IInputSource InputSource { get; private set; }
        public uint SourceId { get; private set; }
        public int InputBufferSize { get; private set; }
        public byte InputData { get; private set; }
        public bool IsManipulating { get; private set; }
        public bool IsEvaluating { get; set; }

        // How many InputDown events have been registered on this source
        // since the last reset?
        public int InputDown { get; set; }

        // How many InputUp  events have been registered on this source
        // since the last reset?
        public int InputUp { get; set; }

        public GestureSource(IInputSource inputSource, uint sourceId)
            : this (inputSource, sourceId, 2) { }

        public GestureSource(IInputSource inputSource, uint sourceId, int inputBufferSize)
        {
            var pressedFunc = new Func<GestureSource, byte>((source) => source.GetInputData(1, 0));
            var releaseFunc = new Func<GestureSource, byte>((source) => source.GetInputData(0, 1));

            _stateBytes = new Dictionary<SourceState, Func<GestureSource, byte>>
            {
                { SourceState.Pressed, pressedFunc },
                { SourceState.Released, releaseFunc }
            };

            InputSource = inputSource;
            SourceId = sourceId;
            InputBufferSize = inputBufferSize;
            IsManipulating = false;
            IsEvaluating = false;
            InputDown = 0;
            InputUp = 0;
        }

        public byte Evaluate()
        {
            InputData = GetInputData(InputDown, InputUp);

            // Checks whether source is in a pressed state (InputDown without InputUp).
            // If so, source is manipulating.
            if(InputData == _stateBytes[SourceState.Pressed].Invoke(this))
                IsManipulating = true;

            // Checks whether source has just been released (InputUp without InputDown).
            // If so, source is stopped manipulation.
            if (InputData == _stateBytes[SourceState.Released].Invoke(this))
                IsManipulating = false;

            // Evaluate input data again to account for any changes in manipulation state.
            InputData = GetInputData(InputDown, InputUp);
            return InputData;
        }

        // Reset source state for this gesture source.
        public void ResetInputData()
        {
            InputDown = 0;
            InputUp = 0;
        }

        // Encode state of this gesture input source into a byte.
        // LSB 0-2:     Contains number of times source was pressed (InputDown event received).
        // LSB 3-5:     Contains number of times source was released (InputUp event received).
        // LSB 6:       Flag indicating whether this source is currently manipulating.
        // LSB 7:       Flag indicating that source is currently present and active.
        private byte GetInputData(int inputDown, int inputUp)
        {
            IsEvaluating = true;

            byte inputData = 0;

            inputData += (byte) (Math.Min(InputBufferSize, inputDown));
            inputData += (byte) (Math.Min(InputBufferSize, inputUp) << 3);
            inputData += (byte) ((IsManipulating ? 1 : 0) << 6);
            inputData += (byte) (1 << 7);

            IsEvaluating = false;
            return inputData;
        }
    }
}
