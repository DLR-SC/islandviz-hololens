using System.Collections;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public abstract class DiscreteGestureInteractionTask : DiscreteInteractionTask
    {
        public override IEnumerator Perform(InteractionEventArgs eventArgs)
        {
            if(eventArgs is GestureInteractionEventArgs)
            {
                var casted = (GestureInteractionEventArgs)eventArgs;
                yield return Perform(casted);
            }
            else if(eventArgs is SpeechInteractionEventArgs)
            {
                var casted = (SpeechInteractionEventArgs)eventArgs;
                yield return Perform(casted);
            }
        }

        public virtual IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            yield break;
        }

        public virtual IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            yield break;
        }
    }
}