using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskContinuousTest : ContinuousInteractionTask
    {
        public override IEnumerator StartInteraction(GestureInteractionEventArgs eventArgs)
        {
            Debug.Log("START INTERACTION: TaskContinuousTest");
            yield break;
        }

        public override IEnumerator UpdateInteraction(GestureInteractionEventArgs eventArgs)
        {
            Debug.Log("UPDATE INTERACTION: TaskContinuousTest");
            yield break;
        }

        public override IEnumerator EndInteraction(GestureInteractionEventArgs eventArgs)
        {
            Debug.Log("END INTERACTION: TaskContinuousTest");
            yield break;
        }
    }
}
