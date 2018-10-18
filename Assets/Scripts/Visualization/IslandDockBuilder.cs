using HoloIslandVis.OSGiParser.Graph;
using HoloIslandVis.Visualization;
using QuickGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandDockBuilder : MonoBehaviour
{
    private static IslandDockBuilder _instance;
    public static IslandDockBuilder Instance
    {
        get
        {
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

            //float importSize = GlobalVar.minDockSize;
            //float exportSize = GlobalVar.minDockSize;

            ////Outgoing edges -Bundle depends on...
            //IEnumerable<GraphEdge> outEdges;
            //depGraph.TryGetOutEdges(vert, out outEdges);
            //List<GraphEdge> edgeList = outEdges.ToList();
            //importSize = Helperfunctions.mapDependencycountToSize(edgeList.Count);
            ////Import Dock
            //GameObject importD = island.getImportDock();
            //importD.transform.localScale = new Vector3(importSize, importSize, importSize);
            ////Link dependencies
            //DependencyDock dockComponent = importD.GetComponent<DependencyDock>();
            //dockComponent.setDockType(DockType.ImportDock);
            //foreach (GraphEdge e in edgeList)
            //{
            //    GameObject ed = e.Target.getIsland().getIslandGO().GetComponent<IslandGO>().getExportDock();
            //    dockComponent.addDockConnection(ed.GetComponent<DependencyDock>(), e.getWeight());
            //}

            //#region determine optimal Position for ImportDock
            //List<GameObject> doNotCollideList = new List<GameObject>();
            //doNotCollideList.Add(island.getCoast());
            //bool foundLocation = findSuitablePosition2D(importD, doNotCollideList, island.gameObject, 500);
            //if (!foundLocation)
            //    Debug.Log("Could not find suitable location for " + importD.name);
            //#endregion



            ////Ingoing edges -Other Bundles depends on this one...
            //depGraph.TryGetInEdges(vert, out outEdges);
            //edgeList = outEdges.ToList();
            //exportSize = Helperfunctions.mapDependencycountToSize(edgeList.Count);
            ////Export Dock
            //GameObject exportD = island.getExportDock();
            //float eDockWidth = exportD.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * exportSize;
            //float iDockWidth = importD.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * importSize;
            ////exportD.transform.position = importD.transform.position + Vector3.left * (iDockWidth + eDockWidth) * 0.5f;                
            //exportD.transform.localScale = new Vector3(exportSize, exportSize, exportSize);
            ////Link dependencies
            //dockComponent = exportD.GetComponent<DependencyDock>();
            //dockComponent.setDockType(DockType.ExportDock);
            //foreach (GraphEdge e in edgeList)
            //{
            //    GameObject id = e.Source.getIsland().getIslandGO().GetComponent<IslandGO>().getImportDock();
            //    dockComponent.addDockConnection(id.GetComponent<DependencyDock>(), e.getWeight());
            //}

            //#region determine optimal Position for ExportDock
            //doNotCollideList.Clear();
            //doNotCollideList.Add(island.getCoast());
            //foundLocation = findSuitablePosition2D(exportD, doNotCollideList, importD, 500);
            //if (!foundLocation)
            //    Debug.Log("Could not find suitable location for " + exportD.name);
            //#endregion


            //#region extend Island collider based on new Docksizes
            //island.GetComponent<CapsuleCollider>().radius += Mathf.Max(importSize, exportSize) * Mathf.Sqrt(2f);
            //#endregion

        }
    }
}
