using HoloIslandVis.Component.UI;
using HoloIslandVis.Utility;
using HoloIslandVis.Automaton;
using HoloIslandVis.Input;
using HoloToolkit.Sharing;
using System.Collections.Generic;
using UnityEngine;

using GestureType = HoloIslandVis.Input.GestureType;

namespace HoloIslandVis.Sharing
{
    public class RemoteExecutionHelper : InputReceiver
    {
        private StateMachine _stateMachine;

        public RemoteExecutionHelper(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;

            SharingClient.Instance.MessageHandlers[SharingClient.UserMessageID.Transform]
                += OnTransformReceived;

            SharingClient.Instance.MessageHandlers[SharingClient.UserMessageID.GestureInteraction]
                += OnGestureInputReceived;

            SharingClient.Instance.MessageHandlers[SharingClient.UserMessageID.SpeechInteraction]
                += OnSpeechInputReceived;
        }

        public void OnTransformReceived(NetworkInMessage msg)
        {
            long remoteUserId = msg.ReadInt64();
            Vector3 position = SharingClient.Instance.ReadVector3(msg);
            Vector3 scale = SharingClient.Instance.ReadVector3(msg);
            Quaternion rotation = SharingClient.Instance.ReadQuaternion(msg);
            string targetName = msg.ReadString().ToString();

            GameObject target = GameObject.Find(targetName);
            if(target != null)
            {
                target.transform.localPosition = position;
                target.transform.localScale = scale;
                target.transform.localRotation = rotation;
            }
        }

        public void OnGestureInputReceived(NetworkInMessage msg)
        {
            long remoteUserId = msg.ReadInt64();

            GestureInputEventArgs eventArgs;
            GestureType gestureType = (GestureType)msg.ReadByte();
            byte sourceCount = msg.ReadByte();

            List<uint> sourceIds = new List<uint>();
            var sourcePositions = new Dictionary<uint, Vector3>();

            for (int i = 0; i < sourceCount; i++)
            {
                Vector3 sourcePosition = SharingClient.Instance.ReadVector3(msg);
                uint sourceId = (uint)msg.ReadInt32();
                sourcePositions.Add(sourceId, sourcePosition);
                sourceIds.Add(sourceId);
            }

            eventArgs = new GestureInputEventArgs(gestureType, sourceIds, sourcePositions);

            byte targetSet = msg.ReadByte();
            if (targetSet == 1)
            {
                string targetName = msg.ReadString().GetString();
                eventArgs.Target = GameObject.Find(targetName);
            }

            eventArgs.IsRemoteInput = true;
            GestureInputListener.Instance.InvokeGestureInputEvent(eventArgs);
        }

        public void OnSpeechInputReceived(NetworkInMessage msg)
        {

        }

        public override void OnOneHandTap(GestureInputEventArgs eventArgs)
            => SendGestureInputEvent(eventArgs);

        public override void OnOneHandDoubleTap(GestureInputEventArgs eventArgs)
            => SendGestureInputEvent(eventArgs);

        public override void OnOneHandManipStart(GestureInputEventArgs eventArgs)
            => SendGestureInputEvent(eventArgs);

        public override void OnTwoHandManipStart(GestureInputEventArgs eventArgs)
            => SendGestureInputEvent(eventArgs);

        public override void OnManipulationUpdate(GestureInputEventArgs eventArgs)
            => SendGestureInputEvent(eventArgs);

        public override void OnManipulationEnd(GestureInputEventArgs eventArgs)
            => SendGestureInputEvent(eventArgs);

        public override void OnSpeechResponse(SpeechInputEventArgs eventArgs)
        {
            
        }

        private void SendGestureInputEvent(GestureInputEventArgs eventArgs)
        {
            if (eventArgs.IsRemoteInput)
                return;

            GameObject target = RuntimeCache.Instance.CurrentFocus;
            SharingClient.UserMessageID messageType = SharingClient.UserMessageID.GestureInteraction;
            SharingClient.Instance.SendGestureInputEvent((byte)messageType, eventArgs);
        }
    }
}
