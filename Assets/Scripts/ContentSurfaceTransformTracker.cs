using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentSurfaceTransformTracker : MonoBehaviour
{	
	void Update ()
    {
        if(transform.hasChanged)
        {
            RuntimeCache.Instance.CombinedHoloMaterial.SetVector("_ReferencePosition", transform.position);
            RuntimeCache.Instance.CombinedHoloMaterial.SetVector("_ReferenceNormal", transform.up);
        }
	}
}
