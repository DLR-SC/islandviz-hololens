using HoloIslandVis.Core;
using HoloIslandVis.Model.Graph;
using HoloIslandVis.Model.OSGi;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Voronoi;
using UnityEngine;

using VFace = TriangleNet.Topology.DCEL.Face;
using TNetMesh = TriangleNet.Mesh;
using VHEdge = TriangleNet.Topology.DCEL.HalfEdge;
using HoloIslandVis.Sharing;

namespace HoloIslandVis.Core.Builders
{
    public class IslandStructureBuilder : SingletonComponent<IslandStructureBuilder>
    {
        // Refactor
        public const float CELL_SCALE = 20f;
        public static float[] HEIGHT_PROFILE = { -0.03f * CELL_SCALE, -0.034f * CELL_SCALE,
        -0.036f * CELL_SCALE, -0.037f * CELL_SCALE, -0.0375f * CELL_SCALE };

        public int ExpansionFactor;
        public float MinCohesion;
        public float MaxCohesion;

        private List<CartographicIsland> _cartographicIslands;
        private System.Random _random;

        private IslandStructureBuilder()
        {
            _random = new System.Random(0);
            _cartographicIslands = new List<CartographicIsland>();
        }

        public List<CartographicIsland> BuildFromProject(OSGiProject project)
        {
            _cartographicIslands.Clear();
            foreach (Bundle bundle in project.Bundles)
                _cartographicIslands.Add(BuildFromBundle(bundle));

            return _cartographicIslands;
        }

        private CartographicIsland BuildFromBundle(Bundle bundle)
        {
            int seed = bundle.GetHashCode();
            TriangleNet.Configuration config = new TriangleNet.Configuration();
            BoundedVoronoi voronoi = VoronoiMaker.Instance.CreateRelaxedVoronoi(1, CELL_SCALE, seed);

            VFace initCell = ClosestCell(0, 0, voronoi);
            var startCells = new Dictionary<int, VFace>();
            startCells.Add(initCell.ID, initCell);

            var island = new CartographicIsland(bundle, voronoi);
            int maxCount = bundle.MostCompilationUnits.CompilationUnitCount;

            foreach (Package package in bundle.Packages)
                ConstructRegion(package, island, startCells, maxCount);

            ShapeCoastArea(startCells, HEIGHT_PROFILE);
            SetCoastline(startCells, island);
            island.Radius = CalculateRadius(startCells, island);

            foreach (List<VFace> cellMap in island.PackageCells)
            {
                List<TNetMesh> packageMeshes = ConstructTNetMeshFromCellmap(cellMap);
                island.PackageMeshes.Add(packageMeshes);
            }

            List<TNetMesh> coastlineMeshes = ConstructTNetMeshFromCellmap(island.CoastlineCells);
            island.CoastlineMeshes.AddRange(coastlineMeshes);

            LinkDependencyVertex(bundle, island);

            return island;
        }

        private void ConstructRegion(Package package, CartographicIsland island,
            Dictionary<int, VFace> startCells, int maxCount)
        {
            float cohesionMultiplier = (float)(package.CompilationUnitCount / maxCount);

            cohesionMultiplier *= cohesionMultiplier * MaxCohesion;
            cohesionMultiplier = Mathf.Max(MinCohesion, cohesionMultiplier);

            var newCells = ConstructRegionFromPackage(package, island, startCells, cohesionMultiplier);
            UpdateAndFuseCells(startCells, newCells);
        }

        private VFace ClosestCell(float x, float y, BoundedVoronoi voronoi)
        {
            int startingIndex = voronoi.Faces.Count / 2;

            VFace currentCell = voronoi.Faces[startingIndex];

            float xDiff = (float)(currentCell.Generator.X - x);
            float yDiff = (float)(currentCell.Generator.Y - y);
            float currentDist = Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);

            while (true)
            {
                VFace nextCell = null;
                bool foundNeighbour = false;
                foreach (VHEdge edge in currentCell.EnumerateEdges())
                {
                    VFace nCell = edge.Twin.Face;
                    float xDiffNeighbor = (float)(nCell.Generator.X - x);
                    float yDiffNeighbor = (float)(nCell.Generator.Y - y);

                    float distFromNeighbour = Mathf.Sqrt(xDiffNeighbor * xDiffNeighbor
                        + yDiffNeighbor * yDiffNeighbor);

                    if (distFromNeighbour < currentDist)
                    {
                        foundNeighbour = true;
                        currentDist = distFromNeighbour;
                        nextCell = nCell;
                    }
                }

                if (!foundNeighbour)
                    break;

                currentCell = nextCell;
            }

