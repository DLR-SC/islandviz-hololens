using HoloIslandVis.Component.UI;
using HoloIslandVis.OSGiParser;
using HoloIslandVis.OSGiParser.Graph;
using HoloIslandVis.Utility;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing.Algorithm;
using TriangleNet.Voronoi;
using UnityEngine;

using TNetFace = TriangleNet.Topology.DCEL.Face;
using TNetMesh = TriangleNet.Mesh;
using TNetHalfEdge = TriangleNet.Topology.DCEL.HalfEdge;
using System;
using TriangleNet;
using System.Linq;

namespace HoloIslandVis.Visualization
{
    internal class IslandGameObjectBuilder
    {
        public delegate void ConstructionCompletedHandler();
        public event ConstructionCompletedHandler ConstructionCompleted = delegate { };

        // TODO: Refactor.
        public const float ISLAND_ABOVE_OCEAN = 3.0f;

        private bool _constructionComplete;
        private List<Island> _islands;

        public bool ConstructionComplete {
            get { return _constructionComplete; }
            private set {
                _constructionComplete = value;
                if (value)
                    ConstructionCompleted();
            }
        }

        private static IslandGameObjectBuilder _instance;
        public static IslandGameObjectBuilder Instance {
            get {
                if (_instance == null)
                    _instance = new IslandGameObjectBuilder();

                return _instance;
            }

            private set { }
        }

        private IslandGameObjectBuilder()
        {
            _islands = new List<Island>();
        }

        public void BuildFromIslandStructures(List<CartographicIsland> _islandStructures)
        {
            foreach (CartographicIsland islandStructure in _islandStructures)
            {
                if (islandStructure.DependencyVertex != null)
                {
                    GameObject islandGameObject = buildFromIslandStructure(islandStructure);
                    _islands.Add(islandGameObject.GetComponent<Island>());
                }
            }

            RuntimeCache.Instance.Islands = _islands;
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building island game objects.";
            ConstructionComplete = true;
        }

