using HoloIslandVis.Core;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolkitHelper : Singleton<ToolkitHelper>
{
    public Vector3 GazePosition {
        get { return GazeManager.Instance.GazeTransform.position; }
    }


    private ToolkitHelper()
    {

    }
}
