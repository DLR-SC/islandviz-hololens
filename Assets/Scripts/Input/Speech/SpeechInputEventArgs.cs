using UnityEngine.Windows.Speech;
using UnityEngine;

namespace HoloIslandVis.Input.Speech
{
    public class SpeechInputEventArgs : InputEventArgs
    {
        public string Data;
        public string Input;
        public KeywordType Keyword;

        public SpeechInputEventArgs(string input)
        {
            Input = input;
            Keyword = KeywordType.None;
        }
    }
}