using HoloIslandVis.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class ScenarioPanel : UIComponent
    {
        public override IEnumerator Activate()
        {
            gameObject.SetActive(false);
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            gameObject.SetActive(false);
            yield break;
        }
    }
}