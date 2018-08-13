using HoloIslandVis;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using System;
using HoloToolkit.Unity;
using System.Net;

namespace HoloIslandVis.Interaction.Input
{
    public class SpeechInputListener : SingletonComponent<SpeechInputListener>
    {
        public delegate void SpeechInputHandler(EventArgs eventData);
        public event SpeechInputHandler speechResponse = delegate { };

        private DictationRecognizer m_DictationRecognizer;
        private TextToSpeech tts;
        private bool listening = false;

        protected override void Awake()
        {
            base.Awake();
            tts = gameObject.AddComponent<TextToSpeech>();
            initDictationRecognizer();
        }

        private void initDictationRecognizer()
        {
            m_DictationRecognizer = new DictationRecognizer();

            m_DictationRecognizer.DictationResult += (text, confidence) =>
            {
                if (!listening)
                {
                    //words to activate the listening
                    if (text.Equals("hello") || text.Equals("test") || text.Equals("morty"))
                    {
                        listening = true;
                    }
                }
                else
                {
                    //this is just returning what the user just said
                    tts.StartSpeaking("I think you said:" + text);
                    getAPIResponse();
                    listening = false;
                }
            };
            m_DictationRecognizer.Start();
        }

        //TODO
        private void getAPIResponse()
        {
            throw new NotImplementedException();
        }
    }
}
