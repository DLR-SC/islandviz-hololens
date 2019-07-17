using HoloIslandVis.Core.Metaphor;
using System;
using System.Collections.Generic;

namespace HoloIslandVis.Core.Builders
{
    public class IslandGameObjectsBuiltEventArgs : EventArgs
    {
        public List<Island> Islands { get; private set; }

        public IslandGameObjectsBuiltEventArgs(List<Island> islands)
        {
            Islands = islands;
        }
    }
}
