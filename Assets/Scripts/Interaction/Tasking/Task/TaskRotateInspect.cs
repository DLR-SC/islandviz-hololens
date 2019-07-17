using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskRotateInspect : ContinuousInteractionTask
    {
        private Visualization _visualization;
        private ContentPane _contentPane;
        private Vector3 _projectionAxis;
        private Vector3 _rotationAxis;
        private Vector3 _pivot;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;

        public override IEnumerator StartInteraction(GestureInteractionEventArgs eventArgs)
        {
            _visualization = UIManager.Instance.Visualization;
            _contentPane = UIManager.Instance.ContentPane;

            _projectionAxis = GameObject.Find("MixedRealityCamera").transform.right;
            _rotationAxis = _visualization.transform.up;
            _pivot = _contentPane.transform.position;


            _lastPosition = eventArgs.HandOnePos;
            _currentPosition = eventArgs.HandOnePos;

            yield break;
        }

        public override IEnumerator UpdateInteraction(GestureInteractionEventArgs eventArgs)
        {
            _currentPosition = eventArgs.HandOnePos;
            Vector3 positionDifference = (_currentPosition - _lastPosition) * 150;
            float angle = Vector3.Dot(-positionDifference, _projectionAxis);
            _visualization.transform.RotateAround(_pivot, _rotationAxis, angle);
            _lastPosition = _currentPosition;
            yield break;
        }

        public override IEnumerator EndInteraction(GestureInteractionEventArgs eventArgs)
        {
            yield break;
        }
    }
}
