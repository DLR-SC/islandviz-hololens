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

        private void Start()
        {
            VisualizationLoader.Instance.VisualizationLoaded += Init;
        }

        private void Init()
        {
            var bundleContainer = UIManager.Instance.BundleContainer;
            var bundles = bundleContainer.GetComponentsInChildren<Island>();

            foreach (var bundle in bundles)
            {
                Bounds bounds = gameObject.GetComponent<Collider>().bounds;
                CapsuleCollider collider = bundle.GetComponent<CapsuleCollider>();

                Vector3 bundlePosition = bundle.transform.position - transform.position;
                float dist = Vector3.Dot(transform.up, bundlePosition);
                Vector3 projectedWorld = bundlePosition - (dist * transform.up);
                Vector3 projectedLocal = bundleContainer.transform.InverseTransformPoint(projectedWorld);
                float lengthWithoutRadius = projectedLocal.magnitude - collider.radius;
                projectedLocal = projectedLocal.normalized * lengthWithoutRadius;
                projectedWorld = bundleContainer.transform.TransformPoint(projectedLocal);

                if (!bounds.Contains(projectedWorld))
                    bundle.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                else
                    bundle.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            other.gameObject.layer = LayerMask.NameToLayer("Default");
        }

        private void OnTriggerExit(Collider other)
        {
            Context context = ContextManager.Instance.SafeContext;
            if (context.Selected.gameObject == other.gameObject)
                return;

            if (!other.enabled)
                return;

            other.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }
}
