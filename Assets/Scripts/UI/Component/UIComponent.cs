using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public abstract class UIComponent : MonoBehaviour
    {
        public abstract IEnumerator Activate();
        public abstract IEnumerator Deactivate();
    }
}
