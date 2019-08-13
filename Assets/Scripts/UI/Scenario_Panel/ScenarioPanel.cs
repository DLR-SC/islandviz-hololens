using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis.UI.Component
{
    public class ScenarioPanel : UIComponent
    {

        public override IEnumerator Activate()
        {
            gameObject.SetActive(true);
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            gameObject.SetActive(false);
            yield break;
        }
    }
}