using HoloIslandVis.Interaction;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Sharing;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class SurfaceDragTask : ContinuousInteractionTask
    {
        private GameObject _visualizationContainer;
        private Vector3 _surfacePosition;
        private Vector3 _surfaceNormal;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;

        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            _visualizationContainer = RuntimeCache.Instance.VisualizationContainer;
            _surfacePosition = RuntimeCache.Instance.ContentSurface.transform.position;
            _surfaceNormal = RuntimeCache.Instance.ContentSurface.transform.up;

            _lastPosition = Vector3.zero;
            _currentPosition = Vector3.zero;

            eventArgs.TryGetSingleGripPosition(out _lastPosition);
        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            if(eventArgs.TryGetSingleGripPosition(out _currentPosition))
            {
                // Calcualte projection of difference vector onto content surface.
                Vector3 positionDifference = _currentPosition - _lastPosition;
                float projLength = Vector3.Dot(_surfaceNormal, positionDifference);
                Vector3 projPoint = _surfacePosition + positionDifference - projLength * _surfaceNormal;
                Vector3 projVector = projPoint - _surfacePosition;
                _visualizationContainer.transform.position += projVector;
                _lastPosition = _currentPosition;


                CustomMessages.Instance.SendContainerTransform(_visualizationContainer.transform.position,
                    _visualizationContainer.transform.rotation);
            }
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            // Nothing to do!
        }
    }
}
