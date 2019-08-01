using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Sharing;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using HoloIslandVis.UI.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskIslandDeselect : DiscreteGestureInteractionTask
    {
        private Visualization _visualization;
        private Vector3 _visualizationPosition;
        private Vector3 _visualizationScale;
        private float _startScale;
        private float _targetScale;

        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            UpdateHighlights(eventArgs);

            Debug.Log("Deselect");

            _visualization = UIManager.Instance.Visualization;
            _visualization.GetComponent<ObjectStateSynchronizer>().SyncActive = false;
            _visualizationPosition = _visualization.transform.localPosition;
            _visualizationScale = _visualization.transform.localScale;

            _startScale = _visualizationScale.x;
            _targetScale = 0.01f;

            yield return ReturnToDefaultScale();
        }

        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            UpdateHighlights(eventArgs);

            Debug.Log("Deselect");

            _visualization = UIManager.Instance.Visualization;
            _visualization.GetComponent<ObjectStateSynchronizer>().SyncActive = false;
            _visualizationPosition = _visualization.transform.localPosition;
            _visualizationScale = _visualization.transform.localScale;

            _startScale = _visualizationScale.x;
            _targetScale = 0.01f;

            yield return ReturnToDefaultScale();
        }

        private IEnumerator ReturnToDefaultScale()
        {
            float tempScale = _startScale;
            float scalingFactor = tempScale / _startScale;

            while ((tempScale - _targetScale) > 0.0001f)
            {

                float currentTime = (Time.deltaTime / 1.0f);
                tempScale = Mathf.Lerp(tempScale, _targetScale, 0.1f);
                scalingFactor = tempScale / _startScale;
                SetScaleRelativeToCenter(scalingFactor);

                yield return null;
            }

            scalingFactor = _targetScale / _startScale;
            _visualization.GetComponent<ObjectStateSynchronizer>().SyncActive = true;
            SetScaleRelativeToCenter(scalingFactor);
            yield break;
        }

        private void SetScaleRelativeToCenter(float scale)
        {

            _visualization.transform.localScale = _visualizationScale * scale;
            Vector3 positionDiff = _visualizationPosition * scale - _visualizationPosition;
            _visualization.transform.localPosition = _visualizationPosition + positionDiff;
        }

        private void UpdateHighlights(GestureInteractionEventArgs eventArgs)
        {
            Island island = null;
            Region region = null;
            Building building = null;

            if (eventArgs.Selected.Type == InteractableType.Bundle)
            {
                island = eventArgs.Selected.GetComponent<Island>();
            }
            else if (eventArgs.Selected.Type == InteractableType.Package)
            {
                region = eventArgs.Selected.GetComponent<Region>();
                island = region.Island;
            }
            else if (eventArgs.Selected.Type == InteractableType.CompilationUnit)
            {
                building = eventArgs.Selected.GetComponent<Building>();
                region = building.Region;
                island = region.Island;
            }

            UIManager.Instance.SetPanelHeaderTop("");
            UIManager.Instance.SetPanelHeaderBottom("");
            UIManager.Instance.SetActivePanelBody(PanelItem.None);
            UIManager.Instance.DeactivatePanelBody(PanelItem.BodyBundle);

            island.PackageLevelCollider.enabled = false;
            island.IslandLevelCollider.enabled = true;

            foreach (Region r in island.Regions)
                r.RegionLevelCollider.enabled = false;

            eventArgs.Selected.Highlight.gameObject.SetActive(false);

            if (island != null)
                island.GetComponent<Interactable>().OnDeselect();

            if (region != null)
            {
                region.RegionLevelCollider.enabled = true;
                region.BuildingLevelCollider.enabled = false;

                foreach (Building b in region.Buildings)
                {
                    b.GetComponent<Collider>().enabled = false;
                    b.GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
                }

                var interactable = region.GetComponent<Interactable>();
                var renderer = interactable.Highlight.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = UIManager.Instance.WireframeFocused;

                interactable.Highlight.gameObject.SetActive(false);
                region.GetComponent<Interactable>().OnDeselect();
            }

            if (building != null)
            {
                var buildingInteractable = building.GetComponent<Interactable>();
                buildingInteractable.Highlight.gameObject.SetActive(false);
                building.GetComponent<Interactable>().OnDeselect();
            }

            if(eventArgs.Focused.Highlight != null)
                eventArgs.Focused.Highlight.gameObject.SetActive(false);

            UIManager.Instance.SetPanelHeaderTop("");
            UIManager.Instance.SetPanelHeaderBottom("");
        }

        private void UpdateHighlights(SpeechInteractionEventArgs eventArgs)
        {
            Island island = null;
            Region region = null;
            Building building = null;

            if (eventArgs.Selected.Type == InteractableType.Bundle)
            {
                island = eventArgs.Selected.GetComponent<Island>();
            }
            else if (eventArgs.Selected.Type == InteractableType.Package)
            {
                region = eventArgs.Selected.GetComponent<Region>();
                island = region.Island;
            }
            else if (eventArgs.Selected.Type == InteractableType.CompilationUnit)
            {
                building = eventArgs.Selected.GetComponent<Building>();
                region = building.Region;
                island = region.Island;
            }

            UIManager.Instance.SetPanelHeaderTop("");
            UIManager.Instance.SetPanelHeaderBottom("");
            UIManager.Instance.SetActivePanelBody(PanelItem.None);
            UIManager.Instance.DeactivatePanelBody(PanelItem.BodyBundle);

            island.PackageLevelCollider.enabled = false;
            island.IslandLevelCollider.enabled = true;

            foreach (Region r in island.Regions)
                r.RegionLevelCollider.enabled = false;

            eventArgs.Selected.Highlight.gameObject.SetActive(false);

            if (island != null)
                island.GetComponent<Interactable>().OnDeselect();

            if (region != null)
            {
                region.RegionLevelCollider.enabled = true;
                region.BuildingLevelCollider.enabled = false;

                foreach (Building b in region.Buildings)
                {
                    b.GetComponent<Collider>().enabled = false;
                    b.GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
                }

                var interactable = region.GetComponent<Interactable>();
                var renderer = interactable.Highlight.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = UIManager.Instance.WireframeFocused;

                interactable.Highlight.gameObject.SetActive(false);
                region.GetComponent<Interactable>().OnDeselect();
            }

            if (building != null)
            {
                var buildingInteractable = building.GetComponent<Interactable>();
                buildingInteractable.Highlight.gameObject.SetActive(false);
                building.GetComponent<Interactable>().OnDeselect();
            }

            if (eventArgs.Focused.Highlight != null)
                eventArgs.Focused.Highlight.gameObject.SetActive(false);

            UIManager.Instance.SetPanelHeaderTop("");
            UIManager.Instance.SetPanelHeaderBottom("");
        }
    }
}