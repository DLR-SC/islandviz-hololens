using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Voronoi;
using QuickGraph;
using System.Linq;
using HoloIslandVis.OSGiParser;
using HoloIslandVis.OSGiParser.Graph;

using VFace = TriangleNet.Topology.DCEL.Face;
using TnetMesh = TriangleNet.Mesh;
using VHEdge = TriangleNet.Topology.DCEL.HalfEdge;
using VVertex = TriangleNet.Topology.DCEL.Vertex;

namespace HoloIslandVis.Visualization
{
    public class IslandStructureBuilder
    {
        public const float CELL_SCALE_FACTOR = 20f;
        public static float[] HEIGHT_PROFILE = { -0.03f * CELL_SCALE_FACTOR, -0.034f * CELL_SCALE_FACTOR,
        -0.036f * CELL_SCALE_FACTOR, -0.037f * CELL_SCALE_FACTOR, -0.0375f * CELL_SCALE_FACTOR };

        private static IslandStructureBuilder _instance;
        public static IslandStructureBuilder Instance {
            get {
                if (_instance == null)
                    _instance = new IslandStructureBuilder(1, 2, 8);

                return _instance;
            }

            private set { }
        }

        private OSGiProject _osgiProject;
        private List<CartographicIsland> islands;
        private System.Random RNG;
        private int expansionFactor;
        private float minCohesion;
        private float maxCohesion;

        private IslandStructureBuilder(int expansionF, float minCoh, float maxCoh)
        {
            _osgiProject = null;
            islands = new List<CartographicIsland>();
            RNG = new System.Random(0);
            expansionFactor = expansionF;
            minCohesion = minCoh;
            maxCohesion = maxCoh;
        }

        //Public method to construct IslandStructures from an OsgiProject in a separate thread
        public void Construct(OSGiProject osgiProject)
        {
            _osgiProject = osgiProject;
            //ConstructIslands();
        }

        public List<CartographicIsland> BuildFromProject(OSGiProject osgiProject)
        {
            foreach (Bundle bundle in osgiProject.Bundles)
                islands.Add(buildFromBundle(bundle));

            return islands;
        }


