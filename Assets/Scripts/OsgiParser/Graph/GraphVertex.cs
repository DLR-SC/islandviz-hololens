using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;
//using OsgiViz.Island;

namespace IslandVis.OSGiParser.Graph
{
    public class GraphVertex
    {
        private string _name;
        private Vector3 _position;
        private CartographicIsland _island;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public Vector3 Position {
            get { return _position; }
            set { _position = value; }
        }


        public CartographicIsland Island {
            get { return _island; }
            set { _island = value; }
        }

        public GraphVertex(string name)
        {
            _name = name;
            _position = Vector3.zero;
        }
    }
}