        private GameObject buildFromIslandStructure(CartographicIsland islandStructure)
        {
            // TODO: Refactor.
            int randomSeed = islandStructure.Name.GetHashCode() + 200;
            System.Random random = new System.Random(randomSeed);

            GameObject islandGameObject = new GameObject(islandStructure.Name);
            Island islandComponent = islandGameObject.AddComponent<Island>();
            islandGameObject.transform.parent = RuntimeCache.Instance.VisualizationContainer.transform;
            islandComponent.CartographicIsland = islandStructure;
            islandStructure.IslandGameObject = islandGameObject;
            islandGameObject.tag = "Island";

            //islandGameObject.AddComponent<Highlightable>();

            List<List<TNetMesh>> tnetMeshList = islandStructure.getPackageMeshes();
            List<List<TNetFace>> allPackageCells = islandStructure.getPackageCells();
            List<Package> packageList = islandStructure.getPackages();

            // TODO: Refactor.
            float maximumBuildingBoundSize = 0;
            for (int i = 0; i < tnetMeshList.Count; i++)
            {
                Package package = packageList[i];
                GameObject region = new GameObject(package.Name);
                Region regionComponent = region.AddComponent<Region>();
                regionComponent.Island = islandComponent;
                region.transform.parent = islandGameObject.transform;
                islandComponent.Regions.Add(regionComponent);

                GameObject regionArea = new GameObject("Region area");
                regionArea.transform.rotation *= Quaternion.Euler(-90, 0, 0);
                regionArea.transform.SetParent(region.transform);
                MeshFilter meshFilter = regionArea.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = regionArea.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = RuntimeCache.Instance.CombinedHoloMaterial;

                regionComponent.RegionArea = regionArea;
                regionComponent.Package = package;

                List<TNetFace> packageCells = allPackageCells[i];
                CombineInstance[] combinedCellMeshes = new CombineInstance[tnetMeshList[i].Count];

                for (int j = 0; j < tnetMeshList[i].Count; j++)
                {
                    UnityEngine.Mesh mesh = convertTNetMesh(tnetMeshList[i][j]);
                    combinedCellMeshes[j].mesh = mesh;
                }

                meshFilter.mesh = new UnityEngine.Mesh();
                meshFilter.mesh.CombineMeshes(combinedCellMeshes, true, false);

                float randomU = (float)random.NextDouble();
                float randomV = (float)random.NextDouble() * 0.4f;
                Vector2 randomUV = new Vector2(randomU, randomV);
                setUVToSingularCoord(randomUV, meshFilter);

                for (int j = 0; j < package.CompilationUnitCount; j++)
                {
                    CompilationUnit compilationUnit = package.CompilationUnits[j];
                    int heightLevel = mapLinesOfCodeToLevel(compilationUnit.LinesOfCode,
                        package.Bundle.OSGiProject.MaximalLinesOfCode);

                    Building building = getBuildingGameObject(compilationUnit, heightLevel,
                        region.transform);

                    // TODO: Refactor?
                    // TODO: Switch axis!
                    float xPos = (float) packageCells[j].Generator.X;
                    float yPos = (float) packageCells[j].Generator.Y;
                    float zPos = (float) packageCells[j].Generator.Z;
                    building.transform.position = new Vector3(xPos, zPos, -yPos);
                    Vector3 randomRotation = new Vector3(0f, UnityEngine.Random.Range(-180, 180), 0f);
                    building.transform.localEulerAngles = randomRotation;
                    float scale = IslandStructureBuilder.CELL_SCALE_FACTOR * 0.0075f;
                    building.transform.localScale = new Vector3(scale, scale, scale);
                    regionComponent.Buildings.Add(building);

                    BoxCollider boxCollider = building.gameObject.AddComponent<BoxCollider>();
                    boxCollider.isTrigger = true;

                    float buildingExtent = boxCollider.bounds.size.magnitude;
                    if (buildingExtent > maximumBuildingBoundSize)
                        maximumBuildingBoundSize = buildingExtent;
                }
            }

            GameObject coastline = buildIslandCoastline(islandStructure);
            coastline.transform.rotation *= Quaternion.Euler(-90, 0, 0);
            coastline.transform.parent = islandGameObject.transform;
            islandComponent.Coast = coastline;

            GameObject importDock = buildDock(islandStructure, DockType.Import);
            islandComponent.ImportDock = importDock;

            GameObject exportDock = buildDock(islandStructure, DockType.Export);
            islandComponent.ExportDock = exportDock;

            // TODO: Refactor.
            float[] heightProfile = IslandStructureBuilder.HEIGHT_PROFILE;
            Vector3 position = islandStructure.DependencyVertex.Position;
            //Vector3 containerPosition = RuntimeCache.Instance.VisualizationContainer.transform.position;
            //position.y = containerPosition.y - heightProfile[heightProfile.Length - 1];
            //islandGameObject.transform.position = position;
            //islandGameObject.transform.localRotation *= RuntimeCache.Instance.ContentSurface.transform.rotation;

            //Vector3 localIslandPos = islandGameObject.transform.localPosition;
            position.y = Mathf.Abs(heightProfile[heightProfile.Length - 1]) * ISLAND_ABOVE_OCEAN;
            islandGameObject.transform.localRotation *= RuntimeCache.Instance.ContentSurface.transform.rotation;
            islandGameObject.transform.localPosition = position;

            addRegionColliders(islandComponent.Regions);
            addIslandCollider(islandComponent);

            islandGameObject.transform.localScale = Vector3.one;
            return islandGameObject;
        }

        private Building getBuildingGameObject(CompilationUnit compilationUnit, int heightLevel, Transform transform)
        {
            GameObject building;
            if (compilationUnit.IsServiceComponent)
            {
                building = GameObject.Instantiate(RuntimeCache.Instance.SIPrefabs[heightLevel], transform);
                building.AddComponent<ServiceLayer>();
            }
            else if (compilationUnit.IsService)
            {
                building = GameObject.Instantiate(RuntimeCache.Instance.SDPrefabs[heightLevel], transform);
                building.AddComponent<ServiceLayer>();
            }
            else
            {
                building = GameObject.Instantiate(RuntimeCache.Instance.CUPrefabs[heightLevel], transform);
            }

            building.name = compilationUnit.Name;
            Building buildingComponent = building.AddComponent<Building>();
            compilationUnit.GameObject = building;
            buildingComponent.CompilationUnit = compilationUnit;

            return buildingComponent;
        }

        private GameObject buildIslandCoastline(CartographicIsland islandStructure)
        {
            GameObject coastline = new GameObject("coastline");

            MeshFilter meshFilter = coastline.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = coastline.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = RuntimeCache.Instance.CombinedHoloMaterial;
            List<TNetMesh> tnetMeshes = islandStructure.getCoastlineMeshes();
            CombineInstance[] combineInstance = new CombineInstance[tnetMeshes.Count];

            for (int i = 0; i < tnetMeshes.Count; i++)
            {
                UnityEngine.Mesh mesh = convertTNetMesh(tnetMeshes[i]);
                combineInstance[i].mesh = mesh;
            }

            meshFilter.mesh = new UnityEngine.Mesh();
            meshFilter.mesh.CombineMeshes(combineInstance, true, false);
            setUVToSingularCoord(new Vector2(0f, 0.7f), meshFilter);

            return coastline;
        }

