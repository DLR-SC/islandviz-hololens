using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickGraph;

namespace HoloIslandVis.OSGiParser.Graph
{
    public class GraphEdge : Edge<GraphVertex>
    {
        private float _weight;

        public float Weight {
            get { return _weight; }
            set { _weight = value; }
        }

        public GraphEdge(GraphVertex _source, GraphVertex _target)
            : base(_source, _target)
        {
            _weight = 1f;
        }
    }
}
