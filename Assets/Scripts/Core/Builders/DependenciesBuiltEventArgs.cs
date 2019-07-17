using HoloIslandVis.Core.Metaphor;
using System;
using System.Collections.Generic;

namespace HoloIslandVis.Core.Builders
{
    public class DependenciesBuiltEventArgs : EventArgs
    {
        public List<Island> Islands { get; private set; }

        public DependenciesBuiltEventArgs(List<Island> islands)
        {
            Islands = islands;
        }
    }
}
