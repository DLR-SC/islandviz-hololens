using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Input
{
    public class GestureInputEventArgs : InputEventArgs
    {
        public bool IsRemoteInput;
        public GestureType GestureType;
        public GameObject Target;
        public List<uint> SourceIds;
        public Dictionary<uint, Vector3> SourcePositions;

        public GestureInputEventArgs(GestureType gestureType, GestureSource[] gestureSources)
        {
            IsRemoteInput = false;
            GestureType = gestureType;
            SourceIds = new List<uint>();
            SourcePositions = new Dictionary<uint, Vector3>();

            foreach (GestureSource source in gestureSources)
            {
                Vector3 sourcePosition;
                uint sourceId = source.SourceId;

                SourceIds.Add(sourceId);

                if (source.InputSource.TryGetGripPosition(sourceId, out sourcePosition))
                    SourcePositions.Add(source.SourceId, sourcePosition);
            }
        }

        public GestureInputEventArgs(GestureType gestureType, List<uint> sourceIds, Dictionary<uint, Vector3> sourcePositions)
        {
            IsRemoteInput = false;
            GestureType = gestureType;
            SourceIds = sourceIds;
            SourcePositions = sourcePositions;
        }

        public bool TryGetSingleGripPosition(out Vector3 sourceOnePos)
        {
            sourceOnePos = default(Vector3);

            if(SourceIds.Count < 1)
                return false;

            if(!SourcePositions.ContainsKey(SourceIds[0]))
                return false;

            sourceOnePos = SourcePositions[SourceIds[0]];

            return true;
        }

        public bool TryGetDoubleGripPosition(out Vector3 sourceOnePos, out Vector3 sourceTwoPos)
        {
            sourceOnePos = default(Vector3);
            sourceTwoPos = default(Vector3);

            if(SourceIds.Count < 2)
                return false;

            foreach(uint sourceId in SourceIds)
            {
                if(!SourcePositions.ContainsKey(sourceId))
                    return false;
            }

            sourceOnePos = SourcePositions[SourceIds[0]];
            sourceTwoPos = SourcePositions[SourceIds[1]];

            return true;
        }
    }
}