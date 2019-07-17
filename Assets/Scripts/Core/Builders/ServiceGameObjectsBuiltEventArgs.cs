using HoloIslandVis.Core.Metaphor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Core.Builders
{
    public class ServiceGameObjectsBuiltEventArgs : EventArgs
    {
        public List<Island> Islands { get; private set; }

        public ServiceGameObjectsBuiltEventArgs(List<Island> islands)
        {
            Islands = islands;
        }
    }
}
