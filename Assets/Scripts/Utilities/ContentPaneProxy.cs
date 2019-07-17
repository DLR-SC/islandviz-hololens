using HoloIslandVis.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentPaneProxy : MonoBehaviour
{
	void Start ()
    {
        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("ContentPaneProxy"), 
            LayerMask.NameToLayer("Default")
        );

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("ContentPaneProxy"),
            LayerMask.NameToLayer("Spatial Mapping")
        );

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("ContentPaneProxy"),
            LayerMask.NameToLayer("WaterProxy")
        );

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("ContentPaneProxy"),
            LayerMask.NameToLayer("BoundingBox")
        );

        Physics.IgnoreLayerCollision(
            LayerMask.NameToLayer("WaterProxy"),
            LayerMask.NameToLayer("Default")
        );
    }
	
	void Update ()
    {
        if (transform.hasChanged)
        {
            UIManager.Instance.SetContentPaneTransform(transform);
            transform.hasChanged = false;
        }
    }
}
