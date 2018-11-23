using HoloIslandVis.Component;
using HoloIslandVis.Utility;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStateSynchronizer : MonoBehaviour
{
    public delegate void TransformChangeHandler();
    public event TransformChangeHandler TransformChange = delegate { };

    public bool SyncTransform;
    public SyncObjectModel SyncObjectModel;

    private Observable<bool> _enabled;
    private Observable<Vector3> _position;
    private Observable<Vector3> _scale;
    private Observable<Quaternion> _rotation;

    private void Awake ()
    {
        _enabled    = new Observable<bool>(false);
        _position   = new Observable<Vector3>(Vector3.zero);
        _scale      = new Observable<Vector3>(Vector3.one);
        _rotation   = new Observable<Quaternion>(Quaternion.identity);

        if (SharingStage.Instance.IsConnected)
        {
            Init();
            return;
        }

        SharingStage.Instance.SharingManagerConnected += Init;
    }

    private void Update()
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

    private void Init(object sender = null, EventArgs eventArgs = null)
    {
        _enabled.ValueChanged += OnEnabledChange;
        _position.ValueChanged += OnPositionChange;
        _scale.ValueChanged += OnScaleChange;
        _rotation.ValueChanged += OnRotationChange;

        SyncObjectModel = new SyncObjectModel(gameObject.name + "_syncObject", SyncTransform);
        
        SyncObjectModel.SyncPosition.ObjectChanged += OnRemotePositionChange;
        SyncObjectModel.SyncScale.ObjectChanged += OnRemoteScaleChange;
        SyncObjectModel.SyncRotation.ObjectChanged += OnRemoteRotationChange;

        ObjectStateSynchronizer parentStateSynchronizer 
            = gameObject.transform.parent.GetComponentInParent<ObjectStateSynchronizer>();

        if (parentStateSynchronizer == null)
        {
            SyncObjectModel.InitializeLocal(SharingStage.Instance.Root.Element);
            SyncObjectModel.InitializationComplete += InitElement;
        }
        else
        {
            if(parentStateSynchronizer.SyncObjectModel.Element == null)
            {
                parentStateSynchronizer.SyncObjectModel.InitializationComplete += (SyncObject syncObj) =>
                {
                    SyncObjectModel.InitializeLocal(syncObj.Element);
                    SyncObjectModel.InitializationComplete += InitElement;
                };
            }
            else
            {
                SyncObjectModel.InitializeLocal(parentStateSynchronizer.SyncObjectModel.Element);
                SyncObjectModel.InitializationComplete += InitElement;
            }
        }

        SharingStage.Instance.SharingManagerConnected -= Init;
    }

    private void InitElement(SyncObject syncObj)
    {
        Debug.Log("Initialized: " + syncObj.FieldName);
        SyncObjectModel.SyncEnabled.InitializeLocal(SyncObjectModel.Element);
        SyncObjectModel.SyncPosition.InitializeLocal(SyncObjectModel.Element);
        SyncObjectModel.SyncScale.InitializeLocal(SyncObjectModel.Element);
        SyncObjectModel.SyncRotation.InitializeLocal(SyncObjectModel.Element);
        syncObj.InitializationComplete -= InitElement;
    }

    private void OnEnable()
        => _enabled.Value = true;

    private void OnDisable()
        => _enabled.Value = false;

    private void OnEnabledChange(bool value)
        => SyncObjectModel.SyncEnabled.Value = value;

    private void OnPositionChange(Vector3 value)
        => SyncObjectModel.SyncPosition.Value = value;

    private void OnScaleChange(Vector3 value)
        => SyncObjectModel.SyncScale.Value = value;

    private void OnRotationChange(Quaternion value)
        => SyncObjectModel.SyncRotation.Value = value;

    private void OnRemoteEnabledChange(SyncObject syncObj)
    {
        
    }

    private void OnRemotePositionChange(SyncObject syncObj)
    {
        Debug.Log("Remote position change received!");
        SyncVector3 syncPosition = (SyncVector3)syncObj;
        transform.localPosition = syncPosition.Value;
        transform.hasChanged = false;
    }

    private void OnRemoteScaleChange(SyncObject syncObj)
    {
        Debug.Log("Remote scale change received!");
        SyncVector3 syncScale = (SyncVector3)syncObj;
        transform.localScale = syncScale.Value;
        transform.hasChanged = false;
    }

    private void OnRemoteRotationChange(SyncObject syncObj)
    {
        Debug.Log("Remote rotation change received!");
        SyncQuaternion syncRotation = (SyncQuaternion)syncObj;
        transform.localRotation = syncRotation.Value;
        transform.hasChanged = false;
    }
}
