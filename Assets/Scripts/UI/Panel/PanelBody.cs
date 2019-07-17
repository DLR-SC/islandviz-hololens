using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI.Info
{
    public class PanelBody : MonoBehaviour
    {
        private static Dictionary<PanelItem, GameObject> _panelBodys;

        public static PanelItem ActiveBody;
        public PanelBodyBundle PanelBodyBundle;
        public PanelBodyPackage PanelBodyPackage;
        public PanelBodyClass PanelBodyClass;

        public void Start()
        {
            _panelBodys = new Dictionary<PanelItem, GameObject>();
            _panelBodys.Add(PanelItem.BodyBundle, PanelBodyBundle.gameObject);
            _panelBodys.Add(PanelItem.BodyPackage, PanelBodyPackage.gameObject);
            _panelBodys.Add(PanelItem.BodyClass, PanelBodyClass.gameObject);
        }

        public void SetActivePanelBody(PanelItem panelItem)
        {
            ActiveBody = panelItem;
        }

        public void Activate(PanelItem panelItem)
        {
            gameObject.SetActive(true);
            foreach (var panelBody in _panelBodys.Values)
                panelBody.SetActive(false);

            if(_panelBodys.ContainsKey(panelItem))
                _panelBodys[panelItem].SetActive(true);
        }

        public void Deactivate(PanelItem panelItem)
        {
            gameObject.SetActive(false);
        }

        public void Build(PanelItem panelItem, Interactable selected)
        {
            if (selected.Type == InteractableType.Bundle) PanelBodyBundle.Build(selected);
            else if (selected.Type == InteractableType.Package) PanelBodyPackage.Build(selected);
            else if (selected.Type == InteractableType.CompilationUnit) PanelBodyClass.Build(selected);
        }

        public PanelItem GetActivePanelBody()
        {
            if (PanelBodyBundle.gameObject.activeSelf)
                return PanelItem.BodyBundle;

            else if (PanelBodyPackage.gameObject.activeSelf)
                return PanelItem.BodyPackage;

            else if (PanelBodyClass.gameObject.activeSelf)
                return PanelItem.BodyClass;

            else // No panel body is active
                return PanelItem.None;
        }


    }
}
