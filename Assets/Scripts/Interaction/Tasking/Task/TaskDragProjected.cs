using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskDragProjected : ContinuousInteractionTask
    {
        Visualization _visualization;

        private Vector3 _position;
        private Vector3 _normal;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;

        public override IEnumerator StartInteraction(GestureInteractionEventArgs eventArgs)
        {
            ScenarioHandler.keywordsGesture.Add("Move");
            ScenarioHandler.IncrementCounterGestureControl();
            _visualization = UIManager.Instance.Visualization;

            _position = _visualization.transform.position;
            _normal = _visualization.transform.up;

            _lastPosition = eventArgs.HandOnePos;
            _currentPosition = eventArgs.HandOnePos;

            yield break;
        }

        public override IEnumerator UpdateInteraction(GestureInteractionEventArgs eventArgs)
        {
            _currentPosition = eventArgs.HandOnePos;
            Vector3 positionDifference = _currentPosition - _lastPosition;
            float projLength = Vector3.Dot(_normal, positionDifference);
            Vector3 projPoint = _position + positionDifference - projLength * _normal;
            Vector3 projVector = projPoint - _position;
            _visualization.transform.position += projVector;
            _lastPosition = _currentPosition;
            yield break;
        }

        public override IEnumerator EndInteraction(GestureInteractionEventArgs eventArgs)
        {
            yield break;
        }
    }
}
