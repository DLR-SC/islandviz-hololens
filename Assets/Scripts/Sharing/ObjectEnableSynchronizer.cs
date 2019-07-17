using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System;
using UnityEngine;

using Random = UnityEngine.Random;
using UnityEngine.UI;
using HoloIslandVis.Core;

namespace HoloIslandVis.Sharing
{
    public class ObjectEnableSynchronizer : MonoBehaviour
    {
        public delegate void TransformChangeHandler();
        public event TransformChangeHandler TransformChange = delegate { };

        public bool SyncTransform;
        public AppConfig AppConfig;

        [SyncData]
        public SyncEnableModel SyncEnableModel;

        private Observable<bool> _enabled;

        private void Awake()
        {
            AppConfig = GameObject.Find("AppConfig").GetComponent<AppConfig>();

            _enabled = new Observable<bool>(false);

            if (SharingStage.Instance != null && SharingStage.Instance.IsConnected)
            {
                InitSyncModel();
                return;
            }

            VisualizationLoader.Instance.VisualizationLoaded += OnVisualizationLoaded;
        }

        private void Update()
        {

        }

        private void OnVisualizationLoaded()
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
            SharingStage.Instance.SharingManagerConnected -= InitSyncModel;
            SyncEnableModel = new SyncEnableModel(gameObject.name);

            SyncEnableModel.SyncBoolChanged += OnRemoteEnabledChange;

            if (AppConfig.IsServerInstance)
            {
                _enabled.ValueChanged += OnEnabledChange;
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
            Debug.Log("[SyncSystem] Initializing " + gameObject.name);
            syncObj.InitializationComplete -= OnSyncObjectInitialized;

            Element remoteModelElement = syncObj.Element.GetElement(SyncEnableModel.FieldName);

            if (remoteModelElement != null)
            {
                SyncEnableModel.AddFromRemote(remoteModelElement);

                Element remoteSyncEnabledElement = SyncEnableModel.Element.GetElement("SyncEnabled");

                SyncPrimitive[] modelPrimitives = SyncEnableModel.GetChildren();
                modelPrimitives[0].AddFromRemote(remoteSyncEnabledElement);
            }

            SyncEnableModel.InitializeLocal(syncObj.Element);

            if (AppConfig.IsServerInstance)
            {
                _enabled.Value = gameObject.activeInHierarchy;
            }

        }

        private void OnEnable()
            => _enabled.Value = true;

        private void OnDisable()
            => _enabled.Value = false;

        private void OnEnabledChange(bool value)
            => SyncEnableModel.SyncEnabled.Value = value;

        private void OnRemoteEnabledChange(SyncBool syncBool)
        {
            Debug.Log(gameObject.name + ": Remote bool change received!");
            gameObject.SetActive(syncBool.Value);
            transform.hasChanged = false;
            TransformChange();
        }
    }
}