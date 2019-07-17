using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class LoadingIcon : UIComponent
    {
        public LoadingCube LoadingCube;
        public LoadingText LoadingText;

        public override IEnumerator Activate()
        {
            LoadingCube.Activate();
            LoadingText.Activate();
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            LoadingCube.Deactivate();
            LoadingText.Deactivate();
            yield break;
        }
    }
}
