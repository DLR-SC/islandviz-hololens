using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Task
{
    public abstract class InteractionTask
    {
        public abstract void Pass(BaseInputEventData eventData);
    }
}
