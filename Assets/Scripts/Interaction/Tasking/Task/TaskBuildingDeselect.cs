using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskBuildingDeselect : DiscreteGestureInteractionTask
    {
        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            UpdateHighlights(eventArgs);
            yield return null;
        }

        private void UpdateHighlights(GestureInteractionEventArgs eventArgs)
        {
            Building building = eventArgs.Selected.GetComponent<Building>();
            UIManager.Instance.SetPanelHeaderTop("Compilation Unit");
            UIManager.Instance.SetPanelHeaderBottom(building.name);
            UIManager.Instance.SetActivePanelBody(PanelItem.BodyClass);
            //UIManager.Instance.ActivatePanelBody(PanelItem.BodyClass);
            eventArgs.Selected.Highlight.gameObject.SetActive(false);
            eventArgs.Selected.OnDeselect();

            if (eventArgs.Focused.Type == InteractableType.Package)
            {
                Region region = eventArgs.Focused.GetComponent<Region>();

                if(region != building.Region)
                {
                    building.Region.RegionLevelCollider.enabled = true;
                    building.Region.BuildingLevelCollider.enabled = false;

                    foreach (Building b in building.Region.Buildings)
                        b.GetComponent<Collider>().enabled = false;

                    var regionInteractable = building.Region.GetComponent<Interactable>();
                    var currentRenderer = regionInteractable.Highlight.GetComponent<MeshRenderer>();
                    currentRenderer.sharedMaterial = UIManager.Instance.WireframeFocused;
                    regionInteractable.Highlight.gameObject.SetActive(false);
                    regionInteractable.OnDeselect();
                }

                region.RegionLevelCollider.enabled = false;
                region.BuildingLevelCollider.enabled = true;

                foreach (Building b in region.Buildings)
                    b.GetComponent<Collider>().enabled = true;

                UIManager.Instance.SetActivePanelBody(PanelItem.BodyPackage);
                //UIManager.Instance.ActivatePanelBody(PanelItem.BodyPackage);
                var interactable = region.GetComponent<Interactable>();
                var renderer = interactable.Highlight.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = UIManager.Instance.WireframeSelected;
                interactable.Highlight.gameObject.SetActive(true);
                eventArgs.Focused.OnSelect();
            }
        }
    }
}
