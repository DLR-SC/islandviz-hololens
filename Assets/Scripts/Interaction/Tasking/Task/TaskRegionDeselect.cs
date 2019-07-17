using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskRegionDeselect : DiscreteGestureInteractionTask
    {
        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            UpdateHighlights(eventArgs);
            yield return null;
        }

        private void UpdateHighlights(GestureInteractionEventArgs eventArgs)
        {
            Region region = null;
            Building building = null;

            if (eventArgs.Selected.Type == InteractableType.Package)
            {
                region = eventArgs.Selected.GetComponent<Region>();
            }
            else if (eventArgs.Selected.Type == InteractableType.CompilationUnit)
            {
                building = eventArgs.Selected.GetComponent<Building>();
                region = building.Region;
            }

            UIManager.Instance.SetPanelHeaderTop("Bundle");
            UIManager.Instance.SetPanelHeaderBottom(region.Island.name);
            UIManager.Instance.DeactivatePanelBody(PanelItem.BodyPackage);

            region.RegionLevelCollider.enabled = true;
            region.BuildingLevelCollider.enabled = false;

            foreach (Building b in region.Buildings)
                b.GetComponent<Collider>().enabled = false;

            var interactable = region.GetComponent<Interactable>();
            var renderer = interactable.Highlight.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = UIManager.Instance.WireframeFocused;
            interactable.Highlight.gameObject.SetActive(false);
            interactable.OnDeselect();

            if(building != null)
            {
                var buildingInteractable = building.GetComponent<Interactable>();
                buildingInteractable.Highlight.gameObject.SetActive(false);
                building.GetComponent<Interactable>().OnDeselect();
            }


            if (eventArgs.Focused.Type == InteractableType.Bundle)
            {
                UIManager.Instance.SetActivePanelBody(PanelItem.BodyBundle);
                //UIManager.Instance.ActivatePanelBody(PanelItem.BodyBundle);
                eventArgs.Focused.OnSelect();
            }
            else if (eventArgs.Focused.Type == InteractableType.ContentPane)
            {
                UIManager.Instance.SetActivePanelBody(PanelItem.None);
                //UIManager.Instance.ActivatePanelBody(PanelItem.None);
                region.Island.PackageLevelCollider.enabled = false;
                region.Island.IslandLevelCollider.enabled = true;
            }
            else if (eventArgs.Focused.Type == InteractableType.None)
            {
                UIManager.Instance.SetActivePanelBody(PanelItem.None);
                //UIManager.Instance.ActivatePanelBody(PanelItem.None);
                region.Island.PackageLevelCollider.enabled = false;
                region.Island.IslandLevelCollider.enabled = true;
            }
        }
    }
}
