using HoloIslandVis.Core;
using HoloIslandVis.Input.Speech;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloIslandVis.Controller.NLU
{
    public class NLUServiceClient : SingletonComponent<NLUServiceClient>, IExternalResponder
    {
        public string ServerAddress;
        public int ServerPort;

        private string _serverEndpoint;
        private string _latestResponse;

        void Start()
        {
            _serverEndpoint = "http://" + ServerAddress + ":" + ServerPort + "/";
        }

        public IEnumerator SendRequest(SpeechInputEventArgs eventArgs)
        {
            Context context = ContextManager.Instance.SafeContext;

            string focused = context.Focused.name;
            string selected = context.Selected.name;
            string focusedType = context.SelectedType.ToString();
            string selectedType = context.SelectedType.ToString();
            string gesture = "";

            NLUServiceContext nluContext = new NLUServiceContext(focused,
                focusedType, selected, selectedType, gesture);

            yield return FetchData(eventArgs.Input, nluContext);
            string response = _latestResponse;

            JSONObject jsonObject = new JSONObject(response);
            string intent = jsonObject.GetField("intent_name").ToString();
            intent = char.ToUpper(intent[1]) + intent.Substring(2, intent.Length - 3);

            eventArgs.Keyword = (KeywordType)Enum.Parse(typeof(KeywordType), intent);
            eventArgs.Data = jsonObject.GetField("data").ToString();

            yield return null;
        }

        public IEnumerator FetchData(string input, NLUServiceContext context)
        {
            string putData = buildPutData(input, context);
            byte[] bytes = Encoding.UTF8.GetBytes(putData);
            yield return null;
            var webRequest = UnityWebRequest.Put(_serverEndpoint + "api", bytes);
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError) Debug.Log("Error While Sending: " + webRequest.error);
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
