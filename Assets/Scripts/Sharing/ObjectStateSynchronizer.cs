using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System;
using UnityEngine;

using Random = UnityEngine.Random;
using UnityEngine.UI;
using HoloIslandVis.Core;

namespace HoloIslandVis.Sharing
{
    public class ObjectStateSynchronizer : MonoBehaviour
    {
        public delegate void TransformChangeHandler();
        public event TransformChangeHandler TransformChange = delegate { };

        public bool SyncActive;
        public AppConfig AppConfig;
        public SyncManager SyncManager;

        [SyncData]
        public SyncModel SyncModel;

        private Observable<bool> _enabled;
        private Observable<Vector3> _position;
        private Observable<Vector3> _scale;
        private Observable<Quaternion> _rotation;

        private void Start()
        {
            AppConfig = GameObject.Find("AppConfig").GetComponent<AppConfig>();

            Vector3 init = new Vector3(Random.value, Random.value, Random.value);
            _enabled = new Observable<bool>(false);
            _position = new Observable<Vector3>(init);
            _scale = new Observable<Vector3>(init);
            _rotation = new Observable<Quaternion>(Quaternion.Euler(init));

            if (SharingStage.Instance != null && SharingStage.Instance.IsConnected)
            {
                InitSyncModel();
                return;
            }

            SyncActive = true;
            SyncManager.SyncManagerStarted += OnSyncManagerStarted;
        }

        public void TransformChanged()
        {
            if (transform.hasChanged)
            {
                _position.Value = transform.localPosition;
                _scale.Value = transform.localScale;
                _rotation.Value = transform.localRotation;
                transform.hasChanged = false;
                TransformChange();
            }
        }

        private void OnSyncManagerStarted()
        {
            if (SharingStage.Instance.IsConnected)
            {
                InitSyncModel();
                return;
            }

            SharingStage.Instance.SharingManagerConnected += InitSyncModel;
        }

        private void InitSyncModel(object sender = null, EventArgs eventArgs = null)
        {
            Debug.Log("[SyncSystem] Initializing " + gameObject.name);
            SharingStage.Instance.SharingManagerConnected -= InitSyncModel;
            SyncModel = new SyncModel(gameObject.name);

            if (!AppConfig.IsServerInstance)
            {
                SyncModel.SyncBoolChanged += OnRemoteEnabledChange;
                SyncModel.SyncPosition.ObjectChanged += OnRemotePositionChange;
                SyncModel.SyncScale.ObjectChanged += OnRemoteScaleChange;
                SyncModel.SyncRotation.ObjectChanged += OnRemoteRotationChange;
            }

            if (AppConfig.IsServerInstance)
            {
                _enabled.ValueChanged += OnEnabledChange;
                _position.ValueChanged += OnPositionChange;
                _scale.ValueChanged += OnScaleChange;
                _rotation.ValueChanged += OnRotationChange;
            }

            SyncObject root = SharingStage.Instance.Root;

            if (root.Element != null)
            {
                OnSyncObjectInitialized(root);
                return;
            }

            root.InitializationComplete += OnSyncObjectInitialized;
        }