            return currentCell;
        }

        private Dictionary<int, VFace> ConstructRegionFromPackage(Package package, CartographicIsland cartographicIsland,
            Dictionary<int, VFace> startCells, float cohesionMultiplier)
        {
            List<VFace> cellMap = new List<VFace>();

            Dictionary<int, VFace> newCells = new Dictionary<int, VFace>();
            VFace startCell = SelectFromCells(startCells, cohesionMultiplier).Value;

            int maxIterations = 20;
            int counter = 0;

            bool expand = ExpandCountries(package.CompilationUnits, cellMap, newCells, startCell, cohesionMultiplier);

            while (!expand && counter < maxIterations)
            {
                startCell = SelectFromCells(startCells, cohesionMultiplier).Value;
                counter++;
            }

            cartographicIsland.Packages.Add(package);
            cartographicIsland.PackageCells.Add(cellMap);

            return newCells;
        }

        private void UpdateAndFuseCells(Dictionary<int, VFace> dictA, Dictionary<int, VFace> dictB)
        {
            List<int> keysToRemove = new List<int>();

            foreach (KeyValuePair<int, VFace> keyValuePair in dictA)
            {
                if (keyValuePair.Value.Mark != 0)
                    keysToRemove.Add(keyValuePair.Key);
            }

            foreach (int key in keysToRemove)
                dictA.Remove(key);

            foreach (KeyValuePair<int, VFace> keyValuePair in dictB)
            {
                if (!dictA.ContainsKey(keyValuePair.Key))
                    dictA.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private KeyValuePair<int, VFace> SelectFromCells(Dictionary<int, VFace> cells, float cohesionMultiplier)
        {
            KeyValuePair<int, VFace> selectedCell = new KeyValuePair<int, VFace>();
            List<float> cellScores = new List<float>();
            float totalScore = 0f;

            foreach (KeyValuePair<int, VFace> keyValuePair in cells)
            {
                int n = 0;
                foreach (VHEdge edge in keyValuePair.Value.EnumerateEdges())
                {
                    int mark = edge.Twin.Face.Mark;
                    if (mark != 0)
                        n++;
                }

                float score = Mathf.Pow(cohesionMultiplier, n);
                cellScores.Add(score);
                totalScore += score;
            }

            for (int i = 0; i < cellScores.Count; i++)
                cellScores[i] = cellScores[i] / totalScore;

            float randomNumber = SyncDataStorage.Instance.GetRandomNumber(_random);

            int counter = 0;
            foreach (KeyValuePair<int, VFace> keyValuePair in cells)
            {
                randomNumber -= cellScores[counter];
                if (randomNumber <= 0f)
                {
                    selectedCell = keyValuePair;
                    break;
                }

                counter++;
            }

            return selectedCell;
        }

        private bool ExpandCountries(List<CompilationUnit> compilationUnits, List<VFace> cellMap,
            Dictionary<int, VFace> endCells, VFace startCell, float cohesionMultiplier)
        {
            Dictionary<int, VFace> cells = new Dictionary<int, VFace>();
            cells.Add(startCell.ID, startCell);

            for (int i = 1; i < compilationUnits.Count * ExpansionFactor + 1; i++)
            {
                if (cells.Count == 0)
                {
                    cellMap.Clear();
                    endCells.Clear();
                    return false;
                }

                KeyValuePair<int, VFace> selectedCell = SelectFromCells(cells, cohesionMultiplier);
                selectedCell.Value.Mark = i;
                cellMap.Add(selectedCell.Value);
                cells.Remove(selectedCell.Key);

                foreach (VHEdge edge in selectedCell.Value.EnumerateEdges())
                {
                    VFace nCell = edge.Twin.Face;
                    float scale = CELL_SCALE * 0.4f;
                    bool cellValid = CheckCell(nCell, -scale, scale, -scale, scale);

                    if (cellValid && nCell.Mark == 0 && !cells.ContainsKey(nCell.ID))
                        cells.Add(nCell.ID, nCell);
                }

            }

            //transfer candidates into endCandidates Dict
            foreach (KeyValuePair<int, VFace> keyValuePair in cells)
                endCells.Add(keyValuePair.Key, keyValuePair.Value);

            return true;
        }

        private void ShapeCoastArea(Dictionary<int, VFace> coastline, float[] heightProfile)
        {
            Dictionary<int, VFace> newCoastline = new Dictionary<int, VFace>(coastline);

            for (int i = 0; i < heightProfile.Length; i++)
            {
                newCoastline.Clear();
                foreach (KeyValuePair<int, VFace> kvp in coastline)
                {
                    foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                    {
                        VFace nCell = edge.Twin.Face;
                        float scale = CELL_SCALE * 0.4f;
                        bool cellValid = CheckCell(nCell, -scale, scale, -scale, scale);

                        if (cellValid && nCell.Mark == 0 && !coastline.ContainsKey(nCell.ID)
                            && !newCoastline.ContainsKey(nCell.ID))
                            newCoastline.Add(nCell.ID, nCell);
                    }
                }

                foreach (KeyValuePair<int, VFace> keyValuePair in newCoastline)
                {
                    coastline.Add(keyValuePair.Key, keyValuePair.Value);
                    foreach (VHEdge edge in keyValuePair.Value.EnumerateEdges())
                        edge.Origin.Z += heightProfile[i];
                }
            }

            foreach (KeyValuePair<int, VFace> kvp in newCoastline)
                coastline.Remove(kvp.Key);

        }

        private float CalculateRadius(Dictionary<int, VFace> startCells, CartographicIsland cartographicIsland)
        {
            List<float> radii = new List<float>();
            foreach (KeyValuePair<int, VFace> kvp in startCells)
            {
                float x = (float)kvp.Value.Generator.X - cartographicIsland.WeightedCenter.x;
                float z = (float)kvp.Value.Generator.Y - cartographicIsland.WeightedCenter.z;
                float radius = Mathf.Sqrt(x * x + z * z);
                radii.Add(radius);
            }

            return ComputeMax(radii);
        }

        private void SetCoastline(Dictionary<int, VFace> startCells, CartographicIsland island)
        {
            List<VFace> coastline = new List<VFace>();
            Vector3 weightedCenter = Vector3.zero;

            foreach (KeyValuePair<int, VFace> kvp in startCells)
            {
                coastline.Add(kvp.Value);
                float x = (float)kvp.Value.Generator.X;
                float z = (float)kvp.Value.Generator.Y;
                Vector3 tilePos = new Vector3(x, 0, z);
                weightedCenter += tilePos;
            }

            weightedCenter /= startCells.Count;
            island.WeightedCenter = weightedCenter;
            island.CoastlineCells.AddRange(coastline);
        }

        private List<TNetMesh> ConstructTNetMeshFromCellmap(List<VFace> cellmap)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<TNetMesh> result = new List<TNetMesh>();
            TriangleNet.Configuration config = new TriangleNet.Configuration();
            SweepLine sweepline = new SweepLine();

            foreach (VFace face in cellmap)
            {
                vertices.Clear();
                foreach (VHEdge he in face.EnumerateEdges())
                {
                    Vertex v = new Vertex(he.Origin.X, he.Origin.Y);
                    v.Z = he.Origin.Z;
                    if (!vertices.Contains(v))
                        vertices.Add(v);
                }

                TNetMesh tMesh = (TNetMesh)sweepline.Triangulate(vertices, config);
                result.Add(tMesh);
            }

            return result;
        }

        private void LinkDependencyVertex(Bundle bundle, CartographicIsland island)
        {
            var dependencyGraph = bundle.OSGiProject.DependencyGraph;
            List<GraphVertex> allVertices = dependencyGraph.Vertices.ToList();
            GraphVertex vertex = allVertices.Find(v => string.Equals(v.Name, bundle.Name));

            if (vertex != null)
            {
                vertex.Island = island;
                island.DependencyVertex = vertex;
            }
        }

        public bool CheckCell(VFace cell, float minX, float maxX, float minY, float maxY)
        {
            if (cell.Bounded && cell.ID != -1 &&
                cell.Generator.X > minX && cell.Generator.X < maxX &&
                cell.Generator.Y > minY && cell.Generator.Y < maxY)
                return true;
            else
                return false;
        }

        public float ComputeMax(List<float> list)
        {
            float max = Mathf.NegativeInfinity;
            if (list.Count == 0)
                return 0;

            foreach (float value in list)
                if (value > max)
                    max = value;

            return max;
        }
    }
}
