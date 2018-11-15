using HoloIslandVis;
using HoloIslandVis.Utility;
using HoloToolkit.Sharing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    public class SharingClient : Singleton<SharingClient>
    {
        public delegate void MessageCallback(NetworkInMessage msg);
        public event EventHandler SharingClientInitialized = delegate { };

        public enum UserMessageID : byte
        {
            SurfaceTransform = MessageID.UserMessageIDStart,
            ContainerTransform,
            Max
        }

        public bool IsConnected {
            get { return SharingStage.Instance.IsConnected; }
        }

        public Dictionary<UserMessageID, MessageCallback> MessageHandlers {
            get { return _messageHandlers; }
        }

        public long LocalUserId { get; private set; }
        public bool IsInitialized {
            get { return _isInitialized; }
            private set {
                _isInitialized = value;

                if (_isInitialized)
                    SharingClientInitialized(null, null);
            }
        }

        private bool _isInitialized;
        private NetworkConnection _serverConnection;
        private NetworkConnectionAdapter _connectionAdapter;

        private Dictionary<UserMessageID, MessageCallback> _messageHandlers
            = new Dictionary<UserMessageID, MessageCallback>();

        private SharingClient() { }

        public void Init()
        {
            if (IsConnected)
            {
                initMessageHandlers();
                return;
            }

            SharingStage.Instance.SharingManagerConnected += connected;
        }

        public void SendTransform(byte messageType, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            if (_serverConnection != null && _serverConnection.IsConnected())
            {
                NetworkOutMessage msg = CreateMessage(messageType);
                appendTransform(msg, position, scale, rotation);
                _serverConnection.Broadcast(msg, MessagePriority.Immediate, 
                    MessageReliability.UnreliableSequenced, MessageChannel.Default);
            }
        }

        public Vector3 ReadVector3(NetworkInMessage msg)
        {
            return new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
        }

        public Quaternion ReadQuaternion(NetworkInMessage msg)
        {
            return new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
        }

        private void connected(object sender = null, EventArgs e = null)
        {
            SharingStage.Instance.SharingManagerConnected -= connected;
            initMessageHandlers();
        }

        private void initMessageHandlers()
        {
            SharingStage sharingStage = SharingStage.Instance;

            if (sharingStage == null)
                return;

            _serverConnection = sharingStage.Manager.GetServerConnection();
            if (_serverConnection == null)
                return;

            _connectionAdapter = new NetworkConnectionAdapter();
            _connectionAdapter.MessageReceivedCallback += OnMessageReceived;
            LocalUserId = SharingStage.Instance.Manager.GetLocalUser().GetID();

            for (byte index = (byte)UserMessageID.SurfaceTransform; index < (byte)UserMessageID.Max; index++)
            {
                if (_messageHandlers.ContainsKey((UserMessageID)index) == false)
                    _messageHandlers.Add((UserMessageID)index, delegate { });

                _serverConnection.AddListener(index, _connectionAdapter);
            }

            IsInitialized = true;
        }

        private NetworkOutMessage CreateMessage(byte messageType)
        {
            NetworkOutMessage msg = _serverConnection.CreateMessage(messageType);
            msg.Write(messageType);
            msg.Write(LocalUserId);
            return msg;
        }

        private void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
        {
            Debug.Log("MSG Received.");
            byte messageType = msg.ReadByte();
            _messageHandlers[(UserMessageID)messageType]?.Invoke(msg);
        }

        private void appendTransform(NetworkOutMessage msg, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            appendVector3(msg, position);
            appendVector3(msg, scale);
            appendQuaternion(msg, rotation);
        }

        private void appendVector3(NetworkOutMessage msg, Vector3 vector)
        {
            msg.Write(vector.x);
            msg.Write(vector.y);
            msg.Write(vector.z);
        }

        private void appendQuaternion(NetworkOutMessage msg, Quaternion rotation)
        {
            msg.Write(rotation.x);
            msg.Write(rotation.y);
            msg.Write(rotation.z);
            msg.Write(rotation.w);
        }
    }
}