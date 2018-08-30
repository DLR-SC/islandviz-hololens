using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlightable : MonoBehaviour {

    private GameObject _highlightedInteractable;

	// Use this for initialization
	void Start ()
    {
        _highlightedInteractable = new GameObject(name + "- Highlight");
        _highlightedInteractable.transform.SetParent(transform.parent);
        _highlightedInteractable.transform.localPosition = transform.localPosition;
        _highlightedInteractable.transform.localRotation = transform.localRotation;
        _highlightedInteractable.transform.localScale = transform.localScale;

        MeshFilter filterHighlight = _highlightedInteractable.AddComponent<MeshFilter>();
        MeshFilter filterOriginal = GetComponent<MeshFilter>();
        filterHighlight.mesh = filterOriginal.mesh;

        MeshRenderer rendererHighlight = _highlightedInteractable.AddComponent<MeshRenderer>();
        Material[] highlightMaterials = new Material[filterHighlight.mesh.subMeshCount];
        for (int i = 0; i < highlightMaterials.Length; i++)
            highlightMaterials[i] = RuntimeCache.Instance.HighlightMaterial;

        rendererHighlight.sharedMaterials = highlightMaterials;
    }
}
