using HoloIslandVis.Component;
using HoloIslandVis.Utility;
using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis.Mapping
{
    public class SpatialScan : Singleton<SpatialScan>
    {
        private bool _beginScanRequested;
        private bool _finishScanRequested;

        #region OBSERVABLE PROPERTIES
        private Observable<int> _platformCellCountObservable;
        private Observable<SpatialUnderstanding.ScanStates> _scanStateObservable;

        public Observable<int> PlatformCellCountObservable {
            get {
                if (_platformCellCountObservable == null)
                    _platformCellCountObservable = new Observable<int>(0);

                return _platformCellCountObservable;
            }

            private set { }
        }

        public Observable<SpatialUnderstanding.ScanStates> ScanStateObservable {
            get {
                if (_scanStateObservable == null)
                    _scanStateObservable = new Observable<SpatialUnderstanding.ScanStates>
                        (SpatialUnderstanding.ScanStates.None);

                return _scanStateObservable;
            }

            private set { }
        }
        #endregion

        #region PROPERTIES
        public int PlatformCellCount {
            get { return PlatformCellCountObservable.Value; }
            set { PlatformCellCountObservable.Value = value; }
        }

        // TODO: No magic number?
        public int TargetPlatformCellCount { get; } = 5;

        public SpatialUnderstanding.ScanStates ScanState {
            get {
                SpatialUnderstanding.ScanStates scanState = SpatialUnderstanding.Instance.ScanState;
                if(ScanStateObservable.Value != scanState)
                    ScanStateObservable.Value = scanState;

                return ScanStateObservable.Value;
            }
        }

        public bool IsScanning {
            get {
                SpatialUnderstanding.ScanStates scanState = ScanState;
                if(scanState == SpatialUnderstanding.ScanStates.Scanning)
                    return true;

                return false;
            }
        }

        public bool AllowSpatialUnderstanding {
            get {
                if (SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
                    return true;

                return false;
            }
        }
        #endregion

        private SpatialScan()
        {
            SpatialUnderstanding.Instance.ScanStateChanged += onScanStateChanged;
            _beginScanRequested = false;
            _finishScanRequested = false;
        }

        public void RequestBeginScanning()
        {
            if(!_beginScanRequested && !IsScanning)
            {
                SpatialUnderstanding.Instance.RequestBeginScanning();
                _beginScanRequested = true;
            }
        }

        public void RequestFinishScanning()
        {
            if(IsScanning)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
                _finishScanRequested = true;
            }
        }

        private async void updateScanStats()
        {
            while (IsScanning && !_finishScanRequested)
            {
                // TODO: No magic number here?
                await Task.Delay(250);
                UnityMainThreadDispatcher.Instance.Enqueue(() => {
                    SpatialUnderstandingDll.Imports.PlayspaceStats stats;
                    if(!tryGetPlayspaceStats(out stats))
                        return;

                    PlatformCellCount = stats.NumPlatform;
                });
            }

            _finishScanRequested = false;
        }

        private bool tryGetPlayspaceStats(out SpatialUnderstandingDll.Imports.PlayspaceStats stats)
        {
            stats = null;

            if(!IsScanning || !AllowSpatialUnderstanding)
                return false;

            IntPtr statsPointer = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStatsPtr();
            if(SpatialUnderstandingDll.Imports.QueryPlayspaceStats(statsPointer) == 0)
                return false;

            stats = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceStats();
            return true;
        }

        private void onScanStateChanged()
        {
            if(_beginScanRequested && IsScanning)
            {
                _beginScanRequested = false;
                new Task(() => updateScanStats()).Start();
            }
        }
    }
}
