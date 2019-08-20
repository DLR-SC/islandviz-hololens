using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Info;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskBuildingSelect : DiscreteGestureInteractionTask
    {
        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            ScenarioHandler.current_compilationUnit = eventArgs.Focused.name;
            ScenarioHandler.keywordsGesture.Add("Select[Building]");
            UpdateHighlights(eventArgs);
            yield return null;
        }
        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            ScenarioHandler.current_compilationUnit = eventArgs.Focused.name;
            ScenarioHandler.keywordsSpeech.Add("Select[Building]");
            ScenarioHandler.IncrementCounterVoiceControl();
            UpdateHighlights(eventArgs);
            yield return null;
        }

        private void UpdateHighlights(GestureInteractionEventArgs eventArgs)
        {
            if (eventArgs.Selected.Type == InteractableType.CompilationUnit)
            {
                UIManager.Instance.DisableToolbar(true);
                UIManager.Instance.SetActivePanelBody(PanelItem.BodyClass);
                UIManager.Instance.DeactivatePanelBody(PanelItem.BodyClass);

                eventArgs.Selected.Highlight.gameObject.SetActive(false);

                eventArgs.Selected.OnDeselect();
                eventArgs.Focused.OnSelect();
                UIManager.Instance.EnableToolbar(true);
            }

            Building building = eventArgs.Focused.GetComponent<Building>();
            UIManager.Instance.SetPanelHeaderTop("Compilation Unit");
            UIManager.Instance.SetPanelHeaderBottom(building.name);
            UIManager.Instance.SetActivePanelBody(PanelItem.BodyClass);
            //UIManager.Instance.ActivatePanelBody(PanelItem.BodyClass);
            eventArgs.Focused.Highlight.gameObject.SetActive(true);
            eventArgs.Focused.OnSelect();
        }

        private void UpdateHighlights(SpeechInteractionEventArgs eventArgs)
        {
            if (eventArgs.Selected.Type == InteractableType.CompilationUnit)
            {
                UIManager.Instance.DisableToolbar(true);
                UIManager.Instance.SetActivePanelBody(PanelItem.BodyClass);
                UIManager.Instance.DeactivatePanelBody(PanelItem.BodyClass);

                eventArgs.Selected.Highlight.gameObject.SetActive(false);

                eventArgs.Selected.OnDeselect();
                eventArgs.Focused.OnSelect();
                UIManager.Instance.EnableToolbar(true);
            }

            Building building = eventArgs.Focused.GetComponent<Building>();
            UIManager.Instance.SetPanelHeaderTop("Compilation Unit");
            UIManager.Instance.SetPanelHeaderBottom(building.name);
            UIManager.Instance.SetActivePanelBody(PanelItem.BodyClass);
            //UIManager.Instance.ActivatePanelBody(PanelItem.BodyClass);
            eventArgs.Focused.Highlight.gameObject.SetActive(true);
            eventArgs.Focused.OnSelect();
        }
    }
}
