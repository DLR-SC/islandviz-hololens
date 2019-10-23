using HoloIslandVis.Core;
using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Interaction;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Sharing;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using HoloIslandVis.UI.Info;
using HoloIslandVis.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TaskSelectSmallest : DiscreteGestureInteractionTask
{
    private Visualization _visualization;
    private ContentPane _contentPane;
    private Interactable _focused;

    private Bounds _paneBounds;
    private Bounds _focusedBounds;
    private float _maxPaneExtent;
    private float _maxFocusedExtent;

    private Vector3 _visualizationPosition;
    private Vector3 _visualizationScale;

    private Vector3 _focusedPosition;

    private Vector3 _targetPosition;
    private float _targetScale;


    public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
    {
        _focused = eventArgs.Focused;
        _focused.OnSelect();

        yield return Perform(_focused);
    }

    public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
    {
        var bundles = GameObject.Find("Application").GetComponent<AppManager>().CurrentProject.Bundles;
        int min = bundles.Min(b => b.CompilationUnitCount);
        var bundle = bundles.First(b => b.CompilationUnitCount == min);

        _focused = GameObject.Find(bundle.Name).GetComponent<Interactable>();
        yield return Perform(_focused);
    }

    public IEnumerator Perform(Interactable focused)
    {
        UpdateHighlights(focused);

        _visualization = UIManager.Instance.Visualization;
        _visualization.GetComponent<ObjectStateSynchronizer>().SyncActive = false;
        _contentPane = UIManager.Instance.ContentPane;

        _visualizationPosition = _visualization.transform.localPosition;
        _visualizationScale = _visualization.transform.localScale;

        _paneBounds = GameObject.Find("Water").GetComponent<MeshRenderer>().bounds;
        _focusedBounds = GetInteractableBounds(_focused);

        _maxPaneExtent = Mathf.Max(_paneBounds.extents.x, _paneBounds.extents.z);
        _maxFocusedExtent = Mathf.Max(_focusedBounds.extents.x, _focusedBounds.extents.z);
        _targetScale = (_maxPaneExtent / _maxFocusedExtent) * 0.8f;

        _focusedPosition = _contentPane.transform.InverseTransformPoint(_focusedBounds.center);
        _focusedPosition = new Vector3(_focusedPosition.x, 0, _focusedPosition.z);
        _targetPosition = _visualizationPosition - _focusedPosition;
        yield return CenterIsland();
    }

    private IEnumerator CenterIsland()
    {
        Vector3 _tempPosition = _visualizationPosition;
        while (Vector3.Distance(_tempPosition, _targetPosition) > 0.01f)
        {
            float currentTime = (Time.deltaTime / 0.25f);
            _tempPosition = Vector3.Lerp(_tempPosition, _targetPosition, currentTime);
            _visualization.transform.localPosition = _tempPosition;

            yield return null;
        }

        _visualization.transform.localPosition = _targetPosition;

        float tempScale = 1.0f;

        while ((_targetScale - tempScale) > 0.01f)
        {
            float currentTime = (Time.deltaTime / 1.0f);
            tempScale = Mathf.Lerp(tempScale, _targetScale, 0.1f);
            SetScaleRelativeToCenter(tempScale);

            yield return null;
        }

        SetScaleRelativeToCenter(_targetScale);
        _visualization.GetComponent<ObjectStateSynchronizer>().SyncActive = true;
    }

    private Bounds GetInteractableBounds(Interactable interactable)
    {
        var renderers = interactable.GetComponentsInChildren<MeshRenderer>();
        Bounds result = renderers[0].bounds;

        foreach (MeshRenderer renderer in renderers)
            result.Encapsulate(renderer.bounds);

        return result;
    }

    private void SetScaleRelativeToCenter(float scale)
    {
        _visualization.transform.localScale = _visualizationScale * scale;
        Vector3 positionDiff = _targetPosition * scale - _targetPosition;
        _visualization.transform.localPosition = _targetPosition + positionDiff;
    }

    private void UpdateHighlights(Interactable focused)
    {
        var island = focused.GetComponent<Island>();
        UIManager.Instance.SetPanelHeaderTop("Bundle");
        UIManager.Instance.SetPanelHeaderBottom(island.name);
        UIManager.Instance.SetActivePanelBody(PanelItem.BodyBundle);
        //UIManager.Instance.ActivatePanelBody(PanelItem.BodyBundle);

        island.IslandLevelCollider.enabled = false;
        island.PackageLevelCollider.enabled = true;

        foreach (Region region in island.Regions)
            region.RegionLevelCollider.enabled = true;

        focused.gameObject.layer = LayerMask.NameToLayer("Default");
        focused.Highlight.gameObject.SetActive(false);
        focused.OnSelect();
    }
}