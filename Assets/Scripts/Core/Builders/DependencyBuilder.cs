using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Model.Graph;
using HoloIslandVis.Sharing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloIslandVis.Core.Builders
{
    public class DependencyBuilder : SingletonComponent<DependencyBuilder>
    {
        public delegate void DependenciesBuiltHandler(DependenciesBuiltEventArgs eventArgs);
        public event DependenciesBuiltHandler DependenciesBuilt = delegate { };

        void Start()
        {

        }

        public IEnumerator BuildDependenciesForIslands(List<Island> islands)
        {
            for (int i = 0; i < islands.Count; i++)
                yield return AdjustDockTransform(islands[i]);

            for (int i = 0; i < islands.Count; i++)
            {
                Island island = islands[i].GetComponent<Island>();
                GameObject exportDock = island.ExportDock;
                GameObject importDock = island.ImportDock;

                if (exportDock != null)
                    exportDock.GetComponent<DependencyDock>().constructConnectionArrows();

                if (importDock != null)
                    importDock.GetComponent<DependencyDock>().constructConnectionArrows();
            }

            var eventArgs = new DependenciesBuiltEventArgs(islands);
            DependenciesBuilt(eventArgs);
        }

        public IEnumerator AdjustDockTransform(Island island)
        {
            CartographicIsland islandStructure = island.CartographicIsland;
            var dependencyGraph = islandStructure.Bundle.OSGiProject.DependencyGraph;
            GraphVertex vertex = islandStructure.DependencyVertex;

            GameObject importDock = island.ImportDock;
            GameObject exportDock = island.ExportDock;

            yield return FindSuitablePosition2D(island, importDock, exportDock);

            if (vertex == null)
                yield break;

            float importSize = 0.04f * 25f; // Don't hardcode.
            float exportSize = 0.04f * 25f; // Don't hardcode.

            IEnumerable<GraphEdge> outEdges;
            dependencyGraph.TryGetOutEdges(vertex, out outEdges);
            List<GraphEdge> edgeList = outEdges.ToList();

            importDock.transform.localScale = new Vector3(importSize, importSize, importSize);
            DependencyDock dockComponent = importDock.GetComponent<DependencyDock>();
            dockComponent.setDockType(DockType.Import);

            foreach (GraphEdge e in edgeList)
            {
                GameObject ed = e.Target.Island.IslandGameObject.GetComponent<Island>().ExportDock;
                dockComponent.addDockConnection(ed.GetComponent<DependencyDock>(), e.Weight);
            }

            dependencyGraph.TryGetInEdges(vertex, out outEdges);
            edgeList = outEdges.ToList();

            float eDockWidth = exportDock.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * exportSize;
            float iDockWidth = importDock.GetComponent<MeshFilter>().sharedMesh.bounds.size.x * importSize;
            exportDock.transform.localScale = new Vector3(exportSize, exportSize, exportSize);
            dockComponent = exportDock.GetComponent<DependencyDock>();
            dockComponent.setDockType(DockType.Export);

            foreach (GraphEdge e in edgeList)
            {
                GameObject id = e.Source.Island.IslandGameObject.GetComponent<Island>().ImportDock;
                dockComponent.addDockConnection(id.GetComponent<DependencyDock>(), e.Weight);
            }
        }

        private IEnumerator FindSuitablePosition2D(Island island, GameObject importDock, GameObject exportDock)
        {
            CapsuleCollider islandCollider = island.gameObject.GetComponent<CapsuleCollider>();
            BoxCollider importDockCollider = importDock.gameObject.GetComponent<BoxCollider>();
            BoxCollider exportDockCollider = exportDock.gameObject.GetComponent<BoxCollider>();

            float distance = islandCollider.radius + importDockCollider.bounds.extents.magnitude;

            float value_x = SyncDataStorage.Instance.GetRandomNumber(null);
            float value_z = SyncDataStorage.Instance.GetRandomNumber(null);

            Vector3 importDockDirection = new Vector3(value_x, 0, value_z).normalized;
            Vector3 importDockPosition = importDockDirection * distance; //* Random.Range(1.0f, 1.2f);
            importDockPosition.y = -2.0f;

            importDock.transform.localPosition = importDockPosition;

            float value_range = 0.30f * SyncDataStorage.Instance.GetRandomNumber(null);
            float angle_range = 10.0f * SyncDataStorage.Instance.GetRandomNumber(null);

            Vector3 exportDockPosition = importDockPosition * (0.95f + value_range);
            exportDockPosition = Quaternion.AngleAxis(35.0f + angle_range, island.gameObject.transform.up) * exportDockPosition;
            exportDockPosition.y = -2.0f;

            importDock.transform.localPosition = importDockPosition;
            exportDock.transform.localPosition = exportDockPosition;

            yield return null;
        }
    }
}
