using HoloIslandVis.Input.Speech;
using System;
using UnityEngine.Windows.Speech;

namespace HoloIslandVis.Interaction
{
    public class SpeechInteractionEventArgs : InteractionEventArgs
    {
        public string Data;
        public string Input;
        public KeywordType Keyword;

        public Interactable Focused;
        public Interactable Selected;

        public SpeechInteractionEventArgs()
        {

        }

        public SpeechInteractionEventArgs(SpeechInputEventArgs eventArgs)
        {
            Data = eventArgs.Data;
            Input = eventArgs.Input;

            string keywordString = eventArgs.Keyword.ToString();
            Keyword = (KeywordType)Enum.Parse(typeof(KeywordType), keywordString, true);
        }
    }
}
