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
using UnityEngine.Networking;

namespace HoloIslandVis.Interaction.Input
{
    public class SpeechInputListener : SingletonComponent<SpeechInputListener>
    {
        public delegate void SpeechInputHandler(EventArgs eventData);
        public event SpeechInputHandler speechResponse = delegate { };
        private string baseURL = "http://localhost:5005/";

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
                    if (text.Equals("hello") || text.Equals("test") || text.Equals("wilson"))
                    {
                        listening = true;
                        tts.StartSpeaking("I am listening");
                    }
                    else
                    if (text.StartsWith("hello") || text.StartsWith("test") || text.StartsWith("wilson"))
                    {
                        int i = text.IndexOf(" ") + 1;
                        string str = text.Substring(i);
                        StartCoroutine(getAPIResponse(str));
                    }
                }
                else
                {
                    StartCoroutine(getAPIResponse(text));
                    listening = false;
                }
            };
            m_DictationRecognizer.Start();
        }

        IEnumerator getAPIResponse(string voiceCommand)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");

            using (WWW www = new WWW(
                baseURL + "conversations/default/parse",
                System.Text.Encoding.UTF8.GetBytes("{\"query\" : \"" + voiceCommand + "\"}"),
                headers))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    tts.StartSpeaking("somethings wrong with the API.");
                    Debug.Log(www.error);
                }
                else
                {
                    RasaResponse response = JsonUtility.FromJson<RasaResponse>(www.text);
                    Debug.Log("next command would be:" + response.next_action);
                }
            }
        }
    }
}
