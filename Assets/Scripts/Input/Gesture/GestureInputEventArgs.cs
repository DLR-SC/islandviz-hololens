using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Input.Gesture
{
    public class GestureInputEventArgs : InputEventArgs
    {
        public bool IsRemoteInput;
        public List<uint> SourceIds;
        public Dictionary<uint, Vector3> SourcePositions;

        public GestureInputEventArgs(GestureSource[] gestureSources)
        {
            IsRemoteInput = false;
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

        public GestureInputEventArgs(List<uint> sourceIds, Dictionary<uint, Vector3> sourcePositions)
        {
            IsRemoteInput = false;
            SourceIds = sourceIds;
            SourcePositions = sourcePositions;
        }

        // Returns the source position if at least one source position is available.
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

        // Returns the source positions if two gesture sources are available.
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