        private CartographicIsland buildFromBundle(Bundle b)
        {

            int rngSeed = b.Name.GetHashCode() + 200;
            RNG = new System.Random(rngSeed);

            #region VoronoiPlane 
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            List<Vertex> vertices = createPointsOnPlane(CELL_SCALE_FACTOR, CELL_SCALE_FACTOR, 50, 50, 1.0f, RNG);
            BoundedVoronoi voronoi = createRelaxedVoronoi(vertices, 1);
            #endregion

            #region initFirstCell
            VFace firstCell = closestCell(0, 0, voronoi);
            Dictionary<int, VFace> startingCandidates = new Dictionary<int, VFace>();
            startingCandidates.Add(firstCell.ID, firstCell);
            #endregion

            List<Package> packages = b.Packages;
            CartographicIsland island = new CartographicIsland(b, voronoi);

            //Compute maximal compilation unit count in bundle
            float maxCUCountInIsland = 0;
            foreach (Package package in packages)
            {
                long cuCount = package.CompilationUnitCount;
                if (cuCount > maxCUCountInIsland)
                    maxCUCountInIsland = cuCount;
            }
            #region construct regions
            foreach (Package package in packages)
            {
                float cohesionMult = (float)package.CompilationUnitCount / maxCUCountInIsland;
                cohesionMult *= maxCohesion;
                cohesionMult = Mathf.Max(minCohesion, cohesionMult);
                Dictionary<int, VFace> newCandidates = constructRegionFromPackage(package, island, startingCandidates, cohesionMult);
                updateAndFuseCandidates(startingCandidates, newCandidates);
            }
            #endregion


            #region Shape island coast
            //Advance startingCandidates X cells outwards and ajdust the height of all vertices
            shapeCoastArea(startingCandidates, HEIGHT_PROFILE);
            #endregion

            #region WeightedCenter & set coast
            List<VFace> coastlineList = new List<VFace>();
            Vector3 weightedCenter = Vector3.zero;
            foreach (KeyValuePair<int, VFace> kvp in startingCandidates)
            {
                coastlineList.Add(kvp.Value);
                float x = (float)kvp.Value.Generator.X;
                float z = (float)kvp.Value.Generator.Y;
                Vector3 tilePos = new Vector3(x, 0, z);
                weightedCenter += tilePos;
            }
            weightedCenter /= startingCandidates.Count;
            island.setWeightedCenter(weightedCenter);
            island.setCoastlineCells(coastlineList);
            #endregion

            #region conservative Radius
            List<float> radii = new List<float>();
            foreach (KeyValuePair<int, VFace> kvp in startingCandidates)
            {
                float x = (float)kvp.Value.Generator.X - island.getWeightedCenter().x;
                float z = (float)kvp.Value.Generator.Y - island.getWeightedCenter().z;
                float radius = Mathf.Sqrt(x * x + z * z);
                radii.Add(radius);
            }
            float maxRadius = computeMax(radii);
            island.setRadius(maxRadius);
            #endregion

            #region TnetMeshesConstruction
            foreach (List<VFace> cellMap in island.getPackageCells())
                island.addPackageMesh(constructTnetMeshFromCellmap(cellMap));

            island.setCoastlineMesh(constructTnetMeshFromCellmap(coastlineList));
            #endregion

            #region link dependency vertex

            //Find graph vertex associated with the island
            BidirectionalGraph<GraphVertex, GraphEdge> depGraph = b.OSGiProject.DependencyGraph;
            List<GraphVertex> allVertices = depGraph.Vertices.ToList();
            GraphVertex vert = allVertices.Find(v => string.Equals(v.Name, b.Name));
            if (vert != null)
            {
                //Link GraphVertex-Island
                vert.Island = island;
                island.DependencyVertex = vert;
            }

            #endregion

            return island;
        }

        //Update dictA and fuse dictB into it
        private void updateAndFuseCandidates(Dictionary<int, VFace> dictA, Dictionary<int, VFace> dictB)
        {
            //update dictA
            List<int> keysToRemove = new List<int>();
            foreach (KeyValuePair<int, VFace> kvp in dictA)
                if (kvp.Value.Mark != 0)
                    keysToRemove.Add(kvp.Key);
            foreach (int key in keysToRemove)
                dictA.Remove(key);


            //Fuse dictB into dictA
            foreach (KeyValuePair<int, VFace> kvp in dictB)
            {
                if (!dictA.ContainsKey(kvp.Key))
                    dictA.Add(kvp.Key, kvp.Value);
            }

        }

        //return: the unused candidates
        //b[1, 10]: cohesion factor. higher b -> more compact "cohesive" islands
        private Dictionary<int, VFace> constructRegionFromPackage(Package package, CartographicIsland island, Dictionary<int, VFace> startingCandidates, float b)
        {
            BoundedVoronoi islandVoronoi = island.Voronoi;
            List<CompilationUnit> cuList = package.CompilationUnits;
            List<VFace> cellMap = new List<VFace>();
            Dictionary<int, VFace> newCandidates = new Dictionary<int, VFace>();
            VFace startingCell = selectFromCandidates(startingCandidates, b).Value;

            int maxIterations = 10;
            int counter = 0;
            while (expandCountries(cuList, cellMap, newCandidates, startingCell, b) == false && counter < maxIterations)
            {
                //Debug.Log("Backtracking");
                startingCell = selectFromCandidates(startingCandidates, b).Value;
                counter++;
            }

            island.addPackage(package);
            island.addPackageCells(cellMap);
            return newCandidates;
        }

