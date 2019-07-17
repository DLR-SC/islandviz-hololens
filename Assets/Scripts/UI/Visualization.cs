using HoloIslandVis.Core;
using HoloIslandVis.Utilities;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class Visualization : UIComponent
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
