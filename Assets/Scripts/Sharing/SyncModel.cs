using HoloIslandVis.Core;
using HoloToolkit.Sharing.SyncModel;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    [SyncDataClass]
    public class SyncModel : SyncObject
    {
        public delegate void SyncBoolChangedHandler(SyncBool syncBool);
        public delegate void SyncFloatChangedHandler(SyncFloat syncBool);
        public event SyncBoolChangedHandler SyncBoolChanged = delegate { };
        public event SyncFloatChangedHandler SyncFloatChanged = delegate { };

        private string _name;
        private Dictionary<long, SyncPrimitive> _primitiveMap;

        [SyncData] public SyncBool SyncEnabled;
        [SyncData] public SyncVec3 SyncPosition;
        [SyncData] public SyncVec3 SyncScale;
        [SyncData] public SyncQuat SyncRotation;

        public SyncModel(string name) : base(name)
        {
            _primitiveMap = new Dictionary<long, SyncPrimitive>();
        }

        protected override void OnBoolElementChanged(long elementID, bool newValue)
        {
            if (GameObject.Find("AppConfig").GetComponent<AppConfig>().IsServerInstance)
            {
                base.OnBoolElementChanged(elementID, newValue);
            }
            else
            {
                if (_primitiveMap.ContainsKey(elementID))
                {
                    SyncPrimitive primitive = _primitiveMap[elementID];
                    primitive.UpdateFromRemote(newValue);
                    NotifyPrimitiveChanged(primitive);
                }
                else
                {
                    Debug.LogWarningFormat("Unknown primitive, discarding update.  Value: {1}, Id: {2}", elementID, newValue);
                }
            }
        }

        protected override void OnFloatElementChanged(long elementID, float newValue)
        {
            if (GameObject.Find("AppConfig").GetComponent<AppConfig>().IsServerInstance)
            {
                base.OnFloatElementChanged(elementID, newValue);
            }
            else
            {
                if (_primitiveMap.ContainsKey(elementID))
                {
                    SyncPrimitive primitive = _primitiveMap[elementID];
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
            if (primitive is SyncBool)
            {
                SyncBoolChanged((SyncBool)primitive);
            }
            else if (primitive is SyncFloat)
            {
                SyncFloatChanged((SyncFloat)primitive);
            }
        }

        public void AddRemotePrimitive(long guid, SyncPrimitive syncPrimitve)
        {
            _primitiveMap.Add(guid, syncPrimitve);
        }
    }
}