        //writes into cellMap and endCandidates
        private bool expandCountries(List<CompilationUnit> cuList, List<VFace> cellMap, Dictionary<int, VFace> endCandidates, VFace startingCell, float b)
        {
            Dictionary<int, VFace> candidates = new Dictionary<int, VFace>();
            candidates.Add(startingCell.ID, startingCell);

            for (int i = 1; i < cuList.Count * expansionFactor + 1; i++)
            {

                if (candidates.Count == 0)
                {
                    cellMap.Clear();
                    endCandidates.Clear();
                    return false;
                }

                //Select cell from candidates
                KeyValuePair<int, VFace> selectedCell = selectFromCandidates(candidates, b);
                //Mark cell in islandVoronoi
                selectedCell.Value.Mark = i;
                //Add cell to package dictionary
                cellMap.Add(selectedCell.Value);
                //Remove cell from future candidates list
                candidates.Remove(selectedCell.Key);
                //Add viable candidates around cell

                foreach (VHEdge edge in selectedCell.Value.EnumerateEdges())
                {
                    VFace nCell = edge.Twin.Face;
                    //If cell is OK, not occupied and not already in candidateList, add to candidate list
                    if (checkCell(nCell, -CELL_SCALE_FACTOR * 0.4f, CELL_SCALE_FACTOR * 0.4f,
                        -CELL_SCALE_FACTOR * 0.4f, CELL_SCALE_FACTOR * 0.4f) && nCell.Mark == 0 && !candidates.ContainsKey(nCell.ID))
                        candidates.Add(nCell.ID, nCell);
                }

            }

            //transfer candidates into endCandidates Dict
            foreach (KeyValuePair<int, VFace> kvp in candidates)
                endCandidates.Add(kvp.Key, kvp.Value);

            return true;
        }


        private List<TnetMesh> constructTnetMeshFromCellmap(List<VFace> cellmap)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<TnetMesh> result = new List<TnetMesh>();
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            SweepLine sl = new SweepLine();

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
                TriangleNet.Mesh tMesh = (TriangleNet.Mesh)sl.Triangulate(vertices, conf);
                result.Add(tMesh);
            }

