using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskFitContent : DiscreteGestureInteractionTask
    {
        private UIComponent _contentPane;
        private UIComponent _visualization;
        private float _duration;

        public override IEnumerator Perform(GestureInteractionEventArgs eventArgs)
        {
            yield return Perform();
        }

        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            yield return Perform();
        }

        private IEnumerator Perform()
        {
            _contentPane = UIManager.Instance.ContentPane;
            _visualization = UIManager.Instance.Visualization;
            _duration = 1.0f;

            // TODO Calculate target transform.
            var targetPosition = new Vector3(0.04f, 0f, 0.04f);
            var targetRotation = Quaternion.identity;
            var targetScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

            yield return Translate(_visualization.transform.localPosition, targetPosition);
            yield return Rotate(_visualization.transform.localRotation, targetRotation);
            yield return Scale(_visualization.transform.localScale, targetScale);
        }

        private IEnumerator Rotate(Quaternion startRotation, Quaternion targetRotation)
        {
            Quaternion currentRotation = startRotation;

            float interpolation = 0.0f;
            float cumulativeTime = 0.0f;
            float durationOffset = _duration / 2.0f;
            float durationFactor = 12.0f / _duration;

            while (Quaternion.Angle(targetRotation, currentRotation) > 0.001f)
            {
                cumulativeTime += Time.deltaTime;
                float sigmoidInput = (cumulativeTime - durationOffset) * durationFactor;
                interpolation = Sigmoid(sigmoidInput);
                currentRotation = Quaternion.Lerp(startRotation, targetRotation, interpolation);
                _visualization.transform.localRotation = currentRotation;
                yield return null;
            }

            _visualization.transform.localRotation = targetRotation;
        }

        private IEnumerator Translate(Vector3 startPosition, Vector3 targetPosition)
        {
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
                _visualization.transform.localPosition = currentPosition;
                yield return null;
            }

            _visualization.transform.localPosition = targetPosition;
        }

        private IEnumerator Scale(Vector3 startScale, Vector3 targetScale)
        {
            Vector3 currentScale = startScale;

            float interpolation = 0.0f;
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
            return (1 / (1 + Mathf.Exp(-x)));
        }
    }
}
