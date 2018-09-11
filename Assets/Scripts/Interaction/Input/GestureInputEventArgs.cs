using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Input
{
    public class GestureInputEventArgs : InputEventArgs
    {
        public List<uint> SourceIds;
        public Dictionary<uint, Vector3> SourcePositions;

        public GestureInputEventArgs(GestureSource[] gestureSources)
        {
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

        public bool TryGetSourcePosition(uint sourceId, out Vector3 sourcePosition)
        {
            sourcePosition = default(Vector3);

            if(SourcePositions.ContainsKey(sourceId))
            {
                sourcePosition = SourcePositions[sourceId];
                return true;
            }

            return false;
        }
    }
}