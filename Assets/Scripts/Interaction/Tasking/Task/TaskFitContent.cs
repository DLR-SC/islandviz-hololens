using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskFitContent : DiscreteGestureInteractionTask
    {
        private ContentPane _contentPane;
        private Visualization _visualization;
        private BundleContainer _bundleContainer;

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
            _bundleContainer = UIManager.Instance.BundleContainer;

            _visualization.transform.position = Vector3.zero;
            _visualization.transform.localScale = GetContentFitScale();
            
            yield break;
        }

        private Vector3 GetContentFitScale()
        {
            Vector3 containerPosition = _bundleContainer.transform.position;
            Bounds contentBounds = new Bounds(containerPosition, Vector3.zero);

            GameObject water = GameObject.Find("Water");
            Bounds waterBounds = water.GetComponent<Renderer>().bounds;

            var renderers = _bundleContainer.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
                contentBounds.Encapsulate(renderer.bounds);

            Debug.Log("CONTENT BOUNDS:   " + contentBounds.extents.x);
            Debug.Log("PANE BOUNDS:      " + waterBounds.extents.x);

            float maxExtents = Mathf.Max(contentBounds.extents.x, contentBounds.extents.z);

            float scaleFactor = waterBounds.extents.x / maxExtents;
            Vector3 result = _visualization.transform.localScale * scaleFactor * 0.9f;

            return result;
        }
    }
}
