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
            _dictationRecognizer.Start();

            _isProcessing = false;
        }

        void Update()
        {
            if (!(_dictationRecognizer.Status == SpeechSystemStatus.Running)
                && !_dictationStarted && !_isProcessing)
            {
                _dictationStarted = true;
                StartCoroutine(StartRecording());
            }
        }

        public IEnumerator StartRecording()
        {
            _dictationRecognizer.Start();
            yield return null;
        }

        public void OnDictationResult(string text, ConfidenceLevel confidence)
        {
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

            if (_dictationRecognizer.Status == SpeechSystemStatus.Running)
                _dictationRecognizer.Stop();

            _dictationStarted = false;
        }

        public void OnDictationComplete(DictationCompletionCause cause)
        {
            if(_dictationRecognizer.Status == SpeechSystemStatus.Running)
                _dictationRecognizer.Stop();

            _dictationStarted = false;
        }

        public void OnDictationError(string error, int hresult)
        {
            if (_dictationRecognizer.Status == SpeechSystemStatus.Running)
                _dictationRecognizer.Stop();

            _dictationStarted = false;
        }

        private IEnumerator ProcessInput(SpeechInputEventArgs eventArgs)
        {
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

            _isProcessing = false;
        }
    }
}
