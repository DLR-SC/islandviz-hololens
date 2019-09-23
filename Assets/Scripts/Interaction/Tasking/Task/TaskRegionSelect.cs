using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskRegionSelect : DiscreteGestureInteractionTask
    {
        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            UpdateHighlights(eventArgs);
            yield return null;
        }
        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            UpdateHighlights(eventArgs);
            yield return null;
        }

        private void UpdateHighlights(GestureInteractionEventArgs eventArgs)
        {
            if (eventArgs.Selected.Type == InteractableType.Package)
            {
                UIManager.Instance.DisableToolbar(true);
                UIManager.Instance.SetActivePanelBody(PanelItem.BodyPackage);
                UIManager.Instance.DeactivatePanelBody(PanelItem.BodyPackage);

                Region currentRegion = eventArgs.Selected.GetComponent<Region>();

                currentRegion.RegionLevelCollider.enabled = true;
                currentRegion.BuildingLevelCollider.enabled = false;

                foreach (Building building in currentRegion.Buildings)
                    building.GetComponent<Collider>().enabled = false;

                var currentRenderer = eventArgs.Selected.Highlight.GetComponent<MeshRenderer>();
                currentRenderer.sharedMaterial = UIManager.Instance.WireframeFocused;
                eventArgs.Selected.Highlight.gameObject.SetActive(false);
                eventArgs.Selected.OnDeselect();
                eventArgs.Focused.OnSelect();
                UIManager.Instance.EnableToolbar(true);
            }

            Region region = eventArgs.Focused.GetComponent<Region>();
            UIManager.Instance.SetPanelHeaderTop("Package");
            UIManager.Instance.SetPanelHeaderBottom(region.name);
            UIManager.Instance.SetActivePanelBody(PanelItem.BodyPackage);
            //UIManager.Instance.ActivatePanelBody(PanelItem.BodyPackage);

            region.RegionLevelCollider.enabled = false;
            region.BuildingLevelCollider.enabled = true;

            foreach (Building building in region.Buildings)
                building.GetComponent<Collider>().enabled = true;

            var renderer = eventArgs.Focused.Highlight.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = UIManager.Instance.WireframeSelected;
            eventArgs.Focused.Highlight.gameObject.SetActive(true);
            eventArgs.Focused.OnSelect();
        }

        private void UpdateHighlights(SpeechInteractionEventArgs eventArgs)
        {
            if (eventArgs.Selected.Type == InteractableType.Package)
            {
                UIManager.Instance.DisableToolbar(true);
                UIManager.Instance.SetActivePanelBody(PanelItem.BodyPackage);
                UIManager.Instance.DeactivatePanelBody(PanelItem.BodyPackage);

                Region currentRegion = eventArgs.Selected.GetComponent<Region>();

                currentRegion.RegionLevelCollider.enabled = true;
                currentRegion.BuildingLevelCollider.enabled = false;

                foreach (Building building in currentRegion.Buildings)
                    building.GetComponent<Collider>().enabled = false;

                var currentRenderer = eventArgs.Selected.Highlight.GetComponent<MeshRenderer>();
                currentRenderer.sharedMaterial = UIManager.Instance.WireframeFocused;
                eventArgs.Selected.Highlight.gameObject.SetActive(false);
                eventArgs.Selected.OnDeselect();
                eventArgs.Focused.OnSelect();
                UIManager.Instance.EnableToolbar(true);
            }

            Region region = eventArgs.Focused.GetComponent<Region>();
            UIManager.Instance.SetPanelHeaderTop("Package");
            UIManager.Instance.SetPanelHeaderBottom(region.name);
            UIManager.Instance.SetActivePanelBody(PanelItem.BodyPackage);
            //UIManager.Instance.ActivatePanelBody(PanelItem.BodyPackage);

            region.RegionLevelCollider.enabled = false;
            region.BuildingLevelCollider.enabled = true;

            foreach (Building building in region.Buildings)
                building.GetComponent<Collider>().enabled = true;

            var renderer = eventArgs.Focused.Highlight.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = UIManager.Instance.WireframeSelected;
            eventArgs.Focused.Highlight.gameObject.SetActive(true);
            eventArgs.Focused.OnSelect();
        }
    }
}
