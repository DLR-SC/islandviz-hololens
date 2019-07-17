using HoloIslandVis.Input.Gesture;
using HoloIslandVis.Input.Speech;

namespace HoloIslandVis.Input
{
    public interface IInputReceiver
    {
        void OnSpeechInputEvent(SpeechInputEventArgs eventArgs);
        void OnOneHandTap(GestureInputEventArgs eventArgs);
        void OnOneHandDoubleTap(GestureInputEventArgs eventArgs);
        void OnOneHandManipStart(GestureInputEventArgs eventArgs);
        void OnTwoHandManipStart(GestureInputEventArgs eventArgs);
        void OnManipulationUpdate(GestureInputEventArgs eventArgs);
        void OnManipulationEnd(GestureInputEventArgs eventArgs);
    }
}
