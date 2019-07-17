using QuickGraph;

namespace HoloIslandVis.Model.Graph
{
    public class GraphEdge : Edge<GraphVertex>
    {
        public float Weight { get; set; }

        public GraphEdge(GraphVertex _source, GraphVertex _target)
            : base(_source, _target)
        {
            Weight = 1f;
        }
    }
}
