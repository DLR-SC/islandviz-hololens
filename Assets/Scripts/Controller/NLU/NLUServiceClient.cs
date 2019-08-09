using HoloIslandVis.Core;
using HoloIslandVis.Input.Speech;
using HoloIslandVis.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloIslandVis.Controller.NLU
{
    public class NLUServiceClient : SingletonComponent<NLUServiceClient>, IExternalResponder
    {
        private Dictionary<string, KeywordType> _intentToKeyword;

        public AppConfig AppConfig;
        public string ServiceAddress;
        public int ServicePort;

        private string _serverEndpoint;
        private string _latestResponse;
        private bool _errorState;

        void Start()
        {
            _serverEndpoint = "http://" + ServiceAddress + ":" + ServicePort + "/";

            _intentToKeyword = new Dictionary<string, KeywordType>()
            {
                { "zoom_in", KeywordType.ZoomIn},
                { "zoom_out", KeywordType.ZoomOut},
                { "move_up", KeywordType.MoveUp},
                { "move_down", KeywordType.MoveDown},
                { "move_left", KeywordType.MoveLeft},
                { "move_right", KeywordType.MoveRight},
                { "select_component", KeywordType.Select}
            };
        }

        public IEnumerator SendRequest(SpeechInputEventArgs eventArgs)
        {
            if (AppConfig.SharingEnabled && !AppConfig.IsServerInstance)
                yield break;

            Context context = ContextManager.Instance.SafeContext;

            string focused = context.Focused.name;
            string selected = context.Selected.name;
            string focusedType = context.SelectedType.ToString();
            string selectedType = context.SelectedType.ToString();
            string gesture = "";

            NLUServiceContext nluContext = new NLUServiceContext(focused,
                focusedType, selected, selectedType, gesture);

            yield return FetchData(eventArgs.Input, nluContext);

            if (!_errorState)
            {
                string response = _latestResponse;

                JSONObject jsonObject = new JSONObject(response);
                var intent = jsonObject.GetField("intent_name").GetField("name").ToString();
                intent = char.ToLower(intent[1]) + intent.Substring(2, intent.Length - 3);

                if (_intentToKeyword.ContainsKey(intent))
                    eventArgs.Keyword = _intentToKeyword[intent];
                else eventArgs.Keyword = KeywordType.None;

                eventArgs.Data = jsonObject.GetField("data").ToString();
            }

            yield return null;
        }

        public IEnumerator FetchData(string input, NLUServiceContext context)
        {
            string putData = buildPutData(input, context);
            byte[] bytes = Encoding.UTF8.GetBytes(putData);

            var webRequest = UnityWebRequest.Put(_serverEndpoint+"api", bytes);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error While Sending: " + webRequest.error);
                _errorState = true;
            }
            else Debug.Log("Received: " + webRequest.downloadHandler.text);
            _latestResponse = webRequest.downloadHandler.text;
        }

        private string buildPutData(string input, NLUServiceContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"recipient_id\": \"default\",");
            builder.Append("\"application_state\": {");
            builder.Append($"\"focused_object\":\"{context.Focused}\",");
            builder.Append($"\"focused_object_type\":\"{context.FocusedType}\",");
            builder.Append($"\"selected_object\":\"{context.Selected}\",");
            builder.Append($"\"selected_object_type\":\"{context.SelectedType}\"}},");
            builder.Append($"\"user_utterance\":\"{input}\",");
            builder.Append($"\"gesture_type\":\"{context.GestureType}\"}}");
            return builder.ToString();
        }
    }
}
