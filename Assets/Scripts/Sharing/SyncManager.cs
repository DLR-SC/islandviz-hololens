using HoloIslandVis;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing.SyncModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncManager : SingletonComponent<SyncManager>
{
    private SharingStage _sharingStage;
    private SyncArray<SyncSpawnedObject> _syncSource;

    private void Start()
    {
        _sharingStage = SharingStage.Instance;
        if (_sharingStage.IsConnected)
        {
            connected();
            return;
        }

        _sharingStage.SharingManagerConnected += connected;
    }

    private void connected(object sender = null, EventArgs eventArgs = null)
    {
        _sharingStage.SharingManagerConnected -= connected;
        if (_syncSource != null)
            removeAll();

        setDataModelSource();
        registerToDataModel();
    }

    private void removeAll()
    {

    }

    private void setDataModelSource()
        => _syncSource = _sharingStage.Root.InstantiatedPrefabs;
    
    private void registerToDataModel()
    {
        _syncSource.ObjectAdded += OnObjectAdded;
        _syncSource.ObjectRemoved += OnObjectRemoved;
        _syncSource.ObjectChanged += OnObjectChanged;
    }

    private void OnObjectAdded(SyncSpawnedObject syncObj)
    {

    }

    private void OnObjectRemoved(SyncSpawnedObject syncObj)
    {

    }

    private void OnObjectChanged(SyncObject syncObj)
    {

    }
}
