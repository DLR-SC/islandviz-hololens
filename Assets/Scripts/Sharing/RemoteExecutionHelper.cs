using HoloIslandVis.Utility;
using HoloIslandVis.Automaton;
using HoloIslandVis.Input;
using HoloToolkit.Sharing;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    public class RemoteExecutionHelper : InputReceiver
    {
        private StateMachine _stateMachine;

        public RemoteExecutionHelper(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            SharingClient.Instance.MessageHandlers[SharingClient.UserMessageID.GestureInteraction]
                += OnGestureInputReceived;
        }

        public void OnGestureInputReceived(NetworkInMessage msg)
        {
            GestureInputEventArgs eventArgs;
            short inputData = msg.ReadInt16();
            byte sourceCount = msg.ReadByte();
            List<uint> sourceIds = new List<uint>();
            var sourcePositions = new Dictionary<uint, Vector3>();

            for(int i = 0; i < sourceCount; i++)
            {
                Vector3 sourcePosition = SharingClient.Instance.ReadVector3(msg);
                uint sourceId = (uint)msg.ReadInt32();
                sourcePositions.Add(sourceId, sourcePosition);
                sourceIds.Add(sourceId);
            }

            eventArgs = new GestureInputEventArgs(inputData, sourceIds, sourcePositions);
            if(msg.ReadByte() == 1)
            {
                string targetName = msg.ReadString().GetString();
                eventArgs.Target = GameObject.Find(targetName);
            }

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

        public override void OnSpeechResponse(SpeechInputEventArgs eventArgs)
        {
            
        }

        private void SendGestureInputEvent(GestureInputEventArgs eventArgs)
        {
            GameObject target = RuntimeCache.Instance.CurrentFocus;
            SharingClient.UserMessageID messageType = SharingClient.UserMessageID.GestureInteraction;
            SharingClient.Instance.SendGestureInputEvent((byte)messageType, eventArgs);
        }
    }
}
