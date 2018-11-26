using HoloIslandVis.Component.UI;
using HoloIslandVis.Component;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;
using UnityEngine.UI;

public class ObjectStateSynchronizer : MonoBehaviour
{
    public delegate void TransformChangeHandler();
    public event TransformChangeHandler TransformChange = delegate { };

    public bool SyncTransform;

    [SyncData]
    public SyncModel SyncModel;

    private Observable<bool> _enabled;
    private Observable<Vector3> _position;
    private Observable<Vector3> _scale;
    private Observable<Quaternion> _rotation;

    private void Awake ()
    {
        Vector3 init = new Vector3(Random.value, Random.value, Random.value);
        _enabled = new Observable<bool>(false);
        _position = new Observable<Vector3>(init);
        _scale = new Observable<Vector3>(init);
        _rotation = new Observable<Quaternion>(Quaternion.Euler(init));

        if (SharingStage.Instance.IsConnected)
        {
            InitSyncModel();
            return;
        }

        SharingStage.Instance.SharingManagerConnected += InitSyncModel;
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

    private void InitSyncModel(object sender = null, EventArgs eventArgs = null)
    {
        SharingStage.Instance.SharingManagerConnected -= InitSyncModel;
        SyncModel = new SyncModel(gameObject.name, SyncTransform);

        SyncModel.SyncBoolChanged += OnRemoteEnabledChange;

        if(gameObject.name != "Panel" && !gameObject.name.Contains("_Highlight"))
        {
            SyncModel.SyncPosition.ObjectChanged += OnRemotePositionChange;
            SyncModel.SyncScale.ObjectChanged += OnRemoteScaleChange;
            SyncModel.SyncRotation.ObjectChanged += OnRemoteRotationChange;
        }

        _enabled.ValueChanged += OnEnabledChange;
        _position.ValueChanged += OnPositionChange;
        _scale.ValueChanged += OnScaleChange;
        _rotation.ValueChanged += OnRotationChange;

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

        Element remoteModelElement = syncObj.Element.GetElement(SyncModel.FieldName);
        if (remoteModelElement != null)
        {
            //Debug.Log("Adding exsisting element.");
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

        _enabled.Value = gameObject.activeInHierarchy;
        _position.Value = gameObject.transform.localPosition;
        _scale.Value = gameObject.transform.localScale;
        _rotation.Value = gameObject.transform.localRotation;
    }

    private void OnEnable()
        => _enabled.Value = true;

    private void OnDisable()
        => _enabled.Value = false;

    private void OnEnabledChange(bool value)
    {
        SyncModel.SyncEnabled.Value = value;
    }

    private void OnPositionChange(Vector3 value)
    {
        SyncModel.SyncPosition.Value = value;
    }

    private void OnScaleChange(Vector3 value)
    {
        SyncModel.SyncScale.Value = value;
    }

    private void OnRotationChange(Quaternion value)
    {
        SyncModel.SyncRotation.Value = value;
    }

    private void OnRemoteEnabledChange(SyncBool syncBool)
    {
        Debug.Log(gameObject.name + ": Remote bool change received!");

        gameObject.SetActive(syncBool.Value);
        if (gameObject.name.Contains("_Highlight"))
            gameObject.GetComponent<MeshRenderer>().enabled = syncBool.Value;

        changePanelInfo();
        transform.hasChanged = false;
        TransformChange();
    }

    private void OnRemotePositionChange(SyncObject syncObj)
    {
        Debug.Log(gameObject.name + ": Remote position change received!");
        SyncVector3 syncPosition = (SyncVector3)syncObj;
        transform.localPosition = syncPosition.Value;
        transform.hasChanged = false;
        TransformChange();
    }

    private void OnRemoteScaleChange(SyncObject syncObj)
    {
        Debug.Log(gameObject.name + ": Remote scale change received!");
        SyncVector3 syncScale = (SyncVector3)syncObj;
        transform.localScale = syncScale.Value;
        transform.hasChanged = false;
        TransformChange();
    }

    private void OnRemoteRotationChange(SyncObject syncObj)
    {
        Debug.Log(gameObject.name + ": Remote rotation change received!");
        SyncQuaternion syncRotation = (SyncQuaternion)syncObj;
        transform.localRotation = syncRotation.Value;
        transform.hasChanged = false;
        TransformChange();
    }

    private void changePanelInfo()
    {
        if (gameObject.name.Contains("_Highlight"))
        {
            GameObject currentFocus = gameObject.transform.parent.gameObject;
            GameObject infoPanel = UserInterface.Instance.Panel;
            RuntimeCache cache = RuntimeCache.Instance;
            Island island = RuntimeCache.Instance.GetIsland(currentFocus.name);
            Transform[] transforms = currentFocus.transform.GetComponentsInChildren<Transform>(true);
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            //infoPanel.SetActive(true);

            //Debug.Log("This Bundle contains:\n"
            //    + island.Regions.Count + " packages\n"
            //    + island.CartographicIsland.Bundle.CompilationUnitCount + " compilation units\n"
            //    + island.CartographicIsland.Bundle.ExportedPackages.Count + " exports\n"
            //    + island.CartographicIsland.Bundle.ImportedPackages.Count + " imports\n"
            //    + island.CartographicIsland.Bundle.ServiceComponents.Count + " services\n");

            infoPanel.transform.Find("Canvas").Find("Title").GetComponent<Text>().text = currentFocus.name;
            infoPanel.transform.Find("Canvas").Find("Maintext").GetComponent<Text>().text =
                "This Bundle contains:\n"
                + island.Regions.Count + " packages\n"
                + island.CartographicIsland.Bundle.CompilationUnitCount + " compilation units\n"
                + island.CartographicIsland.Bundle.ExportedPackages.Count + " exports\n"
                + island.CartographicIsland.Bundle.ImportedPackages.Count + " imports\n"
                + island.CartographicIsland.Bundle.ServiceComponents.Count + " services\n";
        }
    }
}
