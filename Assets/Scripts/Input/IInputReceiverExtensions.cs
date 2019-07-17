using HoloIslandVis.Input.Gesture;
using HoloIslandVis.Input.Speech;

namespace HoloIslandVis.Input
{
    public static class IInputReceiverExtensions
    {
        // Hook this receiver to all input events.
        public static void SubscribeToInputEvents(this IInputReceiver receiver)
        {
            SpeechInputListener.Instance.SpeechInputEvent += receiver.OnSpeechInputEvent;

            GestureInputListener.Instance.OneHandTap += receiver.OnOneHandTap;
            GestureInputListener.Instance.OneHandDoubleTap += receiver.OnOneHandDoubleTap;
            GestureInputListener.Instance.OneHandManipStart += receiver.OnOneHandManipStart;
            GestureInputListener.Instance.TwoHandManipStart += receiver.OnTwoHandManipStart;
            GestureInputListener.Instance.ManipulationUpdate += receiver.OnManipulationUpdate;
            GestureInputListener.Instance.ManipulationEnd += receiver.OnManipulationEnd;
        }

        // Unsubscribe this receiver from all input events.
        public static void UnsubscribeToInputEvents(this IInputReceiver receiver)
        {
            SpeechInputListener.Instance.SpeechInputEvent -= receiver.OnSpeechInputEvent;

            GestureInputListener.Instance.OneHandTap -= receiver.OnOneHandTap;
            GestureInputListener.Instance.OneHandDoubleTap -= receiver.OnOneHandDoubleTap;
            GestureInputListener.Instance.OneHandManipStart -= receiver.OnOneHandManipStart;
            GestureInputListener.Instance.TwoHandManipStart -= receiver.OnTwoHandManipStart;
            GestureInputListener.Instance.ManipulationUpdate -= receiver.OnManipulationUpdate;
            GestureInputListener.Instance.ManipulationEnd -= receiver.OnManipulationEnd;
        }
    }
}
