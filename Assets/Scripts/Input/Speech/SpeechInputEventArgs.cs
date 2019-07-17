using UnityEngine.Windows.Speech;
using UnityEngine;

namespace HoloIslandVis.Input.Speech
{
    public class SpeechInputEventArgs : InputEventArgs
    {
        public string Data;
        public string Input;
        public ConfidenceLevel Confidence;
        public KeywordType Keyword;

        public SpeechInputEventArgs(string input, ConfidenceLevel confidence)
        {
            Input = input;
            Confidence = confidence;
            Keyword = KeywordType.None;
        }
    }
}