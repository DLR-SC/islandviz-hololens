using HoloIslandVis.Core;
using UnityEngine;

namespace HoloIslandVis.Model.Graph
{
    public class GraphVertex
    {

        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public CartographicIsland Island { get; set; }

        public GraphVertex(string name)
        {
            Name = name;
            Position = Vector3.zero;
        }
    }
}
