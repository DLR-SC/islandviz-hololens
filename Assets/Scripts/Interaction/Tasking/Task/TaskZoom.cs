using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskZoom : DiscreteSpeechInteractionTask
    {
        private UIComponent _visualization;
        UIComponent _contentPane;
        
        private Vector3 focusPosition;
        private Vector3 projectedFocus;
        private Vector3 distanceVector;

        private float _zoomFactor;
        private float _duration;

        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            // Number of seconds to zoom.
            _duration = 4.0f;
            // Zoom intensity.
            _zoomFactor = 2.0f;

            // Get position information
            _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);
            _contentPane = UIManager.Instance.GetUIElement(UIElement.ContentPane);
            focusPosition = GazeManager.Instance.HitPosition;

            // Relocate the focus vector to a position on the plane which is orthogonal 
            // below the position of the focus vector.
            Vector3 paneToFocus = _contentPane.transform.position - focusPosition;
            float heightFactor = Vector3.Dot(_contentPane.transform.up, paneToFocus);
            Vector3 height = _contentPane.transform.up * heightFactor;
            Vector3 projectedPaneToFocus = paneToFocus - height;

            // Calculate the distance between the center of the pane and the position on 
            // the plane calculated above.            
            projectedFocus = _contentPane.transform.position - projectedPaneToFocus;
            distanceVector = projectedFocus - _visualization.transform.position;

            // Based on the KeywordType, calculate the new scale of the visualization.
            // When changing the scale, the visualization must be shifted, so that the
            // focus position remains the same. The shiftVector contains the positions
            // with which the visualization must be shifted.
            Vector3 shiftVector;
            Vector3 scaledVisualizationScale;
            switch (eventArgs.Keyword)
            {
                case KeywordType.ZoomIn:
                    shiftVector = distanceVector - _zoomFactor * distanceVector;
                    scaledVisualizationScale = _zoomFactor * _visualization.transform.localScale;
                    break;
                case KeywordType.ZoomOut:
                    shiftVector = distanceVector - (1 / _zoomFactor) * distanceVector;
                    scaledVisualizationScale = (1 / _zoomFactor) * _visualization.transform.localScale;
                    break;
                default:
                    shiftVector = distanceVector;
                    scaledVisualizationScale = _visualization.transform.localScale;
                    break;
            }

            Vector3 startPosition = _visualization.transform.position;
            Vector3 targetPosition = startPosition + shiftVector;
            Vector3 startScale = _visualization.transform.localScale;
            Vector3 targetScale = scaledVisualizationScale;

            yield return Zoom(startPosition, targetPosition, startScale, targetScale);
        }

        private IEnumerator Zoom(Vector3 startPosition, Vector3 targetPosition, Vector3 startScale, Vector3 targetScale)
        {
            Vector3 currentPosition = startPosition;
            Vector3 currentScale = startScale;

            /* 
             * (cumulative_time - duration / 2) - 12 / duration 
             * 
             * This function takes the time that has passed since the beginning of the move process
             * and maps it to input range of -6 to 6. This input range is chosen, as the sigmoid function
             * converges to -1 or 1 outside of this area.
             */

            float interpolation = 0.0f;
            // Time passed since the beginning of the move process
            float cumulativeTime = 0.0f;
            float durationOffset = _duration / 2.0f;
            float durationFactor = 12.0f / _duration;

            while (Vector3.Distance(targetPosition, currentPosition) > 0.001f)
            {
                cumulativeTime += Time.deltaTime;
                float sigmoidInput = (cumulativeTime - durationOffset) * durationFactor;
                interpolation = Sigmoid(sigmoidInput);
                currentPosition = Vector3.Lerp(startPosition, targetPosition, interpolation);
                currentScale = Vector3.Lerp(startScale, targetScale, interpolation);
                _visualization.transform.position = currentPosition;
                _visualization.transform.localScale = currentScale;
                yield return null;
            }

            _visualization.transform.position = currentPosition;
            _visualization.transform.localScale = targetScale;
        }

        public float Sigmoid(float x)
        {
            // Results of the sigmoid function are in the interval of (0, 1)
            return (1 / (1 + Mathf.Exp(-x)));
        }
    }
}
