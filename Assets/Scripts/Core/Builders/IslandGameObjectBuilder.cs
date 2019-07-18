using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Interaction;
using HoloIslandVis.Model.OSGi;
using HoloIslandVis.Model.OSGi.Services;
using HoloIslandVis.Sharing;
using HoloIslandVis.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology.DCEL;
using UnityEngine;

using TNetFace = TriangleNet.Topology.DCEL.Face;
using TNetMesh = TriangleNet.Mesh;

namespace HoloIslandVis.Core.Builders
{
    public class IslandGameObjectBuilder : SingletonComponent<IslandGameObjectBuilder>
    {
        public delegate void IslandGameObjectsBuiltHandler(IslandGameObjectsBuiltEventArgs eventArgs);
        public event IslandGameObjectsBuiltHandler IslandGameObjectsBuilt = delegate { };

        private System.Random _random;

        // Use this for initialization
        void Start()
        {

        }

        public IEnumerator BuildFromIslandStructures(List<CartographicIsland> _islandStructures)
        {
            List<Island> islands = new List<Island>();

            foreach (CartographicIsland islandStructure in _islandStructures)
            {
                if (islandStructure.DependencyVertex != null)
                {
                    GameObject islandGameObject = new GameObject();
                    yield return BuildFromIslandStructure(islandStructure, islandGameObject);
                    islands.Add(islandGameObject.GetComponent<Island>());
                }
            }

            var eventArgs = new IslandGameObjectsBuiltEventArgs(islands);
            IslandGameObjectsBuilt(eventArgs);
        }