        private GameObject buildDock(CartographicIsland islandStructure, DockType dockType)
        {
            GameObject dock = GameObject.Instantiate(RuntimeCache.Instance.DockPrefabs[dockType], Vector3.zero, Quaternion.identity);
            dock.transform.parent = islandStructure.IslandGameObject.transform;

            float[] heightProfile = IslandStructureBuilder.HEIGHT_PROFILE;
            Vector3 dockDirection = new Vector3(UnityEngine.Random.value, 0, UnityEngine.Random.value);
            dockDirection.Normalize();
            dockDirection *= islandStructure.getRadius();
            Vector3 dockPos = islandStructure.getWeightedCenter() + dockDirection;
            dockPos.y -= 2.0f;

            dock.transform.localPosition = dockPos;
            dock.name = islandStructure.Name + " " + Enum.GetName(typeof(DockType), dockType);
            dock.transform.localScale = Vector3.one;

            return dock;
        }

        private void addRegionColliders(List<Region> regions)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                Region region = regions[i];
                MeshCollider meshCollider = region.gameObject.AddComponent<MeshCollider>();
                MeshFilter meshFilter = region.RegionArea.GetComponent<MeshFilter>();

                meshCollider.sharedMesh = meshFilter.sharedMesh;
                meshCollider.convex = true;
                meshCollider.isTrigger = true;
            }
        }

        private void addIslandCollider(Island islandComponent)
        {

            CapsuleCollider capsuleCollider = islandComponent.gameObject.AddComponent<CapsuleCollider>();
            MeshFilter coastMeshFilter = islandComponent.Coast.GetComponent<MeshFilter>();
            capsuleCollider.radius = islandComponent.CartographicIsland.getRadius();
            capsuleCollider.height = coastMeshFilter.sharedMesh.bounds.size.y;

            Vector3 weightedCenter = islandComponent.CartographicIsland.getWeightedCenter();
            weightedCenter.y = -coastMeshFilter.sharedMesh.bounds.size.y + capsuleCollider.height * 0.5f;
            capsuleCollider.center = weightedCenter;
            capsuleCollider.isTrigger = true;
        }

        // TODO: Refactor.
        public UnityEngine.Mesh convertTNetMesh(TriangleNet.Meshing.IMesh tMesh)
        {
            List<Vector3> outVertices = new List<Vector3>();
            List<int> outIndices = new List<int>();

            foreach (ITriangle t in tMesh.Triangles)
            {
                for (int j = 2; j >= 0; j--)
                {
                    bool found = false;
                    for (int k = 0; k < outVertices.Count; k++)
                    {
                        if ((outVertices[k].x == t.GetVertex(j).X) && (outVertices[k].z == t.GetVertex(j).Y) && (outVertices[k].y == t.GetVertex(j).Z))
                        {
                            outIndices.Add(k);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        outVertices.Add(new Vector3((float)t.GetVertex(j).X, (float)t.GetVertex(j).Y, (float)t.GetVertex(j).Z));
                        outIndices.Add(outVertices.Count - 1);
                    }
                }
            }
            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < outVertices.Count; i++)
                uvs.Add(new Vector2(outVertices[i].x, outVertices[i].z));

            UnityEngine.Mesh resultMesh = new UnityEngine.Mesh();
            resultMesh.SetVertices(outVertices);
            outIndices.Reverse();
            resultMesh.SetTriangles(outIndices, 0);
            resultMesh.SetUVs(0, uvs);
            resultMesh.RecalculateBounds();
            resultMesh.RecalculateNormals();
            return resultMesh;
        }

        // TODO: Refactor.
        private void setUVToSingularCoord(Vector2 newUV, MeshFilter mesh)
        {
            Vector2[] uvs = mesh.sharedMesh.uv;
            Vector2[] newUVs = new Vector2[uvs.Length];
            for (int i = 0; i < uvs.Length; i++)
                newUVs[i] = newUV;

            mesh.sharedMesh.uv = newUVs;
        }

        // TODO: Refactor.
        // Change with different Metric
        public static int mapLinesOfCodeToLevel(long linesOfCode, long maxLinesOfCode)
        {
            int buildingLevels = RuntimeCache.Instance.NumBuildingLevels;
            float mappingValue = Mathf.Sqrt((float)linesOfCode);
            float maxLocMapped = Mathf.Sqrt((float)maxLinesOfCode);

            float segment = maxLocMapped / RuntimeCache.Instance.NumBuildingLevels;
            int level = Mathf.Min(Mathf.FloorToInt(mappingValue / segment), buildingLevels - 1);

            return level;
        }
    }
}