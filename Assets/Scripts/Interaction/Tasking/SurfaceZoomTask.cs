using HoloIslandVis.Input;
using HoloIslandVis.Utility;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class SurfaceZoomTask : ContinuousInteractionTask
    {
        private Vector3 _startPosition;
        private Vector3 _startScale;

        private Vector3 _sourceOnePos;
        private Vector3 _sourceTwoPos;
        private float _startDistance;

        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            _startPosition = RuntimeCache.Instance.VisualizationContainer.transform.position;
            _startScale = RuntimeCache.Instance.VisualizationContainer.transform.localScale;
            _startDistance = 0.1f;

            if(eventArgs.TryGetDoubleGripPosition(out _sourceOnePos, out _sourceTwoPos))
                _startDistance = Vector3.Distance(_sourceOnePos, _sourceTwoPos);
        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            if(eventArgs.TryGetDoubleGripPosition(out _sourceOnePos, out _sourceTwoPos))
            {
                float scalingFactor = Vector3.Distance(_sourceOnePos, _sourceTwoPos) / _startDistance;
                Vector3 toVisContainer = _startPosition - RuntimeCache.Instance.ContentSurface.transform.position;
                Vector3 toVisContainerDiff = (toVisContainer * scalingFactor) - toVisContainer;
                RuntimeCache.Instance.VisualizationContainer.transform.localScale = _startScale * scalingFactor;
                RuntimeCache.Instance.VisualizationContainer.transform.position = _startPosition + toVisContainerDiff;
            }
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            // Nothing to do!
        }
    }
}
