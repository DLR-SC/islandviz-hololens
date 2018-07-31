using HoloIslandVis.Mapping;
using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis.Component.UI
{
    public class ScanProgressBar : MonoBehaviour
    {
        public Image ProgressBar;

        private void OnEnable()
        {
            SpatialScan.Instance.PlatformCellCountObservable.ValueChanged += OnPlatformCellCountChanged;
            SpatialScan.Instance.ScanStateObservable.ValueChanged += OnScanStateChanged;
        }

        private void OnDisable()
        {
            SpatialScan.Instance.PlatformCellCountObservable.ValueChanged -= OnPlatformCellCountChanged;
            SpatialScan.Instance.ScanStateObservable.ValueChanged -= OnScanStateChanged;
        }

        private void OnScanStateChanged(SpatialUnderstanding.ScanStates scanState)
        {
            Debug.Log(gameObject.name + ": Scan state changed to " + scanState);
        }

        private void OnPlatformCellCountChanged(int platformCellCount)
        {
            float progress = (float) platformCellCount / SpatialScan.Instance.TargetPlatformCellCount;
            ProgressBar.fillAmount = progress;
        }
    }
}
