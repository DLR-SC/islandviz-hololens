using HoloIslandVis.Core;
using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Interaction;
using HoloIslandVis.UI.Component;
using HoloIslandVis.UI.Info;
using HoloToolkit.Unity.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.UI
{
    public enum UIElement
    {
        ContentPane,
        Visualization,
        BundleContainer,
        DependencyContainer,
        ServiceContainer,
        ContentPaneFrame,
        BoundingBoxRig,
        SettingsPanel,
        InfoPanel,
        Toolbar
    }

    public enum UIWidget
    {
        SettingsDropdown,
        SettingsStart
    }

    public class UIManager : SingletonComponent<UIManager>
    { 
        public Material WireframeFocused;
        public Material WireframeSelected;

        public Visualization Visualization;
        public BundleContainer BundleContainer;
        public DependencyContainer DependencyContainer;
        public ServiceContainer ServiceContainer;
        public ContentPane ContentPane;
        public Tooltip Tooltip;
        public InfoPanel Panel;
        public Frame Frame;
        public Toolbar Toolbar;

        private Dictionary<UIElement, UIComponent> _uiComponents;

        private ContentPaneProxy _contentPaneProxy;
        private LoadingIcon _loadingIcon;
        private BoundingBox _boundingBox;
        private BoundingBoxRig _bbRig;
        private AppBar _appBar;

        void Start()
        {
            _uiComponents = new Dictionary<UIElement, UIComponent>();

            _uiComponents.Add(UIElement.ContentPane, GameObject.Find("ContentPane").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.Visualization, GameObject.Find("Visualization").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.BundleContainer, GameObject.Find("BundleContainer").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.DependencyContainer, GameObject.Find("DependencyContainer").GetComponent<UIComponent>());
            //_uiComponents.Add(UIElement.ServiceContainer, GameObject.Find("ServiceContainer").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.BoundingBoxRig, GameObject.Find("BoundingBoxWrapper").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.SettingsPanel, GameObject.Find("SettingsPanel").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.Toolbar, GameObject.Find("Toolbar").GetComponent<UIComponent>());
            _uiComponents.Add(UIElement.InfoPanel, GameObject.Find("InfoPanel").GetComponent<UIComponent>());

            foreach (UIElement element in _uiComponents.Keys)
                Deactivate(element);
        }

        public void Activate(UIElement element)
        {
            UIComponent component = _uiComponents[element];

            if (!component.isActiveAndEnabled)
                component.gameObject.SetActive(true);

            StartCoroutine(component.Activate());
        }

        public void Deactivate(UIElement element)
        {
            UIComponent component = _uiComponents[element];

            if (!component.isActiveAndEnabled)
                return;

            StartCoroutine(component.Deactivate());
        }

        public UIComponent GetUIElement(UIElement element)
        {
            if (_uiComponents.ContainsKey(element))
                return _uiComponents[element];
            else return null;
        }

        public void SetPanelHeaderTop(string text)
        {
            Panel.SetHeaderTextTop(text);
        }

        public void SetPanelHeaderBottom(string text)
            => Panel.SetHeaderTextBottom(text);

        public void SetActivePanelBody(PanelItem panelItem)
            => Panel.PanelBody.SetActivePanelBody(panelItem);

        public void ActivatePanelBody(PanelItem panelItem)
            => Panel.PanelBody.Activate(panelItem);

        public void DeactivatePanelBody(PanelItem panelItem)
            => Panel.PanelBody.Deactivate(panelItem);

        public void BuildPanelBody(PanelItem panelItem, Interactable selected)
            => Panel.PanelBody.Build(panelItem, selected);

        public void SetContentPaneTransform(Transform transform)
        {
            ContentPane.transform.position = transform.position;
            ContentPane.transform.rotation = transform.rotation;
            ContentPane.transform.localScale = transform.localScale;
        }

        public IEnumerator PostponeExecution(Action action)
        {
            while (Frame.Activating)
                yield return null;

            action.Invoke();
        }

        public void EnableToolbar(bool blocking)
        {
            EnableToolbarImpl();
        }

        public void SetActiveButtons(params StaticItem[] buttons)
        {
            Toolbar.SetActiveButtons(buttons);
        }

        public void DisableToolbar(bool blocking)
        {
            DisableToolbarImpl();
        }

        private void EnableToolbarImpl()
        {
            Frame.Activate();
        }

        private void DisableToolbarImpl()
        {
            Frame.Deactivate();
        }
    }
}
