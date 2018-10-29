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

            RuntimeCache.Instance.ArrowHeadMaterial.SetVector("_ReferencePosition", transform.position);
            RuntimeCache.Instance.ArrowHeadMaterial.SetVector("_ReferenceNormal", transform.up);

            RuntimeCache.Instance.ImportArrowMaterial.SetVector("_ReferencePosition", transform.position);
            RuntimeCache.Instance.ImportArrowMaterial.SetVector("_ReferenceNormal", transform.up);

            RuntimeCache.Instance.ExportArrowMaterial.SetVector("_ReferencePosition", transform.position);
            RuntimeCache.Instance.ExportArrowMaterial.SetVector("_ReferenceNormal", transform.up);

            RuntimeCache.Instance.WireFrame.SetVector("_ReferencePosition", transform.position);
            RuntimeCache.Instance.WireFrame.SetVector("_ReferenceNormal", transform.up);

            RuntimeCache.Instance.DependencyContainer.transform.position =
                RuntimeCache.Instance.VisualizationContainer.transform.position;
        }
	}
}
