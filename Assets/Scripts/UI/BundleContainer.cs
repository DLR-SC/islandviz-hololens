using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class BundleContainer : UIComponent
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