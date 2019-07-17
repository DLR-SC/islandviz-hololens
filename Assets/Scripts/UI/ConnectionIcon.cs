using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Component
{
    public class ConnectionIcon : UIComponent
    {
        public ConnectionCube ConnectionCube;
        public ConnectionText ConnectionText;

        public override IEnumerator Activate()
        {
            ConnectionCube.Activate();
            ConnectionText.Activate();
            yield break;
        }

        public override IEnumerator Deactivate()
        {
            ConnectionCube.Deactivate();
            ConnectionText.Deactivate();
            yield break;
        }
    }
}
