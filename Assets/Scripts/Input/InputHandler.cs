using HoloIslandVis.Core;
using HoloIslandVis.Input.Gesture;
using HoloIslandVis.Input.Speech;
using System;

namespace HoloIslandVis.Input
{
    // Input events are validated and invoked.
    public class InputHandler : SingletonComponent<InputHandler>
    {
        public AppConfig AppConfig;
        public SyncManager SyncManager;

        public void InvokeSpeechInputEvent(Action<SpeechInputEventArgs> action, SpeechInputEventArgs eventArgs)
        {
            // If input event is valid, invoke.
            if (ValidateInputEvent(eventArgs))
                action.Invoke(eventArgs);
        }

        public void InvokeGestureInputEvent(Action<GestureInputEventArgs> action, GestureInputEventArgs eventArgs)
        {
            // If input event is valid, invoke.
            if (ValidateInputEvent(eventArgs))
                action.Invoke(eventArgs);
        }

        private bool ValidateInputEvent(GestureInputEventArgs eventArgs)
        {
            return AppConfig.IsServerInstance || !SyncManager.SharingStarted;
        }

        private bool ValidateInputEvent(SpeechInputEventArgs eventArgs)
        {
            return AppConfig.IsServerInstance || !SyncManager.SharingStarted;
        }
    }
}
