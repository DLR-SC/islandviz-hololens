using HoloIslandVis.Core;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    [SyncDataClass]
    public class SyncAppStateModel : SyncObject
    {
        public delegate void SyncStringChangedHandler(SyncString syncString);
        public event SyncStringChangedHandler SyncStringChanged = delegate { };

        private string _name;
        private Dictionary<long, SyncPrimitive> _primitveMap;

        [SyncData] public SyncString SyncCommand;
        [SyncData] public SyncString SyncFocused;
        [SyncData] public SyncString SyncSelected;

        public SyncAppStateModel(string name) : base(name)
        {
            _primitveMap = new Dictionary<long, SyncPrimitive>();
        }

        protected override void OnStringElementChanged(long elementID, XString newValue)
        {
            if (GameObject.Find("AppConfig").GetComponent<AppConfig>().IsServerInstance)
            {
                base.OnStringElementChanged(elementID, newValue);
            }
            else
            {
                if(_primitveMap.ContainsKey(elementID))
                {
                    Debug.Log("Updating!");
                    SyncPrimitive primitive = _primitveMap[elementID];
                    primitive.UpdateFromRemote(newValue);
                    NotifyPrimitiveChanged(primitive);
                }
                else
                {
                    Debug.LogWarningFormat("Unknown primitive, discarding update.  Value: {1}, Id: {2}", elementID, newValue);
                }
            }
        }

        protected override void NotifyPrimitiveChanged(SyncPrimitive primitive)
        {
            if (primitive is SyncString)
            {
                SyncStringChanged((SyncString)primitive);
            }
        }

        public void AddRemotePrimitive(long guid, SyncPrimitive syncPrimitve)
        {
            _primitveMap.Add(guid, syncPrimitve);
        }
    }
}
