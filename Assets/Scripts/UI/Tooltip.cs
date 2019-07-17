using HoloToolkit.UX.ToolTips;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI
{
    public class Tooltip : MonoBehaviour
    {
        public ToolTip HoloTooltip;
        public ToolTipConnector HoloTooltipConnector;

        void Start()
        {
            HoloTooltip.gameObject.SetActive(false);
        }

        public void Show(GameObject target)
        {
            HoloTooltip.ToolTipText = target.name;
            HoloTooltip.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 0.12f, target.transform.position.z);
            HoloTooltipConnector.Target = target;
            HoloTooltip.gameObject.SetActive(true);
        }

        public void Hide()
        {
            HoloTooltip.gameObject.SetActive(false);
        }

    }
}
