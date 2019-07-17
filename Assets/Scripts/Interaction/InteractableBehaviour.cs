using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Behaviour
{
    public abstract class InteractableBehaviour : MonoBehaviour
    {
        public abstract IEnumerator Perform();
    }
}
