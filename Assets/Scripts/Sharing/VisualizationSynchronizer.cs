using HoloIslandVis;
using HoloIslandVis.Sharing;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizationSynchronizer : Singleton<VisualizationSynchronizer>
{
    public void Sync()
    {
        List<Island> islands = RuntimeCache.Instance.Islands;
        foreach(Island island in islands)
        {
            CustomMessages.Instance.SendIslandTransform(island.name, island.transform.position, island.transform.rotation);
        }
    }
	
}
