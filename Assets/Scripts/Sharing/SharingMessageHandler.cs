using HoloIslandVis.Component.UI;
using HoloIslandVis.Utility;
using HoloIslandVis.Sharing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharingMessageHandler : MonoBehaviour
{
    public delegate void TransformChangeCallback();
    public event TransformChangeCallback TransformChange = delegate { };

    public SharingClient.UserMessageID MessageType;

    private void Start()
    {
        if (SharingClient.Instance.IsInitialized)
        {
            initHandler();
            return;
        }

        SharingClient.Instance.SharingClientInitialized += initialized;
    }

    private void Update()
    {
        if (transform.hasChanged && SharingClient.Instance.IsInitialized)
        {
            transform.hasChanged = false;
            OnTransformChange();
        }
    }

    private void initHandler()
        => SharingClient.Instance.MessageHandlers[MessageType] += updateTransform;

    private void initialized(object sender = null, EventArgs e = null)
    {
        SharingClient.Instance.SharingClientInitialized -= initialized;
        initHandler();
    }

    private void OnTransformChange()
    {
        sendTransform();
    }

    private void updateTransform(HoloToolkit.Sharing.NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();
        Vector3 recvPosition = SharingClient.Instance.ReadVector3(msg);
        Vector3 recvScale = SharingClient.Instance.ReadVector3(msg);
        Quaternion recvRotation = SharingClient.Instance.ReadQuaternion(msg);

        transform.localPosition = recvPosition;
        transform.localScale = recvScale;
        transform.localRotation = recvRotation;

        // External change is not falsely recognized as transform change.
        transform.hasChanged = false;
    }

    private void sendTransform()
        => SharingClient.Instance.SendTransform((byte)MessageType, gameObject);
}
