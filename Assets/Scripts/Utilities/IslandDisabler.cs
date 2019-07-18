using HoloIslandVis.Controller;
using HoloIslandVis.Core;
using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Utilities
{

    public class IslandDisabler : MonoBehaviour
    {
        public GameObject BundleContainer;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.transform.parent.gameObject == BundleContainer)
                SetLayerRecursively(other.gameObject, LayerMask.NameToLayer("Default"));
        }

        private void OnTriggerExit(Collider other)
        {
            Context context = ContextManager.Instance.SafeContext;
            if (context.Selected.gameObject == other.gameObject) return;
            if (other.gameObject.transform.parent.gameObject == BundleContainer)
                SetLayerRecursively(other.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
        }

        void SetLayerRecursively(GameObject obj, int layer)
        {
            if (null == obj) return;
            obj.layer = layer;

            foreach (Transform childTransform in obj.transform)
            {
                if (childTransform == null) return;
                SetLayerRecursively(childTransform.gameObject, layer);
            }
        }

    }
}
