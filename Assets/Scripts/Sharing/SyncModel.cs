using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SyncDataClass]
public class SyncModel : SyncObject
{
    public delegate void SyncBoolChangedHandler(SyncBool syncBool);
    public event SyncBoolChangedHandler SyncBoolChanged = delegate { };

    private string _name;
    private bool _syncTransform;

    [SyncData] public SyncBool SyncEnabled;
    [SyncData] public SyncVector3 SyncPosition;
    [SyncData] public SyncVector3 SyncScale;
    [SyncData] public SyncQuaternion SyncRotation;

    public SyncModel(string name, bool syncTransform)
        : base(name)
    {

    }

    protected override void NotifyPrimitiveChanged(SyncPrimitive primitive)
    {
        if(primitive is SyncBool)
        {
            SyncBoolChanged((SyncBool)primitive);
        }
    }
}
