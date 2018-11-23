using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SyncDataClass]
public class SyncObjectModel : SyncObject
{
    private string _name;
    private bool _syncTransform;

    [SyncData] public SyncBool SyncEnabled;
    [SyncData] public SyncVector3 SyncPosition;
    [SyncData] public SyncVector3 SyncScale;
    [SyncData] public SyncQuaternion SyncRotation;

    public SyncObjectModel(string name, bool syncTransform)
        : base(name)
    {
        SyncEnabled = new SyncBool(name + "_syncEnabled");
        SyncPosition = new SyncVector3(name + "_syncPosition");
        SyncScale = new SyncVector3(name + "_syncScale");
        SyncRotation = new SyncQuaternion(name + "_syncRotation");
    }
}
