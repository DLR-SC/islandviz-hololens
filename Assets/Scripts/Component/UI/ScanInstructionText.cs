using HoloIslandVis.Mapping;
using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanInstructionText : MonoBehaviour
{
    private int _platformCellCount;

    public TextMesh TextMesh;

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

    private void OnPlatformCellCountChanged(int platformCellCount)
    {
        _platformCellCount = platformCellCount;
        if(_platformCellCount < SpatialScan.Instance.TargetPlatformCellCount)
            TextMesh.text = "Walk around and scan room.";
        else if(_platformCellCount >= SpatialScan.Instance.TargetPlatformCellCount)
            TextMesh.text = "Tap to finalize scan.";
    }

    private void OnScanStateChanged(SpatialUnderstanding.ScanStates scanState)
    {
        //throw new NotImplementedException();
    }
}
