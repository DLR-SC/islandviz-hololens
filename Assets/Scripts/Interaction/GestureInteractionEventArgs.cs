using HoloIslandVis.Input.Gesture;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public class GestureInteractionEventArgs : InteractionEventArgs
    {
        public bool IsTwoHanded;
        public Vector3 HandOnePos;
        public Vector3 HandTwoPos;
        public GestureType Gesture;

        public Interactable Selected;
        public Interactable Focused;

        public GestureInteractionEventArgs()
        {

        }

        public GestureInteractionEventArgs(GestureInputEventArgs inputData)
        {
            Vector3 srcOnePos;
            Vector3 srcTwoPos;

            if (inputData.TryGetDoubleGripPosition(out srcOnePos, out srcTwoPos))
            {
                IsTwoHanded = true;
                HandOnePos = srcOnePos;
                HandTwoPos = srcTwoPos;
            }
            else if (inputData.TryGetSingleGripPosition(out srcOnePos))
            {
                IsTwoHanded = false;
                HandOnePos = srcOnePos;
            }
        }
    }
}
