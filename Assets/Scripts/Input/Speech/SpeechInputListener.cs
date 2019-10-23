using HoloIslandVis.Controller.NLU;
using HoloIslandVis.Core;
using HoloIslandVis.Utilities;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace HoloIslandVis.Input.Speech
{
    public enum KeywordType
    {
        None,
        Show,
        Select,
        Deselect,
        ZoomIn,
        ZoomOut,
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        SelectBiggest,
        SelectSmallest,
        Invariant
    }

    // The SpeechInputListener captures speech input, checks whether it corresponds 
    // to a keyword and delegates the input events to the InputHandler. Optionally,
    // an external responder can be interposed to determine the keyword and provide
    // additional data as a string.
    public class SpeechInputListener : SingletonComponent<SpeechInputListener>
    {
        // Any time the activiation keyword is issued, a speech input event
        // is invoked, regardless whether the following token corresponds to 
        // a keyword. Input events that do not correspond to a keyword are 
        // forwarded as keyword type "None" and it is up to the receiver to
        // handle this case.
        public delegate void SpeechInputHandler(SpeechInputEventArgs eventArgs);
        public event SpeechInputHandler SpeechInputEvent  = delegate { };

        // Any external responder has to implement the IExternalResponder interface.
        public IExternalResponder ExternalResponder;
        // Keyword which will trigger a processing cycle, defaults to "Hello".
        public string ActivationKeyword;
        // Boolean indicating whether an external responder is interposed.
        public bool RespondExternally;

        private int _minFreq;
        private int _maxFreq;
        private Dictionary<string, KeywordType> _keywordTypeTable;
        private DictationRecognizer _dictationRecognizer;
        private bool _dictationStarted;
        private bool _isProcessing;

        void Start()
        {
            _dictationRecognizer = new DictationRecognizer();
            InputManager.Instance.AddGlobalListener(gameObject);
            Microphone.GetDeviceCaps("", out _minFreq, out _maxFreq);
            ExternalResponder = GameObject.Find("NLUService").GetComponent<NLUServiceClient>();

            _keywordTypeTable = new Dictionary<string, KeywordType>()
            {
                { "select", KeywordType.Select },
                { "deselect", KeywordType.Deselect },
            };

            _dictationRecognizer.DictationComplete += OnDictationComplete;
            _dictationRecognizer.DictationResult += OnDictationResult;
            _dictationRecognizer.DictationError += OnDictationError;

            _isProcessing = false;
            StartCoroutine(Status());
            StartCoroutine(RecognizerTimeout());
        }

        void Update()
        {
            //Debug.Log("Update");
            if (!(_dictationRecognizer.Status == SpeechSystemStatus.Running)
                && !_dictationStarted && !_isProcessing)
            {
                _dictationStarted = true;
                StartCoroutine(StartRecording());
            }
        }

        public IEnumerator Status()
        {
            while (true)
            {
                //Debug.Log("dictation started: " + _dictationStarted);
                //Debug.Log("is processing: " + _isProcessing);
                yield return new WaitForSeconds(2.0f);
            }
        }

        public IEnumerator StartRecording()
        {
            //Debug.Log("Dictation Start!");
            _dictationRecognizer.Start();
            yield return null;
            //Debug.Log("Dictation Stop!");
        }

        public IEnumerator RecognizerTimeout()
        {
            float currentTime = 0f;
            while (true)
            {
                if (_dictationStarted)
                {
                    if (currentTime == 0)
                    {
                        currentTime = Time.time;
                    }
                    //Debug.Log("startTime :" + currentTime);
                    //Debug.Log("currentTime :" + Time.time);
                    if (currentTime + 3 > Time.time)
                    {
                        if (!_isProcessing)
                        {
                            //Debug.Log("Reset");
                            _dictationRecognizer.Stop();
                            _dictationStarted = false;
                        }
                        currentTime = 0f;
                    }
                }
                yield return null;
            }
        }

        public void OnDictationResult(string text, ConfidenceLevel confidence)
        {
            //Debug.Log("OnDictationResultStart");
            //Debug.Log("Dictation result: " + text);
            // Return if SpeechInputListener is already processing speech
            // input (external responders may take more time).
            if (_isProcessing)
                return;

            // First token has to be equal to activation keyword, second token
            // is the actual input string for processing.
            string[] token = text.ToLower().Split(new char[] { ' ' }, 2);
            if (token[0] == ActivationKeyword.ToLower() && token.Length > 1)
            {
                var eventArgs = new SpeechInputEventArgs(token[1]);
                StartCoroutine(ProcessInput(eventArgs));
            }

            _dictationStarted = false;
            //Debug.Log("OnDictationResultStop");
        }

        public void OnDictationComplete(DictationCompletionCause cause)
        {
            //Debug.Log("OnDictationCompleteStart");
            //Debug.Log("Dictation Complete: " + cause.ToString());
            _dictationRecognizer.Stop();
            _dictationStarted = false;
            //Debug.Log("OnDictationCompleteStop");
        }

        public void OnDictationError(string error, int hresult)
        {
            //Debug.Log("OnDictationErrorStart");
            //Debug.Log("Dictation Error: " + error);
            _dictationRecognizer.Stop();
            _dictationStarted = false;
            //Debug.Log("OnDictationErrorStop");
        }

        private IEnumerator ProcessInput(SpeechInputEventArgs eventArgs)
        {
            //Debug.Log("ProcessInputStart");
            /*if (!GameObject.Find("AppConfig").GetComponent<AppConfig>().IsServerInstance)
                yield break;*/

            Debug.Log("Processing '" + eventArgs.Input + "'");
            _isProcessing = true;
            Action<SpeechInputEventArgs> action;

            if (RespondExternally)
            {
                // External responder takes care of evaluating and setting 
                // the keyword and data values for the input event args.
                yield return ExternalResponder.SendRequest(eventArgs);
            }
            // If no external responder is available, check if input token
            // is equal to a keyword.
            else if (_keywordTypeTable.ContainsKey(eventArgs.Input))
            {
                eventArgs.Keyword = (KeywordType)Enum.Parse(typeof(KeywordType), eventArgs.Input);
            }


            action = response => SpeechInputEvent(response);
            InputHandler.Instance.InvokeSpeechInputEvent(action, eventArgs);

            _dictationRecognizer.Stop();
            _isProcessing = false;
            //Debug.Log("ProcessInputStop");
        }
    }
}
