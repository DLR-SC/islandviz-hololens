using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskMoveDirection : DiscreteSpeechInteractionTask
    {
        private UIComponent _contentPane;
        private UIComponent _visualization;
        private float _scalingFactor;
        private float _duration;

        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);
            _contentPane = UIManager.Instance.GetUIElement(UIElement.ContentPane);

            // Number of seconds to perform a shift.
            _duration = 4.0f;

            // Set the width of the shift.
            _scalingFactor = 30.0f * 0.01f / _visualization.transform.localScale.x;


            // Get position information
            Vector3 headPosition = GazeManager.Instance.GazeOrigin;
            Vector3 contentPosition = _contentPane.transform.position;

            // Project the vector from head to content into the plane. 
            Vector3 paneDirection = -(headPosition - contentPosition);
            float heightFactor = Vector3.Dot(_contentPane.transform.up, paneDirection);
            Vector3 height = _contentPane.transform.up * heightFactor;
            Vector3 projectedDirection = Vector3.Normalize(paneDirection - height);

            // The width of the shift is adapted to the zoom level of the application.
            // scaledMovement stores an according factor.
            Vector3 scaledMovement;
            // Start and target position of the shift.
            Vector3 startPosition = _visualization.transform.position;
            Vector3 targetPosition;

            // Used when moving to the left or to the right.
            Vector3 directionShift;

            // Calculate the target position depending on the direction.
            switch (eventArgs.Keyword)
            {
                case KeywordType.MoveRight:
                    directionShift = Vector3.Cross(projectedDirection, _contentPane.transform.up).normalized;
                    break;
                case KeywordType.MoveLeft:
                    directionShift = Vector3.Cross(_contentPane.transform.up, projectedDirection).normalized;
                    break;
                case KeywordType.MoveDown:
                    directionShift = projectedDirection;
                    break;
                case KeywordType.MoveUp:
                    directionShift = -projectedDirection;
                    break;
                default:
                    directionShift = Vector3.zero;
                    break;
            }
            scaledMovement = directionShift * _visualization.transform.localScale.x;
            targetPosition = startPosition + scaledMovement * _scalingFactor;
            yield return ShiftPane(startPosition, targetPosition);
        }

        private IEnumerator ShiftPane(Vector3 startPosition, Vector3 targetPosition)
        {
            Vector3 currentPosition = startPosition;

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
                _visualization.transform.position = currentPosition;
                yield return null;
            }

            _visualization.transform.position = targetPosition;
        }

        public float Sigmoid(float x)
        {
            // Results of the sigmoid function are in the interval of (0, 1)
            return (1 / (1 + Mathf.Exp(-x)));
        }
    }
}
