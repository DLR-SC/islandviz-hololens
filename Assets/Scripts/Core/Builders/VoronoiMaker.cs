using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Voronoi;

using VFace = TriangleNet.Topology.DCEL.Face;
using TNetMesh = TriangleNet.Mesh;
using VHEdge = TriangleNet.Topology.DCEL.HalfEdge;
using HoloIslandVis.Sharing;

namespace HoloIslandVis.Core.Builders
{
    public class VoronoiMaker : Singleton<VoronoiMaker>
    {
        private VoronoiMaker()
        {

        }

        public BoundedVoronoi CreateRelaxedVoronoi(int lloydRelaxations, float cellScale, int seed)
        {
            System.Random random = new System.Random(seed);

            List<Vertex> startingVertices = CreatePointsOnPlane(cellScale, cellScale, 50, 50, 1.0f, random);
            List<Vertex> vertices = new List<Vertex>();

            TriangleNet.Configuration config = new TriangleNet.Configuration();
            SweepLine sweepline  = new SweepLine();

            TNetMesh tMesh = (TNetMesh) sweepline.Triangulate(startingVertices, config);
            BoundedVoronoi voronoi = new BoundedVoronoi(tMesh);

            for (int i = 0; i < lloydRelaxations; i++)
            {
                foreach (VFace face in voronoi.Faces)
                {
                    if (CheckCell(face))
                    {
                        Vertex newCentroid = new Vertex(0, 0);
                        int vertexCount = 0;
                        foreach (VHEdge edge in face.EnumerateEdges())
                        {
                            newCentroid.X += edge.Origin.X;
                            newCentroid.Y += edge.Origin.Y;
                            vertexCount++;
                        }
                        newCentroid.X /= vertexCount;
                        newCentroid.Y /= vertexCount;
                        vertices.Add(newCentroid);
                    }
                }

                tMesh = (TNetMesh) sweepline.Triangulate(vertices, config);
                voronoi = new BoundedVoronoi(tMesh);
                vertices.Clear();
            }

            return voronoi;
        }

        public List<Vertex> CreatePointsOnPlane(float widthScale, 
                                                float heightScale, 
                                                int widthSegments, 
                                                int heightSegments, 
                                                float preturbFactor, 
                                                System.Random random)
        {

            float preturbFactorX = preturbFactor * (widthScale / widthSegments);
            float preturbFactorY = preturbFactor * (heightScale / heightSegments);
            int hCount2 = widthSegments + 1;
            int vCount2 = heightSegments + 1;
            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount2 * vCount2;

            List<Vertex> vertices = new List<Vertex>();

            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / heightSegments;
            float scaleX = widthScale / widthSegments;
            float scaleY = heightScale / heightSegments;

            for (int y = 0; y < vCount2; y++)
            {
                for (int x = 0; x < hCount2; x++)
                {
                    Vertex newVert = new Vertex(x * scaleX - widthScale / 2f, y * scaleY - heightScale / 2f);
                    newVert.X += SyncDataStorage.Instance.GetRandomNumber(random) * preturbFactorX;
                    newVert.Y += SyncDataStorage.Instance.GetRandomNumber(random) * preturbFactorY;
                    vertices.Add(newVert);
                }
            }

            return vertices;
        }

        public bool CheckCell(VFace cell)
        {
            if (cell.Bounded && cell.ID != -1)
                return true;
            else
                return false;
        }
    }
}
