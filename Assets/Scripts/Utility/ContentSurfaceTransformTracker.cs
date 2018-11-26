using HoloIslandVis.Sharing;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentSurfaceTransformTracker : MonoBehaviour
{	

    void Start()
    {
        ObjectStateSynchronizer stateSynchronizer 
            = gameObject.GetComponent<ObjectStateSynchronizer>();

        if (stateSynchronizer != null)
            stateSynchronizer.TransformChange += OnTransformChange;
    }

	void Update ()
    {
        if (transform.hasChanged && !SharingClient.Instance.IsConnected)
        {
            transform.hasChanged = false;
            updateShaderParams();
        }
    }

    public void OnTransformChange()
    {
        if (SharingClient.Instance.IsConnected)
            updateShaderParams();
    }

    private void updateShaderParams()
    {
        RuntimeCache.Instance.CombinedHoloMaterial.SetVector("_ReferencePosition", transform.localPosition);
        RuntimeCache.Instance.CombinedHoloMaterial.SetVector("_ReferenceNormal", transform.up);

        RuntimeCache.Instance.ArrowHeadMaterial.SetVector("_ReferencePosition", transform.localPosition);
        RuntimeCache.Instance.ArrowHeadMaterial.SetVector("_ReferenceNormal", transform.up);

        RuntimeCache.Instance.ImportArrowMaterial.SetVector("_ReferencePosition", transform.localPosition);
        RuntimeCache.Instance.ImportArrowMaterial.SetVector("_ReferenceNormal", transform.up);

        RuntimeCache.Instance.ExportArrowMaterial.SetVector("_ReferencePosition", transform.localPosition);
        RuntimeCache.Instance.ExportArrowMaterial.SetVector("_ReferenceNormal", transform.up);

        RuntimeCache.Instance.WireFrame.SetVector("_ReferencePosition", transform.localPosition);
        RuntimeCache.Instance.WireFrame.SetVector("_ReferenceNormal", transform.up);
    }
}