        private void OnSyncObjectInitialized(SyncObject syncObj)
        {
            Debug.Log("[SyncSystem] Initialized " + gameObject.name);
            syncObj.InitializationComplete -= OnSyncObjectInitialized;

            Element remoteModelElement = syncObj.Element.GetElement(SyncModel.FieldName);

            if (remoteModelElement != null)
            {
                SyncModel.AddFromRemote(remoteModelElement);

                Element remoteSyncEnabledElement = SyncModel.Element.GetElement("SyncEnabled");
                Element remoteSyncPositionElement = SyncModel.Element.GetElement("SyncPosition");
                Element remoteSyncScaleElement = SyncModel.Element.GetElement("SyncScale");
                Element remoteSyncRotationElement = SyncModel.Element.GetElement("SyncRotation");

                SyncPrimitive[] modelPrimitives = SyncModel.GetChildren();
                modelPrimitives[0].AddFromRemote(remoteSyncEnabledElement);
                modelPrimitives[1].AddFromRemote(remoteSyncPositionElement);
                modelPrimitives[2].AddFromRemote(remoteSyncScaleElement);
                modelPrimitives[3].AddFromRemote(remoteSyncRotationElement);

                ObjectElement remotePositionElement = ObjectElement.Cast(modelPrimitives[1].NetworkElement);
                Element remotePositionX = remotePositionElement.GetElement("x");
                Element remotePositionY = remotePositionElement.GetElement("y");
                Element remotePositionZ = remotePositionElement.GetElement("z");

                SyncVector3 syncPosition = (SyncVector3)modelPrimitives[1];
                SyncPrimitive[] positionPrimitves = syncPosition.GetChildren();
                positionPrimitves[0].AddFromRemote(remotePositionX);
                positionPrimitves[1].AddFromRemote(remotePositionY);
                positionPrimitves[2].AddFromRemote(remotePositionZ);

                ObjectElement remoteScaleElement = ObjectElement.Cast(modelPrimitives[2].NetworkElement);
                Element remoteScaleX = remoteScaleElement.GetElement("x");
                Element remoteScaleY = remoteScaleElement.GetElement("y");
                Element remoteScaleZ = remoteScaleElement.GetElement("z");

                SyncPrimitive[] scalePrimitves = ((SyncObject)modelPrimitives[2]).GetChildren();
                scalePrimitves[0].AddFromRemote(remoteScaleX);
                scalePrimitves[1].AddFromRemote(remoteScaleY);
                scalePrimitves[2].AddFromRemote(remoteScaleZ);

                ObjectElement remoteRotationElement = ObjectElement.Cast(modelPrimitives[3].NetworkElement);
                Element remoteRotationX = remoteRotationElement.GetElement("x");
                Element remoteRotationY = remoteRotationElement.GetElement("y");
                Element remoteRotationZ = remoteRotationElement.GetElement("z");
                Element remoteRotationW = remoteRotationElement.GetElement("w");

                SyncPrimitive[] rotationPrimitves = ((SyncObject)modelPrimitives[3]).GetChildren();
                rotationPrimitves[0].AddFromRemote(remoteRotationX);
                rotationPrimitves[1].AddFromRemote(remoteRotationY);
                rotationPrimitves[2].AddFromRemote(remoteRotationZ);
                rotationPrimitves[3].AddFromRemote(remoteRotationW);
            }

            SyncModel.InitializeLocal(syncObj.Element);

            if (AppConfig.IsServerInstance)
            {
                _enabled.Value = gameObject.activeInHierarchy;
                _position.Value = gameObject.transform.localPosition;
                _scale.Value = gameObject.transform.localScale;
                _rotation.Value = gameObject.transform.localRotation;
            }

        }

        private void OnEnabledChange(bool value)
            => SyncModel.SyncEnabled.Value = value;

        private void OnPositionChange(Vector3 value)
            => SyncModel.SyncPosition.Value = value;

        private void OnScaleChange(Vector3 value)
            => SyncModel.SyncScale.Value = value;

        private void OnRotationChange(Quaternion value)
            => SyncModel.SyncRotation.Value = value;

        private void OnRemoteEnabledChange(SyncBool syncBool)
        {
            //Debug.Log(gameObject.name + ": Remote bool change received!");

            if (!SyncActive)
                return;

            gameObject.SetActive(syncBool.Value);
            transform.hasChanged = false;
            //TransformChange();
        }

        private void OnRemotePositionChange(SyncObject syncObj)
        {
            //Debug.Log(gameObject.name + ": Remote position change received!");

            if (!SyncActive)
                return;

            SyncVector3 syncPosition = (SyncVector3)syncObj;
            transform.localPosition = syncPosition.Value;
            transform.hasChanged = false;
            //TransformChange();
        }

        private void OnRemoteScaleChange(SyncObject syncObj)
        {
            //Debug.Log(gameObject.name + ": Remote scale change received!");

            if (!SyncActive)
                return;

            SyncVector3 syncScale = (SyncVector3)syncObj;
            transform.localScale = syncScale.Value;
            transform.hasChanged = false;
            //TransformChange();
        }

        private void OnRemoteRotationChange(SyncObject syncObj)
        {
            //Debug.Log(gameObject.name + ": Remote rotation change received!");

            if (!SyncActive)
                return;

            SyncQuaternion syncRotation = (SyncQuaternion)syncObj;
            transform.localRotation = syncRotation.Value;
            transform.hasChanged = false;
            //TransformChange();
        }
    }
}