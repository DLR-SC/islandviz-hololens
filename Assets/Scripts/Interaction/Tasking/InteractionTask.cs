using HoloIslandVis.Controller;
using System.Collections;

namespace HoloIslandVis.Interaction
{
    public abstract class InteractionTask
    {
        public abstract IEnumerator Perform(InteractionEventArgs eventArgs);
    }
}
