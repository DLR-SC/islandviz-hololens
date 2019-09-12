using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Sharing;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using HoloIslandVis.UI.Info;
using HoloIslandVis.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskIslandSelect : DiscreteGestureInteractionTask
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
            ScenarioHandler.current_bundle = eventArgs.Focused.name;
            ScenarioHandler.keywordsGesture.Add("Select[Island]");
            ScenarioHandler.IncrementCounterGestureControl();

            _focused = eventArgs.Focused;
            _focused.OnSelect();

            yield return Perform(_focused);
        }

        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            ScenarioHandler.IncrementCounterVoiceControl();
            ScenarioHandler.current_bundle = eventArgs.Focused.name;
            ScenarioHandler.keywordsSpeech.Add("Select[Island]");

            _focused = eventArgs.Focused;
            _focused.OnSelect();

            yield return Perform(_focused);
        }

        public IEnumerator Perform(Interactable focused)
        {
            UpdateHighlights(focused);
            if (ScenarioHandler.name_highlighted_island != "")
            {
                Debug.Log("Highlight island");
                GameObject.Find(ScenarioHandler.name_highlighted_island).GetComponent<Interactable>().Highlight.gameObject.SetActive(false);
            }

            //Check_goal_reached(focused);

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

        public void Check_goal_reached(Interactable focused)
        {
            switch (ScenarioHandler.scenario)
            {
                case ScenarioHandler.Scenario_type.FIRST:
                    switch (ScenarioHandler.control)
                    {
                        case ScenarioHandler.Control_type.GESTURE:
                            if (focused.name == "RCE Components Switch GUI")
                            {
                                ScenarioHandler.Instance.FinishScenario();
                            }
                            break;
                        case ScenarioHandler.Control_type.VOICE:
                            if (focused.name == "RCE Database Component Execution")
                            {
                                ScenarioHandler.Instance.FinishScenario();
                            }
                            break;
                    }
                    break;
                case ScenarioHandler.Scenario_type.SECOND:
                    /*switch (ScenarioHandler.control)
                    {
                        case ScenarioHandler.Control_type.GESTURE:
                            break;
                        case ScenarioHandler.Control_type.VOICE:
                            break;
                    }*/
                    break;
                case ScenarioHandler.Scenario_type.THIRD:
                    switch (ScenarioHandler.control)
                    {
                        case ScenarioHandler.Control_type.GESTURE:
                            if (focused.name == "RCE Toolkit - Common Modules")
                            {
                                ScenarioHandler.Instance.FinishScenario();
                            }
                            break;
                        case ScenarioHandler.Control_type.VOICE:
                            if (focused.name == "RCE Input Provider Component Common")
                            {
                                ScenarioHandler.Instance.FinishScenario();
                            }
                            break;
                    }
                    break;
            }
        }
    }
}