        public IEnumerator BuildFromIslandStructure(CartographicIsland islandStructure, GameObject islandGameObject)
        {
            int randomSeed = islandStructure.Name.GetHashCode() + 200;
            _random = new System.Random(randomSeed);

            islandGameObject.name = islandStructure.Name;
            Island islandComponent = islandGameObject.AddComponent<Island>();
            Interactable interactable = islandGameObject.AddComponent<Interactable>();
            islandGameObject.transform.parent = UIManager.Instance.BundleContainer.transform;
            islandComponent.CartographicIsland = islandStructure;
            islandStructure.IslandGameObject = islandGameObject;
            interactable.Type = InteractableType.Bundle;
            interactable.Focusable = true;
            interactable.Selectable = true;

            yield return BuildRegionAreas(islandComponent);
            yield return BuildIslandCoastline(islandComponent);
            yield return BuildDock(islandComponent, DockType.Import);
            yield return BuildDock(islandComponent, DockType.Export);

            SetLayerRecursively(islandGameObject, LayerMask.NameToLayer("Ignore Raycast"));
            AddRegionColliders(islandComponent.Regions);
            SetIslandPosition(islandComponent);
            AddIslandCollider(islandComponent);

            UIManager.Instance.Visualization.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        private IEnumerator BuildRegionAreas(Island islandComponent)
        {
            var islandStructure = islandComponent.CartographicIsland;
            var islandGameObject = islandStructure.IslandGameObject;

            List<List<TNetMesh>> tnetMeshList = islandComponent.CartographicIsland.PackageMeshes;
            List<List<TNetFace>> allPackageCells = islandComponent.CartographicIsland.PackageCells;
            List<Package> packages = islandComponent.CartographicIsland.Packages;

            for (int i = 0; i < tnetMeshList.Count; i++)
            {
                Package package = packages[i];
                GameObject region = new GameObject(package.Name);
                Region regionComponent = region.AddComponent<Region>();
                Interactable interactable = region.AddComponent<Interactable>();
                interactable.Type = InteractableType.Package;
                interactable.Focusable = true;
                interactable.Selectable = true;

                islandComponent.Regions.Add(regionComponent);
                regionComponent.Island = islandComponent;
                region.transform.parent = islandGameObject.transform;

                GameObject regionArea = new GameObject("Region Area");
                yield return BuildRegionArea(regionArea, allPackageCells[i], tnetMeshList[i]);
                regionArea.transform.SetParent(region.transform);

                regionComponent.RegionArea = regionArea;
                regionComponent.Package = package;

                yield return CreateBuildings(regionComponent, allPackageCells[i]);
            }

            yield return null;
        }

        private IEnumerator BuildRegionArea(GameObject regionArea, List<TNetFace> packageCells, List<TNetMesh> packageMesh)
        {
            MeshFilter meshFilter = regionArea.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = regionArea.AddComponent<MeshRenderer>();
            CombineInstance[] combinedMeshes = new CombineInstance[packageMesh.Count];

            for (int i = 0; i < packageMesh.Count; i++)
            {
                Matrix4x4 matrix = meshFilter.transform.localToWorldMatrix;
                combinedMeshes[i].transform = matrix * Matrix4x4.Rotate(Quaternion.Euler(-90, 0, 0));
                UnityEngine.Mesh mesh = ConvertTNetMesh(packageMesh[i]);
                combinedMeshes[i].mesh = mesh;
            }

            meshFilter.mesh = new UnityEngine.Mesh();
            meshFilter.mesh.CombineMeshes(combinedMeshes, true, true);
            meshRenderer.sharedMaterial = VisualizationLoader.Instance.DefaultMaterial;

            float randomU = SyncDataStorage.Instance.GetRandomNumber(_random);
            float randomV = SyncDataStorage.Instance.GetRandomNumber(_random) * 0.4f;
            Vector2 randomUV = new Vector2(randomU, randomV);
            SetUVToSingularCoord(randomUV, meshFilter);

            yield return null;
        }

        private IEnumerator CreateBuildings(Region region, List<TNetFace> packageCells)
        {
            float maximumBuildingBoundSize = 0;
            for (int i = 0; i < region.Package.CompilationUnitCount; i++)
            {
                CompilationUnit compilationUnit = region.Package.CompilationUnits[i];
                long maxLinesOfCode = region.Package.Bundle.OSGiProject.MaximalLinesOfCode;
                int heightLevel = MapLinesOfCodeToLevel(compilationUnit.LinesOfCode, maxLinesOfCode);

                Building building = CreateBuilding(compilationUnit, heightLevel, region.transform);
                building.Region = region;

                float xPos = (float)packageCells[i].Generator.X;
                float yPos = (float)packageCells[i].Generator.Y;
                float zPos = (float)packageCells[i].Generator.Z;
                building.transform.localPosition = new Vector3(xPos, zPos, -yPos);

                float randomRange = UnityEngine.Random.Range(-180, 180);
                Vector3 randomRotation = new Vector3(0f, randomRange, 0f);
                building.transform.localEulerAngles = randomRotation;

                float scale = IslandStructureBuilder.CELL_SCALE * 0.0075f;
                building.transform.localScale = new Vector3(scale, scale, scale);
                building.transform.parent = region.RegionArea.transform;
                region.Buildings.Add(building);

                BoxCollider boxCollider = building.gameObject.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                boxCollider.enabled = false;

                float buildingExtent = boxCollider.bounds.size.magnitude;
                if (buildingExtent > maximumBuildingBoundSize)
                    maximumBuildingBoundSize = buildingExtent;

                yield return null;
            }
        }

        private Building CreateBuilding(CompilationUnit compilationUnit, int heightLevel, Transform transform)
        {
            GameObject buildingGameObject = null;

            if (compilationUnit.IsServiceComponent)
            {
                buildingGameObject = Instantiate(VisualizationLoader.Instance.SIPrefabs[heightLevel], transform);
                buildingGameObject.AddComponent<ServiceLayerGO>();
            }
            else if (compilationUnit.IsService)
            {
                buildingGameObject = Instantiate(VisualizationLoader.Instance.SDPrefabs[heightLevel], transform);
                buildingGameObject.AddComponent<ServiceLayerGO>();
            }
            else
            {
                buildingGameObject = Instantiate(VisualizationLoader.Instance.CUPrefabs[heightLevel], transform);
            }

            Building building = buildingGameObject.AddComponent<Building>();
            Interactable interactable = buildingGameObject.AddComponent<Interactable>();
            interactable.Type = InteractableType.CompilationUnit;
            interactable.Focusable = true;
            interactable.Selectable = true;

            buildingGameObject.name = compilationUnit.Name;
            compilationUnit.GameObject = buildingGameObject;
            building.CompilationUnit = compilationUnit;

            return building;
        }

        public UnityEngine.Mesh ConvertTNetMesh(TriangleNet.Meshing.IMesh tnetMesh)
        {
            List<Vector3> outVertices = new List<Vector3>();
            List<int> outIndices = new List<int>();

            foreach (ITriangle t in tnetMesh.Triangles)
            {
                for (int i = 2; i >= 0; i--)
                {
                    bool found = false;

                    for (int j = 0; j < outVertices.Count; j++)
                    {
                        if ((outVertices[j].x == t.GetVertex(i).X) && 
                            (outVertices[j].z == t.GetVertex(i).Y) && 
                            (outVertices[j].y == t.GetVertex(i).Z))
                        {
                            outIndices.Add(j);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        float x = (float)t.GetVertex(i).X;
                        float y = (float)t.GetVertex(i).Y;
                        float z = (float)t.GetVertex(i).Z;

                        outVertices.Add(new Vector3(x, y, z));
                        outIndices.Add(outVertices.Count - 1);
                    }
                }
            }

            List<Vector2> uv = new List<Vector2>();
            for (int i = 0; i < outVertices.Count; i++)
                uv.Add(new Vector2(outVertices[i].x, outVertices[i].z));

            UnityEngine.Mesh result = new UnityEngine.Mesh();

            result.SetVertices(outVertices);
            outIndices.Reverse();

            result.SetTriangles(outIndices, 0);
            result.SetUVs(0, uv);
            result.RecalculateBounds();
            result.RecalculateNormals();

            return result;
        }

        private void SetUVToSingularCoord(Vector2 randomuv, MeshFilter mesh)
        {
            Vector2[] uv = mesh.sharedMesh.uv;
            Vector2[] newUV = new Vector2[uv.Length];

            for (int i = 0; i < uv.Length; i++)
                newUV[i] = randomuv;

            mesh.sharedMesh.uv = newUV;
        }


        public int MapLinesOfCodeToLevel(long linesOfCode, long maxLinesOfCode)
        {
            int buildingLevels = 8; // Number of building levels.
            float mappingValue = Mathf.Sqrt(linesOfCode);
            float maxLocMapped = Mathf.Sqrt(maxLinesOfCode);

            float segment = maxLocMapped / buildingLevels;
            int floored = Mathf.FloorToInt(mappingValue / segment);
            int level = Mathf.Min(floored, buildingLevels - 1);

            return level;
        }

        private IEnumerator BuildIslandCoastline(Island islandComponent)
        {
            var islandStructure = islandComponent.CartographicIsland;
            GameObject coastline = new GameObject("coastline");
            coastline.transform.parent = islandStructure.IslandGameObject.transform;
            islandComponent.Coast = coastline;

            MeshFilter meshFilter = coastline.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = coastline.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = VisualizationLoader.Instance.DefaultMaterial;

            List<TNetMesh> tnetMeshes = islandStructure.CoastlineMeshes;
            CombineInstance[] combineInstance = new CombineInstance[tnetMeshes.Count];

            for (int i = 0; i < tnetMeshes.Count; i++)
            {
                UnityEngine.Mesh mesh = ConvertTNetMesh(tnetMeshes[i]);
                Matrix4x4 matrix = meshFilter.transform.localToWorldMatrix;
                combineInstance[i].transform = matrix * Matrix4x4.Rotate(Quaternion.Euler(-90, 0, 0));
                combineInstance[i].mesh = mesh;
            }

            meshFilter.mesh = new UnityEngine.Mesh();
            meshFilter.mesh.CombineMeshes(combineInstance, true, true);
            SetUVToSingularCoord(new Vector2(0f, 0.7f), meshFilter);

            coastline.transform.parent = islandStructure.IslandGameObject.transform;
            islandComponent.Coast = coastline;

            yield return null;
        }

        private IEnumerator BuildDock(Island islandComponent, DockType dockType)
        {
            var islandStructure = islandComponent.CartographicIsland;
            var dockPrefabs = VisualizationLoader.Instance.DockPrefabs;
            GameObject dock = Instantiate(dockPrefabs[dockType], Vector3.zero, Quaternion.identity);
            dock.transform.parent = islandStructure.IslandGameObject.transform;

            float[] heightProfile = IslandStructureBuilder.HEIGHT_PROFILE;
            Vector3 dockDirection = new Vector3(UnityEngine.Random.value, 0, UnityEngine.Random.value);

            dockDirection.Normalize();
            dockDirection *= islandStructure.Radius;
            Vector3 dockPos = islandStructure.WeightedCenter + dockDirection;
            dockPos.y -= 2.0f;

            dock.transform.localPosition = dockPos;
            dock.name = islandStructure.Name + " " + Enum.GetName(typeof(DockType), dockType);
            dock.transform.localScale = Vector3.one;

            if (dockType == DockType.Import) islandComponent.ImportDock = dock;
            else islandComponent.ExportDock = dock;

            yield return null;
        }

        private void AddRegionColliders(List<Region> regions)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                Region region = regions[i];
                region.RegionLevelCollider = region.gameObject.AddComponent<MeshCollider>();
                region.BuildingLevelCollider = region.gameObject.AddComponent<MeshCollider>();
                MeshFilter meshFilter = region.RegionArea.GetComponent<MeshFilter>();

                MeshFilter[] meshFilters = region.RegionArea.GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combineInstance = new CombineInstance[meshFilters.Length];

                for (int j = 0; j < meshFilters.Length; j++)
                {
                    combineInstance[j].transform = meshFilters[j].transform.localToWorldMatrix;
                    combineInstance[j].mesh = meshFilters[j].mesh;
                }

                UnityEngine.Mesh regionLevelMesh = new UnityEngine.Mesh();
                regionLevelMesh.CombineMeshes(combineInstance, true, true);

                region.RegionLevelCollider.sharedMesh = regionLevelMesh;
                region.BuildingLevelCollider.sharedMesh = meshFilter.sharedMesh;

                region.RegionLevelCollider.enabled = false;
                region.BuildingLevelCollider.enabled = false;
            }
        }

