using HoloIslandVis;
using HoloIslandVis.Utility;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public class ContentSurfaceDrag : ContinuousInteractionTask
    {
        private bool _hasStarted;
        private IInputSource _source;
        private uint _sourceId;
        private GameObject _visualizationContainer;
        private Vector3 _lastPosition;
        private Vector3 _currentPosition;
        private Vector3 _surfacePosition;
        private Vector3 _surfaceNormal;

        public ContentSurfaceDrag()
        {
            _hasStarted = false;
            _lastPosition = Vector3.zero;
            _currentPosition = Vector3.zero;
            _visualizationContainer = RuntimeCache.Instance.VisualizationContainer;
            _surfacePosition = RuntimeCache.Instance.ContentSurface.transform.position;
            _surfaceNormal = RuntimeCache.Instance.ContentSurface.transform.up;
        }

        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            _hasStarted = true;

            _lastPosition = Vector3.zero;
            _currentPosition = Vector3.zero;

            //for (int i = 0; i < eventArgs.InputSources.Length; i++)
            //{
            //    _source = eventArgs.InputSources[i];
            //    _sourceId = eventArgs.SourceIds[i];

            //    _source.TryGetGripPosition(_sourceId, out _lastPosition);
            //}
        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            if(_hasStarted && _source.TryGetGripPosition(_sourceId, out _currentPosition))
            {
                // Calcualte projection of difference vector onto content surface.
                Vector3 positionDifference = _currentPosition - _lastPosition;
                float projLength = Vector3.Dot(_surfaceNormal, positionDifference);
                Vector3 projPoint = _surfacePosition + positionDifference - projLength * _surfaceNormal;
                Vector3 projVector = projPoint - _surfacePosition;
                _visualizationContainer.transform.position += projVector;
                _lastPosition = _currentPosition;
            }
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            _hasStarted = false;
        }
    }
}
