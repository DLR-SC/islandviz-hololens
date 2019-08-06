using HoloIslandVis.Core;
using HoloIslandVis.Sharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clippable : MonoBehaviour
{
    public ObjectStateSynchronizer ObjectStateSynchronizer;

    public Material DefaultMaterial;
    public Material ArrowHeadMaterial;
    public Material ImportArrowMaterial;
    public Material ExportArrowMaterial;
    public Material WireframeMaterial;

    public GameObject ClippingPlaneBottom;
    public GameObject ClippingPlaneFront;
    public GameObject ClippingPlaneBack;
    public GameObject ClippingPlaneRight;
    public GameObject ClippingPlaneLeft;

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (transform.hasChanged)
        {
            DefaultMaterial.SetVector("_PositionClipBottom", ClippingPlaneBottom.transform.position);
            DefaultMaterial.SetVector("_PositionClipFront", ClippingPlaneFront.transform.position);
            DefaultMaterial.SetVector("_PositionClipBack", ClippingPlaneBack.transform.position);
            DefaultMaterial.SetVector("_PositionClipRight", ClippingPlaneRight.transform.position);
            DefaultMaterial.SetVector("_PositionClipLeft", ClippingPlaneLeft.transform.position);
            DefaultMaterial.SetVector("_NormalClipBottom", ClippingPlaneBottom.transform.up);
            DefaultMaterial.SetVector("_NormalClipFront", ClippingPlaneFront.transform.up);
            DefaultMaterial.SetVector("_NormalClipBack", ClippingPlaneBack.transform.up);
            DefaultMaterial.SetVector("_NormalClipRight", ClippingPlaneRight.transform.up);
            DefaultMaterial.SetVector("_NormalClipLeft", ClippingPlaneLeft.transform.up);

            ArrowHeadMaterial.SetVector("_PositionClipBottom", ClippingPlaneBottom.transform.position);
            ArrowHeadMaterial.SetVector("_PositionClipFront", ClippingPlaneFront.transform.position);
            ArrowHeadMaterial.SetVector("_PositionClipBack", ClippingPlaneBack.transform.position);
            ArrowHeadMaterial.SetVector("_PositionClipRight", ClippingPlaneRight.transform.position);
            ArrowHeadMaterial.SetVector("_PositionClipLeft", ClippingPlaneLeft.transform.position);
            ArrowHeadMaterial.SetVector("_NormalClipBottom", ClippingPlaneBottom.transform.up);
            ArrowHeadMaterial.SetVector("_NormalClipFront", ClippingPlaneFront.transform.up);
            ArrowHeadMaterial.SetVector("_NormalClipBack", ClippingPlaneBack.transform.up);
            ArrowHeadMaterial.SetVector("_NormalClipRight", ClippingPlaneRight.transform.up);
            ArrowHeadMaterial.SetVector("_NormalClipLeft", ClippingPlaneLeft.transform.up);

            ImportArrowMaterial.SetVector("_PositionClipBottom", ClippingPlaneBottom.transform.position);
            ImportArrowMaterial.SetVector("_PositionClipFront", ClippingPlaneFront.transform.position);
            ImportArrowMaterial.SetVector("_PositionClipBack", ClippingPlaneBack.transform.position);
            ImportArrowMaterial.SetVector("_PositionClipRight", ClippingPlaneRight.transform.position);
            ImportArrowMaterial.SetVector("_PositionClipLeft", ClippingPlaneLeft.transform.position);
            ImportArrowMaterial.SetVector("_NormalClipBottom", ClippingPlaneBottom.transform.up);
            ImportArrowMaterial.SetVector("_NormalClipFront", ClippingPlaneFront.transform.up);
            ImportArrowMaterial.SetVector("_NormalClipBack", ClippingPlaneBack.transform.up);
            ImportArrowMaterial.SetVector("_NormalClipRight", ClippingPlaneRight.transform.up);
            ImportArrowMaterial.SetVector("_NormalClipLeft", ClippingPlaneLeft.transform.up);

            ExportArrowMaterial.SetVector("_PositionClipBottom", ClippingPlaneBottom.transform.position);
            ExportArrowMaterial.SetVector("_PositionClipFront", ClippingPlaneFront.transform.position);
            ExportArrowMaterial.SetVector("_PositionClipBack", ClippingPlaneBack.transform.position);
            ExportArrowMaterial.SetVector("_PositionClipRight", ClippingPlaneRight.transform.position);
            ExportArrowMaterial.SetVector("_PositionClipLeft", ClippingPlaneLeft.transform.position);
            ExportArrowMaterial.SetVector("_NormalClipBottom", ClippingPlaneBottom.transform.up);
            ExportArrowMaterial.SetVector("_NormalClipFront", ClippingPlaneFront.transform.up);
            ExportArrowMaterial.SetVector("_NormalClipBack", ClippingPlaneBack.transform.up);
            ExportArrowMaterial.SetVector("_NormalClipRight", ClippingPlaneRight.transform.up);
            ExportArrowMaterial.SetVector("_NormalClipLeft", ClippingPlaneLeft.transform.up);

            WireframeMaterial.SetVector("_PositionClipBottom", ClippingPlaneBottom.transform.position);
            WireframeMaterial.SetVector("_PositionClipFront", ClippingPlaneFront.transform.position);
            WireframeMaterial.SetVector("_PositionClipBack", ClippingPlaneBack.transform.position);
            WireframeMaterial.SetVector("_PositionClipRight", ClippingPlaneRight.transform.position);
            WireframeMaterial.SetVector("_PositionClipLeft", ClippingPlaneLeft.transform.position);
            WireframeMaterial.SetVector("_NormalClipBottom", ClippingPlaneBottom.transform.up);
            WireframeMaterial.SetVector("_NormalClipFront", ClippingPlaneFront.transform.up);
            WireframeMaterial.SetVector("_NormalClipBack", ClippingPlaneBack.transform.up);
            WireframeMaterial.SetVector("_NormalClipRight", ClippingPlaneRight.transform.up);
            WireframeMaterial.SetVector("_NormalClipLeft", ClippingPlaneLeft.transform.up);

            if(ObjectStateSynchronizer.enabled)    
                ObjectStateSynchronizer.TransformChanged();
        }
    }

    public void ResetMaterials()
    {
        DefaultMaterial.SetVector("_PositionClipBottom", Vector3.zero);
        DefaultMaterial.SetVector("_PositionClipFront", Vector3.zero);
        DefaultMaterial.SetVector("_PositionClipBack", Vector3.zero);
        DefaultMaterial.SetVector("_PositionClipRight", Vector3.zero);
        DefaultMaterial.SetVector("_PositionClipLeft", Vector3.zero);
        DefaultMaterial.SetVector("_NormalClipBottom", Vector3.zero);
        DefaultMaterial.SetVector("_NormalClipFront", Vector3.zero);
        DefaultMaterial.SetVector("_NormalClipBack", Vector3.zero);
        DefaultMaterial.SetVector("_NormalClipRight", Vector3.zero);
        DefaultMaterial.SetVector("_NormalClipLeft", Vector3.zero);

        ArrowHeadMaterial.SetVector("_PositionClipBottom", Vector3.zero);
        ArrowHeadMaterial.SetVector("_PositionClipFront", Vector3.zero);
        ArrowHeadMaterial.SetVector("_PositionClipBack", Vector3.zero);
        ArrowHeadMaterial.SetVector("_PositionClipRight", Vector3.zero);
        ArrowHeadMaterial.SetVector("_PositionClipLeft", Vector3.zero);
        ArrowHeadMaterial.SetVector("_NormalClipBottom", Vector3.zero);
        ArrowHeadMaterial.SetVector("_NormalClipFront", Vector3.zero);
        ArrowHeadMaterial.SetVector("_NormalClipBack", Vector3.zero);
        ArrowHeadMaterial.SetVector("_NormalClipRight", Vector3.zero);
        ArrowHeadMaterial.SetVector("_NormalClipLeft", Vector3.zero);

        ImportArrowMaterial.SetVector("_PositionClipBottom", Vector3.zero);
        ImportArrowMaterial.SetVector("_PositionClipFront", Vector3.zero);
        ImportArrowMaterial.SetVector("_PositionClipBack", Vector3.zero);
        ImportArrowMaterial.SetVector("_PositionClipRight", Vector3.zero);
        ImportArrowMaterial.SetVector("_PositionClipLeft", Vector3.zero);
        ImportArrowMaterial.SetVector("_NormalClipBottom", Vector3.zero);
        ImportArrowMaterial.SetVector("_NormalClipFront", Vector3.zero);
        ImportArrowMaterial.SetVector("_NormalClipBack", Vector3.zero);
        ImportArrowMaterial.SetVector("_NormalClipRight", Vector3.zero);
        ImportArrowMaterial.SetVector("_NormalClipLeft", Vector3.zero);

        ExportArrowMaterial.SetVector("_PositionClipBottom", Vector3.zero);
        ExportArrowMaterial.SetVector("_PositionClipFront", Vector3.zero);
        ExportArrowMaterial.SetVector("_PositionClipBack", Vector3.zero);
        ExportArrowMaterial.SetVector("_PositionClipRight", Vector3.zero);
        ExportArrowMaterial.SetVector("_PositionClipLeft", Vector3.zero);
        ExportArrowMaterial.SetVector("_NormalClipBottom", Vector3.zero);
        ExportArrowMaterial.SetVector("_NormalClipFront", Vector3.zero);
        ExportArrowMaterial.SetVector("_NormalClipBack", Vector3.zero);
        ExportArrowMaterial.SetVector("_NormalClipRight", Vector3.zero);
        ExportArrowMaterial.SetVector("_NormalClipLeft", Vector3.zero);

        WireframeMaterial.SetVector("_PositionClipBottom", Vector3.zero);
        WireframeMaterial.SetVector("_PositionClipFront", Vector3.zero);
        WireframeMaterial.SetVector("_PositionClipBack", Vector3.zero);
        WireframeMaterial.SetVector("_PositionClipRight", Vector3.zero);
        WireframeMaterial.SetVector("_PositionClipLeft", Vector3.zero);
        WireframeMaterial.SetVector("_NormalClipBottom", Vector3.zero);
        WireframeMaterial.SetVector("_NormalClipFront", Vector3.zero);
        WireframeMaterial.SetVector("_NormalClipBack", Vector3.zero);
        WireframeMaterial.SetVector("_NormalClipRight", Vector3.zero);
        WireframeMaterial.SetVector("_NormalClipLeft", Vector3.zero);
    }
}
