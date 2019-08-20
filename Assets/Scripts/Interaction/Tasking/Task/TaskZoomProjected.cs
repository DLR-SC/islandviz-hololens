using HoloIslandVis.Interaction;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskZoomProjected : ContinuousInteractionTask
    {
        private Visualization _visualization;

        private Vector3 _visualizationPosition;
        private Vector3 _visualizationScale;

        private Vector3 _sourceOnePos;
        private Vector3 _sourceTwoPos;
        private float _startDistance;

        public override IEnumerator StartInteraction(GestureInteractionEventArgs eventArgs)
        {
            ScenarioHandler.keywordsGesture.Add("Zoom");
            _visualization = UIManager.Instance.Visualization;
            _visualizationPosition = _visualization.transform.localPosition;
            _visualizationScale = _visualization.transform.localScale;
            _startDistance = 0.1f;

            if (eventArgs.IsTwoHanded)
            {
                _sourceOnePos = eventArgs.HandOnePos;
                _sourceTwoPos = eventArgs.HandTwoPos;
                _startDistance = Vector3.Distance(_sourceOnePos, _sourceTwoPos);
            }

            yield return null;
        }

        public override IEnumerator UpdateInteraction(GestureInteractionEventArgs eventArgs)
        {
            if (eventArgs.IsTwoHanded)
            {
                _sourceOnePos = eventArgs.HandOnePos;
                _sourceTwoPos = eventArgs.HandTwoPos;

                float scalingFactor = Vector3.Distance(_sourceOnePos, _sourceTwoPos) / _startDistance;
                SetScaleRelativeToCenter(scalingFactor);
            }

            yield return null;
        }

        public override IEnumerator EndInteraction(GestureInteractionEventArgs eventArgs)
        {
            yield return null;
        }

        private void SetScaleRelativeToCenter(float scale)
        {
            _visualization.transform.localScale = _visualizationScale * scale;
            Vector3 positionDiff = _visualizationPosition * scale - _visualizationPosition;
            _visualization.transform.localPosition = _visualizationPosition + positionDiff;
        }
    }
}