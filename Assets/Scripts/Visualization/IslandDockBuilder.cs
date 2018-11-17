﻿using HoloIslandVis.OSGiParser.Graph;
using HoloIslandVis.Visualization;
using QuickGraph;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IslandDockBuilder
{
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

    public void BuildDockForIsland(Island island)
    {
        CartographicIsland islandStructure = island.CartographicIsland;

        //Get graph vertex associated with the island
        BidirectionalGraph<GraphVertex, GraphEdge> depGraph = islandStructure.Bundle.OSGiProject.DependencyGraph;
        GraphVertex vert = islandStructure.DependencyVertex;

        if (vert != null)
        {

            float importSize = 0.04f * 20f; // Don't hardcode.
            float exportSize = 0.04f * 20f; // Don't hardcode.

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
            List<GameObject> doNotCollideList = new List<GameObject>();
            doNotCollideList.Add(island.Coast);
            bool foundLocation = findSuitablePosition2D(importD, doNotCollideList, island.gameObject, 500);
            if (!foundLocation)
                Debug.Log("Could not find suitable location for " + importD.name);
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
            doNotCollideList.Clear();
            doNotCollideList.Add(island.Coast);
            foundLocation = findSuitablePosition2D(exportD, doNotCollideList, importD, 500);
            if (!foundLocation)
                Debug.Log("Could not find suitable location for " + exportD.name);
            #endregion

            #region extend Island collider based on new Docksizes
            //island.GetComponent<CapsuleCollider>().radius += Mathf.Max(importSize, exportSize) * Mathf.Sqrt(2f);
            #endregion

        }
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
            //go.layer = LayerMask.NameToLayer("CalculationOnly");
        }
        #endregion

        Vector3 originalPosition = obj.transform.position;
        Collider objCollider = obj.GetComponent<Collider>();
        Collider nearThisCollider = placeNearThis.GetComponent<Collider>();
        float placeDistance = objCollider.bounds.extents.magnitude + nearThisCollider.bounds.extents.magnitude;
        for (int i = 0; i < iterations; i++)
        {
            Vector3 dockDirection = new Vector3(UnityEngine.Random.value, 0, UnityEngine.Random.value);
            dockDirection.Normalize();
            dockDirection *= placeDistance;
            Vector3 newPossiblePosition = placeNearThis.transform.position + dockDirection;

            bool intersects = Physics.CheckSphere(newPossiblePosition, placeDistance, calculationLayermask);
            if (!intersects)
            {
                obj.transform.position = newPossiblePosition;
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