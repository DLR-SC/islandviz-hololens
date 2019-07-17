using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    public class SyncStorageFloat : SyncObject
    {
        [SyncData] private SyncFloat item = null;

#if UNITY_EDITOR
        public override object RawValue
        {
            get { return Value; }
        }
#endif

        public float Value
        {
            get { return item.Value; }
            set { item.Value = value; }
        }
    }
}