using HoloIslandVis.OSGiParser.Graph;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using QuickGraph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IslandDockBuilder
{
    public delegate void ConstructionCompletedHandler();
    public event ConstructionCompletedHandler ConstructionCompleted = delegate { };

    private bool _constructionComplete;

    public bool ConstructionComplete {
        get { return _constructionComplete; }
        private set {
            _constructionComplete = value;
            if (value)
                ConstructionCompleted();
        }
    }

    private static IslandDockBuilder _instance;
    public static IslandDockBuilder Instance
    {
        get{
            if (_instance == null)
                _instance = new IslandDockBuilder();

            return _instance;
        }

        private set { }
    }

    public void BuildDocksForIslands(List<Island> islands)
    {
        List<GameObject> docks = new List<GameObject>();
        RuntimeCache.Instance.Docks = docks;

        for (int i = 0; i < islands.Count; i++)
            buildDockForIsland(islands[i]);

        for (int i = 0; i < islands.Count; i++)
        {
            Island island = islands[i].GetComponent<Island>();
            GameObject eDock = island.ExportDock;
            GameObject iDock = island.ImportDock;
            if (eDock != null)
            {
                docks.Add(eDock);
                eDock.GetComponent<DependencyDock>().constructConnectionArrows();
            }
            if (iDock != null)
            {
                docks.Add(iDock);
                iDock.GetComponent<DependencyDock>().constructConnectionArrows();
            }
        }

        Debug.Log("Finished with Dock-GameObject construction!");

        foreach (Island island in islands)
        {
            island.gameObject.AddComponent<Interactable>();
            MeshFilter[] islandMeshFilters = island.gameObject.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combineInstance = new CombineInstance[islandMeshFilters.Length];

            for (int i = 0; i < islandMeshFilters.Length; i++)
            {
                if (islandMeshFilters[i].gameObject.name.Contains("Import"))
                    continue;

                if (islandMeshFilters[i].gameObject.name.Contains("Export"))
                    continue;

                combineInstance[i].mesh = islandMeshFilters[i].sharedMesh;
                combineInstance[i].transform = islandMeshFilters[i].transform.localToWorldMatrix;
                

                combineInstance[i].transform *= Matrix4x4.Scale(Vector3.one*1.0001f);
            }

            GameObject highlight = new GameObject(island.name + "_Highlight");
            highlight.tag = "Highlight";
            highlight.transform.parent = island.gameObject.transform;
            highlight.transform.localPosition += new Vector3(0, 0.001f, 0);
            MeshFilter meshFilter = highlight.AddComponent<MeshFilter>();
            meshFilter.mesh.CombineMeshes(combineInstance);

            MeshRenderer meshRenderer = highlight.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = RuntimeCache.Instance.WireFrame;
            meshRenderer.enabled = false;
        }

        ConstructionComplete = true;
    }

    private void buildDockForIsland(Island island)
    {
        CartographicIsland islandStructure = island.CartographicIsland;

        //Get graph vertex associated with the island
        BidirectionalGraph<GraphVertex, GraphEdge> depGraph = islandStructure.Bundle.OSGiProject.DependencyGraph;
        GraphVertex vert = islandStructure.DependencyVertex;

        if (vert != null)
        {

            float importSize = 0.04f * 25f; // Don't hardcode.
            float exportSize = 0.04f * 25f; // Don't hardcode.

            //Outgoing edges -Bundle depends on...
            IEnumerable<GraphEdge> outEdges;
            depGraph.TryGetOutEdges(vert, out outEdges);
            List<GraphEdge> edgeList = outEdges.ToList();
            //importSize = mapDependencyCountToSize(edgeList.Count);

            //Import Dock
            GameObject importD = island.ImportDock;
            importD.transform.localScale = new Vector3(importSize, importSize, importSize);

            //Link dependencies
            DependencyDock dockComponent = importD.GetComponent<DependencyDock>();
            dockComponent.setDockType(DockType.Import);
            foreach (GraphEdge e in edgeList)
            {
                GameObject ed = e.Target.Island.IslandGameObject.GetComponent<Island>().ExportDock;
                dockComponent.addDockConnection(ed.GetComponent<DependencyDock>(), e.Weight);
            }

            #region determine optimal Position for ImportDock
            //List<GameObject> doNotCollideList = new List<GameObject>();
            //doNotCollideList.Add(island.Coast);
            //bool foundLocation = findSuitablePosition2D(importD, doNotCollideList, island.gameObject, 500);
            //if (!foundLocation)
            //    Debug.Log("Could not find suitable location for " + importD.name);
            #endregion



            //Ingoing edges -Other Bundles depends on this one...
            depGraph.TryGetInEdges(vert, out outEdges);
            edgeList = outEdges.ToList();
            //exportSize = mapDependencyCountToSize(edgeList.Count);
            //Export Dock
            GameObject exportD = island.ExportDock;
            float eDockWidth = exportD.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * exportSize;
            float iDockWidth = importD.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * importSize;
            //exportD.transform.position = importD.transform.position + Vector3.left * (iDockWidth + eDockWidth) * 0.5f;                
            exportD.transform.localScale = new Vector3(exportSize, exportSize, exportSize);
            //Link dependencies
            dockComponent = exportD.GetComponent<DependencyDock>();
            dockComponent.setDockType(DockType.Export);
            foreach (GraphEdge e in edgeList)
            {
                GameObject id = e.Source.Island.IslandGameObject.GetComponent<Island>().ImportDock;
                dockComponent.addDockConnection(id.GetComponent<DependencyDock>(), e.Weight);
            }

            #region determine optimal Position for ExportDock
            //doNotCollideList.Clear();
            //doNotCollideList.Add(island.Coast);
            //foundLocation = findSuitablePosition2D(exportD, doNotCollideList, importD, 2000);
            //if (!foundLocation)
            //    Debug.Log("Could not find suitable location for " + exportD.name);
            #endregion

            findSuitablePosition2D(island, importD, exportD);

            #region extend Island collider based on new Docksizes
            //island.GetComponent<CapsuleCollider>().radius += Mathf.Max(importSize, exportSize) * Mathf.Sqrt(2f);
            #endregion

        }
    }

    private void findSuitablePosition2D(Island island, GameObject importDock, GameObject exportDock)
    {
        CapsuleCollider islandCollider = island.gameObject.GetComponent<CapsuleCollider>();
        BoxCollider importDockCollider = importDock.gameObject.GetComponent<BoxCollider>();
        BoxCollider exportDockCollider = exportDock.gameObject.GetComponent<BoxCollider>();

        float distance = islandCollider.radius + importDockCollider.bounds.extents.magnitude;

        Vector3 importDockDirection = new Vector3(Random.value, 0, Random.value).normalized;
        Vector3 importDockPosition = importDockDirection * distance * Random.Range(1.0f, 1.2f);
        importDockPosition.y = -2.0f;

        importDock.transform.localPosition = importDockPosition;

        Vector3 exportDockPosition = importDockPosition * Random.Range(0.95f, 1.25f);
        exportDockPosition = Quaternion.AngleAxis(Random.Range(25, 35), Vector3.up) * exportDockPosition;
        exportDockPosition.y = -2.0f;
        importDock.transform.localPosition = importDockPosition;

        //Vector3 exportDockDirection = new Vector3(Random.value, 0, Random.value).normalized;
        //Vector3 exportDockPosition = importDockPosition + (exportDockDirection * importDockCollider.bounds.extents.magnitude * Random.Range(1.15f, 1.25f));
        //exportDockPosition.y = -2.0f;

        exportDock.transform.localPosition = exportDockPosition;
    }

    private bool findSuitablePosition2D(GameObject obj, List<GameObject> doNotCollideWith, GameObject placeNearThis, int iterations)
    {
        bool result = false;
        List<GameObject> objsWithMeshCollider = new List<GameObject>();
        int calculationLayermask = 1 << LayerMask.NameToLayer("CalculationOnly");

        #region clone doNotCollideWith objects and give them a mesh collider
        foreach (GameObject go in doNotCollideWith)
            objsWithMeshCollider.Add(GameObject.Instantiate(go, go.transform.parent));
        foreach (GameObject go in objsWithMeshCollider)
        {
            GameObject.Destroy(go.GetComponent<Collider>());
            MeshCollider mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            mc.convex = true;
            go.layer = LayerMask.NameToLayer("CalculationOnly");
        }
        #endregion

        Vector3 originalPosition = obj.transform.localPosition;
        Collider objCollider = obj.GetComponent<Collider>();
        Collider nearThisCollider = placeNearThis.GetComponent<Collider>();

        float placeDistance = (objCollider.bounds.extents.magnitude + nearThisCollider.bounds.extents.magnitude);
        for (int i = 0; i < iterations; i++)
        {
            Vector3 dockDirection = new Vector3(Random.value, 0, Random.value);
            dockDirection.Normalize();

            if(obj.GetComponent<DependencyDock>().DockType == DockType.Import)
                dockDirection = dockDirection * placeDistance;
            else
                dockDirection = dockDirection * placeDistance;

            Vector3 newPossiblePosition = dockDirection;
            newPossiblePosition.y = -2.0f;

            bool intersects = Physics.CheckSphere(newPossiblePosition, placeDistance, calculationLayermask);
            if (!intersects)
            {
                obj.transform.localPosition = newPossiblePosition;
                result = true;
                break;
            }

        }

        #region cleanup
        foreach (GameObject go in objsWithMeshCollider)
            GameObject.Destroy(go);
        #endregion

        return result;
    }

    // Do not hardcode scale factors.
    private float mapDependencyCountToSize(long count)
    {
        if (count == 0)
            return 0.02f * 20f;
        else
            return (0.02f * 20f) + (Mathf.Sqrt((float)count) * 0.005f * 20f);
    }
}
