using HoloToolkit.Unity.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wrapper for HoloToolkit Bounding Box to facilitate
// initialization and access via the UIManager.
namespace HoloIslandVis.UI.Component
{
    public class BoundingBoxWrapper : UIComponent
    {
        public BoundingBoxRig Rig;
        public bool IsInitialized;

        private BoundingBox _boundingBox;
        private AppBar _appBar;

        public void Start()
        {
            IsInitialized = false;
            StartCoroutine(Initialize(Rig));
        }

        public override IEnumerator Activate()
        {
            // Postpone activation until bounding box
            // is initialized.
            while (!IsInitialized)
                yield return null;

            gameObject.SetActive(true);
            Rig.gameObject.SetActive(true);
            Rig.Activate();
        }

        public override IEnumerator Deactivate()
        {
            // Postpone deactivation until bounding box
            // is initialized.
            if (!IsInitialized)
                yield return null;

            gameObject.SetActive(false);
            Rig.gameObject.SetActive(false);
            Rig.Deactivate();
        }

        public IEnumerator Initialize(BoundingBoxRig rig)
        {
            while (FindObjectsOfType<BoundingBox>().Length == 0)
                yield return null;

            while (FindObjectsOfType<AppBar>().Length == 0)
                yield return null;

            Initialized();
        }

        private void Initialized()
        {
            _boundingBox = FindObjectsOfType<BoundingBox>()[0].GetComponent<BoundingBox>();
            _boundingBox.BoundsCalculationMethod = BoundingBox.BoundsCalculationMethodEnum.Colliders;
            _boundingBox.gameObject.layer = LayerMask.NameToLayer("BoundingBox");

            _appBar = FindObjectsOfType<AppBar>()[0].GetComponent<AppBar>();
            _appBar.gameObject.SetActive(false);

            IsInitialized = true;
        }
    }
}
