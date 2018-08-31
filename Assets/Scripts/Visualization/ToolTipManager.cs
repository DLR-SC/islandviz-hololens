using HoloIslandVis;
using HoloIslandVis.Utility;
using HoloToolkit.UX.ToolTips;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipManager : SingletonComponent<ToolTipManager> {

    private GameObject toolTip;

    ToolTipConnector toolTipConnector;

    // Use this for initialization
    void Start()
    {
        toolTip = GameObject.Find("ToolTip");
        toolTip.SetActive(false);
        toolTip.GetComponent<ToolTip>().ShowConnector = true;
        toolTip.GetComponent<ToolTip>().ShowBackground = false;
        toolTipConnector = toolTip.GetComponent<ToolTipConnector>();
        
    }
        // Update is called once per frame
    void Update () {
	}

    internal void showToolTip(GameObject metaphor)
    {
        toolTip.GetComponent<ToolTip>().ToolTipText = metaphor.name;
        toolTip.transform.position = new Vector3(metaphor.transform.position.x, metaphor.transform.position.y + 0.1f, metaphor.transform.position.z);
        toolTipConnector.Target = metaphor;
        toolTip.SetActive(true);
    }

    internal void hideToolTip()
    {
        toolTip.SetActive(false);
    }
}
