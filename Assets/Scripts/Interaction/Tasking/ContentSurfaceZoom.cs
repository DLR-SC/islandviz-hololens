using HoloIslandVis.Component.UI;
using HoloIslandVis.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Utility;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking
{
    public class ContentSurfaceZoom : ContinuousInteractionTask
    {
        private bool _hasStarted;
        private List<IInputSource> _inputSources;
        private List<uint> _sourceId;
        private Vector3 _startPosition;
        private Vector3 _startScale;
        private float _startDistance;

        public ContentSurfaceZoom()
        {
            _inputSources = new List<IInputSource>();
            _sourceId = new List<uint>();

            _startDistance = 0;
        }

        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            _startPosition = RuntimeCache.Instance.VisualizationContainer.transform.position;
            _startScale = RuntimeCache.Instance.VisualizationContainer.transform.localScale;
            _hasStarted = true;

            //for(int i = 0; i < eventArgs.InputSources.Length; i++)
            //{
            //    _inputSources.Add(eventArgs.InputSources[i]);
            //    _sourceId.Add(eventArgs.SourceIds[i]);
            //}

            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ZoomStart: " + _inputSources.Count;
            Vector3 handOnePos;
            Vector3 handTwoPos;

            if (_inputSources[0].TryGetGripPosition(_sourceId[0], out handOnePos) &&
                    _inputSources[1].TryGetGripPosition(_sourceId[1], out handTwoPos))
            {
                _startDistance = Vector3.Distance(handOnePos, handTwoPos);
            }

            

        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            Vector3 handOnePos;
            Vector3 handTwoPos;

            if (_hasStarted && _inputSources[0].TryGetGripPosition(_sourceId[0], out handOnePos) &&
                        _inputSources[1].TryGetGripPosition(_sourceId[1], out handTwoPos))
            {
                float scalingFactor = Vector3.Distance(handOnePos, handTwoPos)/_startDistance;
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Scaling factor: " + scalingFactor;

                Vector3 toVisContainer = _startPosition -
                    RuntimeCache.Instance.ContentSurface.transform.position;

                Vector3 toVisContainerDiff = (toVisContainer * scalingFactor) - toVisContainer;

                RuntimeCache.Instance.VisualizationContainer.transform.localScale = _startScale * scalingFactor;
                RuntimeCache.Instance.VisualizationContainer.transform.position = _startPosition + toVisContainerDiff;
            }
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ZoomEnd";
            _inputSources.Clear();
            _sourceId.Clear();
            _hasStarted = false;
        }

        public void scaleAround(GameObject target, Vector3 pivot, Vector3 newScale)
        {
            Vector3 A = target.transform.localPosition;
            Vector3 B = pivot;

            Vector3 C = A - B; // diff from object pivot to desired pivot/origin

            float RS = newScale.x / target.transform.localScale.x; // relative scale factor

            // calc final position post-scale
            Vector3 FP = B + C * RS;

            // finally, actually perform the scale/translation
            target.transform.localScale = newScale;
            target.transform.localPosition = FP;
        }
    }
}
