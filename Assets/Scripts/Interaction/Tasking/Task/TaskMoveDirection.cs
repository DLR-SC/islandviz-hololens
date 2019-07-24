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
            _duration = 4.0f;
            _scalingFactor = 20.0f;
            _visualization = UIManager.Instance.GetUIElement(UIElement.Visualization);
            _contentPane = UIManager.Instance.GetUIElement(UIElement.ContentPane);
            Vector3 headPosition = GazeManager.Instance.GazeOrigin;
            Vector3 contentPosition = _contentPane.transform.position;
            Vector3 paneDirection = -(headPosition - contentPosition);
            float heightFactor = Vector3.Dot(_contentPane.transform.up, paneDirection);
            Vector3 height = _contentPane.transform.up * heightFactor;
            Vector3 projectedDirection = Vector3.Normalize(paneDirection - height);

            switch (eventArgs.Keyword)
            {
                case KeywordType.MoveRight: yield return MoveRight(projectedDirection); break;
                case KeywordType.MoveLeft: yield return MoveLeft(projectedDirection); break;
                case KeywordType.MoveDown: yield return MoveDown(projectedDirection); break;
                case KeywordType.MoveUp: yield return MoveUp(projectedDirection); break;
                default: break;
            }
        }

        private IEnumerator MoveUp(Vector3 lookDirection)
        {
            Vector3 scaledMovement = lookDirection * _visualization.transform.localScale.x;

            Vector3 startPosition = _visualization.transform.position;
            Vector3 targetPosition = startPosition - scaledMovement * _scalingFactor;
            Vector3 currentPosition = startPosition;

            float interpolation = 0.0f;
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

        private IEnumerator MoveDown(Vector3 lookDirection)
        {
            Vector3 scaledMovement = lookDirection * _visualization.transform.localScale.x;
            _visualization.transform.position += scaledMovement * _scalingFactor;
            yield return null;
        }

        private IEnumerator MoveLeft(Vector3 lookDirection)
        {
            yield return null;
        }

        private IEnumerator MoveRight(Vector3 lookDirection)
        {
            yield return null;
        }

        public float Sigmoid(float x)
        {
            return (1 / (1 + Mathf.Exp(-x)));
        }
    }
}
