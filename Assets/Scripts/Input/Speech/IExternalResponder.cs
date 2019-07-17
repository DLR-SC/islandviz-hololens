using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Input.Speech
{
    public interface IExternalResponder
    {
        IEnumerator SendRequest(SpeechInputEventArgs eventArgs);
    }
}
