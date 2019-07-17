using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis.UI.Info
{
    public class PanelHeader : MonoBehaviour
    {
        public static Text HeaderTextTop;
        public static Text HeaderTextBottom;

        private void Start()
        {
            HeaderTextTop = GameObject.Find("HeaderTextTop").GetComponent<Text>();
            HeaderTextBottom = GameObject.Find("HeaderTextBottom").GetComponent<Text>();
        }
    }
}
