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
        private UIComponent _contentPane;
        private UIComponent _visualization;
        private float _scalingFactor;
        private float _duration;

        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            // Number of seconds to zoom.
            _duration = 4.0f;
            // Influences the zoom intensity.
            _scalingFactor = 20.0f;

            _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);
            _contentPane = UIManager.Instance.GetUIElement(UIElement.ContentPane);

            // Get position information
            Vector3 headPosition = GazeManager.Instance.GazeOrigin;
            Vector3 contentPosition = _contentPane.transform.position;

            // Start and target position of the shift.
            Vector3 startPosition = headPosition;
            Vector3 targetPosition;

            /*switch (eventArgs.Keyword)
            {
                case KeywordType.ZoomIn:
                    targetPosition = headPosition - (headPosition - _visualization.transform.position) / 2;
                    break;
                case KeywordType.ZoomOut:
                    targetPosition = headPosition + (headPosition - _visualization.transform.position) / 2;
                    break;
                default:
                    targetPosition = startPosition;
                    break;
            }*/

            Vector3 startScale = _visualization.transform.localScale;
            Vector3 targetScale = 1.1f * _visualization.transform.localScale;
            yield return Zoom(startScale, targetScale);
        }

        private IEnumerator Zoom(Vector3 startScale, Vector3 targetScale)
        {
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

            while (Vector3.Distance(targetScale, currentScale) > 0.001f)
            {
                cumulativeTime += Time.deltaTime;
                float sigmoidInput = (cumulativeTime - durationOffset) * durationFactor;
                interpolation = Sigmoid(sigmoidInput);
                currentScale = Vector3.Lerp(startScale, targetScale, interpolation);
                _visualization.transform.localScale = currentScale;
                yield return null;
            }

            _visualization.transform.localScale = targetScale;
        }

        public float Sigmoid(float x)
        {
            // Results of the sigmoid function are in the interval of (0, 1)
            return (1 / (1 + Mathf.Exp(-x)));
        }
    }
}