            return result;
        }

        private KeyValuePair<int, VFace> selectFromCandidates(Dictionary<int, VFace> candidates, float b)
        {
            KeyValuePair<int, VFace> selectedCandidate = new KeyValuePair<int, VFace>();
            List<float> candidateScores = new List<float>();
            float totalScore = 0f;

            //Fill CandidateScores
            int counter = 0;
            foreach (KeyValuePair<int, VFace> kvp in candidates)
            {
                int n = 0;
                foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                {
                    int mark = edge.Twin.Face.Mark;
                    if (mark != 0)
                        n++;
                }

                float score = Mathf.Pow(b, n);
                candidateScores.Add(score);
                totalScore += score;
                counter++;
            }
            //Normalize CandidateScores
            for (int i = 0; i < candidateScores.Count; i++)
                candidateScores[i] = candidateScores[i] / totalScore;

            //Select candidate based on probability
            float rndNumber = (float)RNG.NextDouble();
            counter = 0;
            foreach (KeyValuePair<int, VFace> kvp in candidates)
            {
                rndNumber -= candidateScores[counter];
                if (rndNumber <= 0f)
                {
                    selectedCandidate = kvp;
                    break;
                }
                counter++;
            }

            return selectedCandidate;
        }

        /*
        private void populateIslandCoastDistance(CartographicIsland island, int hf)
        {
            //Init land height to Inf
            List<List<VFace>> fragCellList = island.getPackageCells();
            foreach (List<VFace> cellMap in fragCellList)
                foreach (VFace face in cellMap)
                    face.coastDistance = int.MaxValue;


            //Enqueue the coast region first
            Queue<VFace> cellQueue = new Queue<VFace>();
            foreach (VFace face in island.getCoastlineCells())
            {
                face.coastDistance = 0;
                cellQueue.Enqueue(face);
            }

            while (cellQueue.Count > 0)
            {
                VFace v = cellQueue.Dequeue();
                foreach (VHEdge edge in v.EnumerateEdges())
                {
                    VFace adjFace = edge.twin.face;
                    int newDistance = v.coastDistance + hf;
                    if (newDistance < adjFace.coastDistance)
                    {
                        adjFace.coastDistance = newDistance;
                        cellQueue.Enqueue(adjFace);
                    }
                }
            }
        }
        */

        /*
        private void computeIslandHeight(CartographicIsland island, float hf)
        {
            //Init land height to Inf
            List<List<VFace>> fragCellList = island.getPackageCells();
            foreach(List<VFace> cellMap in fragCellList)
                foreach (VFace face in cellMap)
                    foreach (VHEdge edge in face.EnumerateEdges())
                        edge.Origin.Z = Mathf.Infinity;

            //Enqueue the coast region first
            Queue<VVertex> vertexQueue = new Queue<VVertex>();
            foreach (VFace face in island.getCoastlineCells())
            {
                foreach (VHEdge edge in face.EnumerateEdges())
                    vertexQueue.Enqueue(edge.Origin);
            }

            while (vertexQueue.Count > 0)
            {
                VVertex v = vertexQueue.Dequeue();
                foreach (VHEdge edge in v.EnumerateEdges())
                {
                    VVertex adjVert = edge.Next.Origin;
                    float newElevation = (float)v.Z + hf;
                    if (newElevation < adjVert.Z)
                    {
                        adjVert.Z = newElevation;
                        vertexQueue.Enqueue(adjVert);
                    }
                }
            }

        }
        */

        //Expands the coastlineCells outwards by hp.length cells and applies the height profile hp during expansion
        private void shapeCoastArea(Dictionary<int, VFace> coastline, float[] hp)
        {
            Dictionary<int, VFace> newestCoastline = new Dictionary<int, VFace>(coastline);

            for (int i = 0; i < hp.Length; i++)
            {
                //Expand cells
                newestCoastline.Clear();
                foreach (KeyValuePair<int, VFace> kvp in coastline)
                {
                    foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                    {
                        VFace nCell = edge.Twin.Face;
                        if (checkCell(nCell, -CELL_SCALE_FACTOR * 0.4f, CELL_SCALE_FACTOR * 0.4f,
                            -CELL_SCALE_FACTOR * 0.4f, CELL_SCALE_FACTOR * 0.4f) && nCell.Mark == 0 && !coastline.ContainsKey(nCell.ID)
                            && !newestCoastline.ContainsKey(nCell.ID))
                            newestCoastline.Add(nCell.ID, nCell);
                    }
                }
                //Adjust height and Add to coastline
                foreach (KeyValuePair<int, VFace> kvp in newestCoastline)
                {
                    coastline.Add(kvp.Key, kvp.Value);
                    foreach (VHEdge edge in kvp.Value.EnumerateEdges())
                    {
                        edge.Origin.Z += hp[i];
                    }
                }

            }
            //Remove the last expansion from the coastline, due to artifacts
            foreach (KeyValuePair<int, VFace> kvp in newestCoastline)
                coastline.Remove(kvp.Key);

        }

        public List<CartographicIsland> getIslandStructureList()
        {
            return islands;
        }

        public List<Vertex> createPointsOnPlane(float widthScale, float heightScale, int widthSegments, int heightSegments, float preturbFactor, System.Random RNG)
        {

            float preturbFactorX = preturbFactor * (widthScale / widthSegments);
            float preturbFactorY = preturbFactor * (heightScale / heightSegments);
            int hCount2 = widthSegments + 1;
            int vCount2 = heightSegments + 1;
            int numTriangles = widthSegments * heightSegments * 6;
            int numVertices = hCount2 * vCount2;

            List<Vertex> vertices = new List<Vertex>();

            int index = 0;
            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / heightSegments;
            float scaleX = widthScale / widthSegments;
            float scaleY = heightScale / heightSegments;

            for (int y = 0; y < vCount2; y++)
            {
                for (int x = 0; x < hCount2; x++)
                {

                    Vertex newVert = new Vertex(x * scaleX - widthScale / 2f, y * scaleY - heightScale / 2f);
                    newVert.X += (float)RNG.NextDouble() * preturbFactorX;
                    newVert.Y += (float)RNG.NextDouble() * preturbFactorY;
                    vertices.Add(newVert);
                }
            }

            return vertices;
        }

        public BoundedVoronoi createRelaxedVoronoi(List<Vertex> startingVertices, int numLloydRelaxations)
        {
            TriangleNet.Configuration conf = new TriangleNet.Configuration();
            SweepLine sl = new SweepLine();
            List<Vertex> vertices = new List<Vertex>();

            TnetMesh tMesh = (TnetMesh)sl.Triangulate(startingVertices, conf);
            BoundedVoronoi voronoi = new BoundedVoronoi(tMesh);

            for (int i = 0; i < numLloydRelaxations; i++)
            {
                foreach (VFace face in voronoi.Faces)
                {
                    if (checkCell(face))
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

                tMesh = (TnetMesh)sl.Triangulate(vertices, conf);
                voronoi = new BoundedVoronoi(tMesh);
                vertices.Clear();
            }

            return voronoi;
        }

        //Rejects unbounded cells and "infinite" cells
        public bool checkCell(VFace cell)
        {
            if (cell.Bounded && cell.ID != -1)
                return true;
            else
                return false;
        }

        //Rejects unbounded cells, "infinite" cells and cells outside a certain boundary
        public bool checkCell(VFace cell, float minX, float maxX, float minY, float maxY)
        {
            if (cell.Bounded && cell.ID != -1 &&
                cell.Generator.X > minX && cell.Generator.X < maxX &&
                cell.Generator.Y > minY && cell.Generator.Y < maxY)
                return true;
            else
                return false;
        }

        //TODO: Unbounded problem could manifest itself here
        public VFace closestCell(float x, float y, BoundedVoronoi voronoi)
        {
            int startingIndex = voronoi.Faces.Count / 2;
            VFace currentCell = voronoi.Faces[startingIndex];
            float currentDistance = Mathf.Sqrt((float)(currentCell.Generator.X - x) * (float)(currentCell.Generator.X - x) + (float)(currentCell.Generator.Y - y) * (float)(currentCell.Generator.Y - y));

            while (true)
            {
                TriangleNet.Topology.DCEL.Face nextCell = null;
                bool foundNeighbour = false;
                foreach (TriangleNet.Topology.DCEL.HalfEdge edge in currentCell.EnumerateEdges())
                {
                    VFace nCell = edge.Twin.Face;
                    float neighbourX = (float)nCell.Generator.X;
                    float neighbourY = (float)nCell.Generator.Y;
                    float distanceFromNeighbour = Mathf.Sqrt((neighbourX - x) * (neighbourX - x) + (neighbourY - y) * (neighbourY - y));
                    if (distanceFromNeighbour < currentDistance)
                    {
                        foundNeighbour = true;
                        currentDistance = distanceFromNeighbour;
                        nextCell = nCell;
                    }
                }

                if (!foundNeighbour)
                    break;

                currentCell = nextCell;
            }

            return currentCell;
        }

        public float computeMax(List<float> list)
        {
            float max = Mathf.NegativeInfinity;
            if (list.Count == 0)
                return 0;
            foreach (float value in list)
                if (value > max)
                    max = value;

            return max;
        }

        public long computeMax(List<long> list)
        {
            long max = long.MinValue;
            if (list.Count == 0)
                return 0;
            foreach (long value in list)
                if (value > max)
                    max = value;

            return max;
        }
    }
}