        private void AddIslandCollider(Island islandComponent)
        {
            CapsuleCollider islandLevelCollider = islandComponent.gameObject.AddComponent<CapsuleCollider>();
            MeshFilter coastMeshFilter = islandComponent.Coast.GetComponent<MeshFilter>();
            islandLevelCollider.radius = islandComponent.CartographicIsland.Radius;
            islandLevelCollider.height = coastMeshFilter.sharedMesh.bounds.size.y;

            //Vector3 weightedCenter = islandComponent.CartographicIsland.WeightedCenter;
            Vector3 weightedCenter = islandLevelCollider.center;
            weightedCenter.y = -coastMeshFilter.sharedMesh.bounds.size.y + islandLevelCollider.height * 0.5f;
            islandLevelCollider.center = weightedCenter;
            islandLevelCollider.isTrigger = true;

            MeshCollider packageLevelCollider = islandComponent.gameObject.AddComponent<MeshCollider>();
            packageLevelCollider.sharedMesh = coastMeshFilter.sharedMesh;
     
            islandComponent.IslandLevelCollider = islandLevelCollider;
            islandComponent.PackageLevelCollider = packageLevelCollider;

            packageLevelCollider.enabled = false;
        }

        public void SetIslandPosition(Island islandComponent)
        {
            var islandStructure = islandComponent.CartographicIsland;
            var islandGameObject = islandStructure.IslandGameObject;

            // TODO: Refactor.
            float[] heightProfile = IslandStructureBuilder.HEIGHT_PROFILE;
            Vector3 position = islandStructure.DependencyVertex.Position;

            // Island above Ocean height.
            position.y = Mathf.Abs(heightProfile[heightProfile.Length - 1]) * 3.0f;
            islandGameObject.transform.localRotation = UIManager.Instance.ContentPane.transform.localRotation;
            islandGameObject.transform.localPosition = position;
            islandGameObject.transform.localScale = Vector3.one;
        }

        void SetLayerRecursively(GameObject obj, int layer)
        {
            if (null == obj) return;
            obj.layer = layer;

            foreach (Transform childTransform in obj.transform)
            {
                if (childTransform == null) return;
                SetLayerRecursively(childTransform.gameObject, layer);
            }
        }
    }
}
