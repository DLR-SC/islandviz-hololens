using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    [SyncDataClass]
    public class SyncAppStateModel : SyncObject
    {
        public delegate void SyncBoolChangedHandler(SyncString syncString);
        public event SyncBoolChangedHandler SyncStringChanged = delegate { };

        private string _name;

        [SyncData] public SyncString SyncCommand;
        [SyncData] public SyncString SyncFocused;
        [SyncData] public SyncString SyncSelected;

        public SyncAppStateModel(string name) : base(name)
        {

        }

        protected override void NotifyPrimitiveChanged(SyncPrimitive primitive)
        {
            if (primitive is SyncString)
            {
                SyncStringChanged((SyncString)primitive);
            }
        }
    }
}
