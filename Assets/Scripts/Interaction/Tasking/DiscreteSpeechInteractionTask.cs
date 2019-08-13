
using System.Collections;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public abstract class DiscreteSpeechInteractionTask : DiscreteInteractionTask
    {
        public override IEnumerator Perform(InteractionEventArgs eventArgs)
        {
            var casted = (SpeechInteractionEventArgs) eventArgs;
            ScenarioHandler.IncrementCounterVoiceControl();
            yield return Perform(casted);
        }

        public abstract IEnumerator Perform(SpeechInteractionEventArgs eventArgs);
    }
}