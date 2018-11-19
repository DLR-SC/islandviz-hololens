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
                += OnGestureInteractionReceived;
        }

        public void OnGestureInteractionReceived(NetworkInMessage msg)
        {
            GestureInputEventArgs eventArgs;
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

            eventArgs = new GestureInputEventArgs(sourceIds, sourcePositions);
            string targetName = msg.ReadString().GetString();
        }

        public void OnSpeechInteractionReceived(NetworkInMessage msg)
        {

        }

        public override void OnOneHandTap(GestureInputEventArgs eventArgs)
        {
            GameObject target = RuntimeCache.Instance.CurrentFocus;
            SharingClient.UserMessageID messageType = SharingClient.UserMessageID.GestureInteraction;
            SharingClient.Instance.SendGestureInteraction((byte)messageType, target, eventArgs);
        }

        public override void OnOneHandDoubleTap(GestureInputEventArgs eventArgs)
        {
            throw new System.NotImplementedException();
        }

        public override void OnOneHandManipStart(GestureInputEventArgs eventArgs)
        {
            throw new System.NotImplementedException();
        }

        public override void OnTwoHandManipStart(GestureInputEventArgs eventArgs)
        {
            throw new System.NotImplementedException();
        }

        public override void OnSpeechResponse(SpeechInputEventArgs eventArgs)
        {
            throw new System.NotImplementedException();
        }
    }
}
