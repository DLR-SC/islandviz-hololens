using HoloIslandVis.Controller;
using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Interaction;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Core.Builders
{
    public class HighlightObjectBuilder : SingletonComponent<HighlightObjectBuilder>
    {
        public delegate void HighlightObjectsBuiltHandler();
        public event HighlightObjectsBuiltHandler HighlightObjectsBuilt = delegate { };

        public AppConfig AppConfig;
        public Material DefaultHighlightMaterial;
        public Material BuildingHighlightMaterial;

        private List<MeshFilter> _islandFilters;
        private List<MeshFilter> _regionFilters;
        private List<MeshFilter> _buildingFilters;

        private void Start()
        {
            _islandFilters = new List<MeshFilter>();
            _regionFilters = new List<MeshFilter>();
            _buildingFilters = new List<MeshFilter>();
        }

        public IEnumerator Build(List<Island> islands)
        {
            foreach (Island island in islands)
            {
                yield return BuildIslandHightlight(island);
            }

            HighlightObjectsBuilt();
        }

        public IEnumerator BuildIslandHightlight(Island island)
        {
            GameObject highlight = new GameObject("Island Highlight");
            yield return BuildHighlight(island.gameObject, highlight);
            MeshRenderer renderer = highlight.GetComponent<MeshRenderer>();
            Interactable interactable = island.GetComponent<Interactable>();

            foreach (Region region in island.Regions)
                yield return BuildRegionHighlight(region);

            if (true)
            {
                interactable.FocusBehaviour = new Action(() => {
                    if(StateManager.Instance.CurrentState.Name == "main" && interactable.Type == InteractableType.Bundle)
                    {
                        UIManager.Instance.SetPanelHeaderTop("Bundle");
                        UIManager.Instance.SetPanelHeaderBottom(island.name);
                        UIManager.Instance.Tooltip.Show(island.gameObject);
                    }

                    UIManager.Instance.Tooltip.Show(island.gameObject);
                    interactable.Highlight.gameObject.SetActive(true);
                });

                interactable.DefocusBehaviour = new Action(() => {
                    if (StateManager.Instance.CurrentState.Name == "main" && interactable.Type == InteractableType.Bundle)
                    {
                        UIManager.Instance.SetPanelHeaderTop("");
                        UIManager.Instance.SetPanelHeaderBottom("");
                    }

                    UIManager.Instance.Tooltip.Hide();
                    interactable.Highlight.gameObject.SetActive(false);
                });

                interactable.SelectBehaviour = new Action(() => {
                    UIManager.Instance.SetPanelHeaderTop("Bundle");
                    UIManager.Instance.SetPanelHeaderBottom(island.name);
                    UIManager.Instance.Tooltip.Hide();
                });

                interactable.DeselectBehaviour = new Action(() => {
                    UIManager.Instance.Tooltip.Hide();
                });
            }
        }

        public IEnumerator BuildRegionHighlight(Region region)
        {
            GameObject highlight = new GameObject("Region Highlight");
            yield return BuildHighlight(region.gameObject, highlight);
            MeshRenderer renderer = highlight.GetComponent<MeshRenderer>();
            Interactable interactable = region.GetComponent<Interactable>();

            foreach (Building building in region.Buildings)
                yield return BuildBuildingHighlight(building);

            if (true)
            {
                interactable.FocusBehaviour = new Action(() => {
                    if (StateManager.Instance.CurrentState.Name == "inspect island" && interactable.Type == InteractableType.Package)
                    {
                        UIManager.Instance.SetPanelHeaderTop("Package");
                        UIManager.Instance.SetPanelHeaderBottom(region.name);
                    }
                    if (StateManager.Instance.CurrentState.Name == "inspect region" && interactable.Type == InteractableType.Package)
                    {
                        UIManager.Instance.SetPanelHeaderTop("Package");
                        UIManager.Instance.SetPanelHeaderBottom(region.name);
                    }

                    UIManager.Instance.Tooltip.Show(region.gameObject);
                    interactable.Highlight.gameObject.SetActive(true);
                });

                interactable.DefocusBehaviour = new Action(() => {
                    if (StateManager.Instance.CurrentState.Name == "inspect island" && interactable.Type == InteractableType.Package)
                    {
                        UIManager.Instance.SetPanelHeaderTop("Bundle");
                        UIManager.Instance.SetPanelHeaderBottom(region.Island.name);
                    }
                    if (StateManager.Instance.CurrentState.Name == "inspect region" && interactable.Type == InteractableType.Package)
                    {
                        Context context = ContextManager.Instance.SafeContext;
                        Region selectedRegion = context.Selected.GetComponent<Region>();

                        UIManager.Instance.SetPanelHeaderTop("Package");
                        UIManager.Instance.SetPanelHeaderBottom(selectedRegion.name);
                    }

                    UIManager.Instance.Tooltip.Hide();
                    interactable.Highlight.gameObject.SetActive(false);
                });

                interactable.SelectBehaviour = new Action(() => {
                    UIManager.Instance.SetPanelHeaderTop("Package");
                    UIManager.Instance.SetPanelHeaderBottom(region.name);
                    UIManager.Instance.Tooltip.Hide();
                });

                interactable.DeselectBehaviour = new Action(() => {
                    UIManager.Instance.Tooltip.Hide();
                });
            }

            
        }

        public IEnumerator BuildBuildingHighlight(Building building)
        {
            GameObject highlight = new GameObject("Building Highlight");
            yield return BuildHighlight(building.gameObject, highlight);
            MeshRenderer renderer = highlight.GetComponent<MeshRenderer>();
            Interactable interactable = building.GetComponent<Interactable>();

            if (true)
            {
                interactable.FocusBehaviour = new Action(() => {
                    UIManager.Instance.SetPanelHeaderTop("Compilation Unit");
                    UIManager.Instance.SetPanelHeaderBottom(building.name);

                    UIManager.Instance.Tooltip.Show(building.gameObject);
                    interactable.Highlight.gameObject.SetActive(true);
                });

                interactable.DefocusBehaviour = new Action(() => {
                    if (StateManager.Instance.CurrentState.Name == "inspect region")
                    {
                        Context context = ContextManager.Instance.SafeContext;
                        Region selectedRegion = context.Selected.GetComponent<Region>();

                        UIManager.Instance.SetPanelHeaderTop("Package");
                        UIManager.Instance.SetPanelHeaderBottom(selectedRegion.name);
                    }
                    else if (StateManager.Instance.CurrentState.Name == "inspect building")
                    {
                        Context context = ContextManager.Instance.SafeContext;
                        Building selectedBuilding = context.Selected.GetComponent<Building>();

                        UIManager.Instance.SetPanelHeaderTop("Compilation Unit");
                        UIManager.Instance.SetPanelHeaderBottom(selectedBuilding.name);
                    }

                    UIManager.Instance.Tooltip.Hide();
                    interactable.Highlight.gameObject.SetActive(false);
                });

                interactable.SelectBehaviour = new Action(() => {
                    UIManager.Instance.SetPanelHeaderTop("Compilation Unit");
                    UIManager.Instance.SetPanelHeaderBottom(building.name);
                    UIManager.Instance.Tooltip.Hide();
                });

                interactable.DeselectBehaviour = new Action(() => {
                    UIManager.Instance.Tooltip.Hide();
                });
            }
        }

        public IEnumerator BuildHighlight(GameObject gameObj, GameObject highlight)
        {
            MeshFilter[] meshFilters = gameObj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combineInstance = new CombineInstance[meshFilters.Length];
            Interactable interactable = gameObj.GetComponent<Interactable>();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                combineInstance[i].mesh = meshFilters[i].sharedMesh;

                if (interactable.Type == InteractableType.CompilationUnit)
                {
                    combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1.15f, 1.15f, 1.15f));
                }
                else
                {
                    combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(1.003f, 1.003f, 1.003f));
                    //combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix * Matrix4x4.Translate(new Vector3(0.0f, 0.03f, 0.0f));
                }
            }

            Highlight highlightComponent = highlight.AddComponent<Highlight>();
            interactable.Highlight = highlightComponent;

            MeshFilter meshFilter = highlight.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = highlight.AddComponent<MeshRenderer>();
            meshFilter.mesh.CombineMeshes(combineInstance);
            highlight.transform.parent = gameObj.transform;

            // Enlarge and displace highlights slightly to avoid Z-Fighting.
            highlight.transform.localPosition += new Vector3(0.000f, 0.007f, 0.000f);

            if(interactable.Type == InteractableType.CompilationUnit)
                meshRenderer.sharedMaterial = BuildingHighlightMaterial;
            else
                meshRenderer.sharedMaterial = DefaultHighlightMaterial;

            highlight.SetActive(false);
            yield return null;
        }
    }
}
