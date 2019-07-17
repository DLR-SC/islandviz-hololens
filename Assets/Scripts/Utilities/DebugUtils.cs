using HoloIslandVis.Controller;
using HoloIslandVis.Core;
using HoloIslandVis.Input.Gesture;
using HoloIslandVis.Interaction;
using HoloIslandVis.Interaction.Tasking.Task;
using UnityEngine;

namespace HoloIslandVis.Utilities
{
    public class DebugUtils : SingletonComponent<DebugUtils>
    {
        public void GestureInput()
        {
            // Two-handed input working only on device.
            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventData) => Debug.Log("OneHandTap");
            GestureInputListener.Instance.TwoHandTap += (GestureInputEventArgs eventData) => Debug.Log("TwoHandTap");
            GestureInputListener.Instance.OneHandDoubleTap += (GestureInputEventArgs eventData) => Debug.Log("OneHandDoubleTap");
            GestureInputListener.Instance.TwoHandDoubleTap += (GestureInputEventArgs eventData) => Debug.Log("TwoHandDoubleTap");
            GestureInputListener.Instance.OneHandManipStart += (GestureInputEventArgs eventData) => Debug.Log("OneHandManipulationStart");
            GestureInputListener.Instance.TwoHandManipStart += (GestureInputEventArgs eventData) => Debug.Log("TwoHandManipulationStart");
            GestureInputListener.Instance.ManipulationUpdate += (GestureInputEventArgs eventData) => Debug.Log("ManipulationUpdate");
            GestureInputListener.Instance.ManipulationEnd += (GestureInputEventArgs eventData) => Debug.Log("ManipulationEnd");
        }

        public void Interaction()
        {
            State state_test = new State("test");

            var task_testCont = new TaskContinuousTest();
            var task_testDisc = new TaskGestureDiscreteTest();

            Command command_testCont = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.ContentPane);
            Command command_testDisc = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane);

            state_test.AddInteractionTask(command_testCont, task_testCont);
            state_test.AddInteractionTask(command_testDisc, task_testDisc);

            StateManager.Instance.Init(state_test);
        }
    }
}
