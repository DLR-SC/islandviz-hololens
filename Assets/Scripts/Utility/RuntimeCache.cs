using HoloIslandVis.Component.UI;
using HoloIslandVis.OSGiParser;
using HoloIslandVis.Visualization;
using HoloToolkit.Sharing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum DockType
{
    Import,
    Export
}

namespace HoloIslandVis.Utility
{
    public class RuntimeCache : Singleton<RuntimeCache>
    {
        private const int _numBuildingLevels = 8;

        private ConnectionPool _connectionPool;
        private ToolTipManager _toolTipManager;

        private GameObject _visualizationContainer;
        private GameObject _dependencyContainer;
        private GameObject _contentSurface;
        private GameObject _surfaceGlow;
        private GameObject _canvas;

        private Text _progressInfo;
        private Material _combinedHoloMaterial;
        private Material _arrowHeadMaterial;
        private Material _exportArrowMaterial;
        private Material _importArrowMaterial;
        private Material _wireFrame;
        private Material _highlightMaterial;

        private List<GameObject> _cuPrefabs;
        private List<GameObject> _siPrefabs;
        private List<GameObject> _sdPrefabs;
        private Dictionary<DockType, GameObject> _dockPrefabs;

        private GameObject _importDockPrefab;
        private GameObject _exportDockPrefab;

        public int NumBuildingLevels {
            get { return _numBuildingLevels; }
        }

        public OSGiProject OSGiProject { get; set; }
        public List<CartographicIsland> IslandStructures { get; set; }
        public List<Island> Islands { get; set; }
        public List<GameObject> Highlights { get; set; }
        public List<GameObject> Docks { get; set; }

        public GameObject CurrentFocus { get; set; }

        public GameObject VisualizationContainer {
            get { return _visualizationContainer; }
            private set { }
        }

        public GameObject DependencyContainer
        {
            get { return _dependencyContainer; }
            private set { }
        }

        public ConnectionPool ConnectionPool {
            get { return _connectionPool; }
            private set { }
        }

        public ToolTipManager ToolTipManager {
            get { return _toolTipManager; }
            private set { }
        }

        public GameObject ContentSurface {
            get { return _contentSurface; }
            private set { }
        }

        public GameObject Canvas {
            get { return _canvas; }
            private set { }
        }

        public Text ProgressInfo {
            get { return _progressInfo; }
            private set { }
        }

        public Material WireFrame {
            get { return _wireFrame; }
            private set { }
        }

        public Material CombinedHoloMaterial {
            get { return _combinedHoloMaterial; }
            private set { }
        }

        public Material ArrowHeadMaterial
        {
            get { return _arrowHeadMaterial; }
            private set { }
        }

        public Material ExportArrowMaterial
        {
            get { return _exportArrowMaterial; }
            private set { }
        }

        public Material ImportArrowMaterial
        {
            get { return _importArrowMaterial; }
            private set { }
        }

        public Material HighlightMaterial {
            get { return _highlightMaterial; }
            private set { }
        }

        public List<GameObject> CUPrefabs {
            get { return _cuPrefabs; }
            private set { }
        }

        public List<GameObject> SIPrefabs {
            get { return _siPrefabs; }
            private set { }
        }

        public List<GameObject> SDPrefabs {
            get { return _sdPrefabs; }
            private set { }
        }

        public Dictionary<DockType, GameObject> DockPrefabs {
            get { return _dockPrefabs; }
            private set { }
        }

        private RuntimeCache()
        {
            // Init
            _dockPrefabs = new Dictionary<DockType, GameObject>();
            Highlights = new List<GameObject>();

            // Object references
            _visualizationContainer = GameObject.Find("VisualizationContainer");
            _dependencyContainer = GameObject.Find("DependencyContainer");
            _connectionPool = GameObject.Find("Content").GetComponent<ConnectionPool>();
            _toolTipManager = GameObject.Find("Content").GetComponent<ToolTipManager>();
            _contentSurface = GameObject.Find("ContentSurface");
            _surfaceGlow = GameObject.Find("Glow");

            _contentSurface.SetActive(false);

            _combinedHoloMaterial = (Material) Resources.Load("Materials/CombinedHoloMaterial");
            _arrowHeadMaterial = (Material)Resources.Load("Materials/ArrowHead");
            _exportArrowMaterial = (Material)Resources.Load("Materials/ExportArrow");
            _importArrowMaterial = (Material)Resources.Load("Materials/ImportArrow");
            _wireFrame = (Material) Resources.Load("Materials/WireFrame");
            _highlightMaterial = (Material) Resources.Load("Materials/Glow");

            // Prefabs
            _cuPrefabs = Resources.LoadAll<GameObject>("Prefabs/CompilationUnit/LOD0").ToList();
            _siPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD0").ToList();
            _sdPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceDecl/LOD0").ToList();

            _dockPrefabs.Add(DockType.Import, (GameObject) Resources.Load("Prefabs/Docks/iDock_1"));
            _dockPrefabs.Add(DockType.Export, (GameObject) Resources.Load("Prefabs/Docks/eDock_1"));
        }

        public void BuildSceneFromFile(string filepath)
        {
            JSONObject modelData = ModelDataReader.Instance.Read(new Uri(filepath).AbsolutePath);
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done parsing model data.");

            OSGiProject = OSGiProjectParser.Instance.Parse(modelData);
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done parsing OSGiProject data.");

            // TODO: Refactor IslandStructureBuilder.
            IslandStructures = IslandStructureBuilder.Instance.BuildFromProject(OSGiProject);
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building island structures.");

            // TODO: Refactor GraphLayoutBuilder.
            GraphLayoutBuilder.Instance.ConstructFDLayout(OSGiProject, 0.25f, 70000);
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building dependency graph.");

            // TODO: Refactor IslandGameObjectBuilder & IslandDockBuilder.
            IslandGameObjectBuilder.Instance.ConstructionCompleted += ()
                => UnityMainThreadDispatcher.Instance.Enqueue(IslandDockBuilder.Instance.BuildDocksForIslands, Islands);

            IslandDockBuilder.Instance.ConstructionCompleted += () =>
            {
                _contentSurface.AddComponent<ObjectStateSynchronizer>().TransformChange +=
                    _contentSurface.GetComponent<ContentSurfaceTransformTracker>().OnTransformChange;

                _visualizationContainer.AddComponent<ObjectStateSynchronizer>();
                _dependencyContainer.AddComponent<ObjectStateSynchronizer>();

                foreach (Island island in Islands)
                    island.gameObject.AddComponent<ObjectStateSynchronizer>();

                foreach (GameObject connection in _connectionPool.Pool.Values.Distinct())
                {
                    connection.AddComponent<ObjectStateSynchronizer>();
                    Transform[] children = connection.GetComponentsInChildren<Transform>(true);
                    for (int i = 1; i < children.Length; i++)
                        children[i].gameObject.AddComponent<ObjectStateSynchronizer>();
                }

                DependencyDock[] docks = GameObject.FindObjectsOfType<DependencyDock>();
                foreach (DependencyDock dock in docks)
                    dock.gameObject.AddComponent<ObjectStateSynchronizer>();

                var highlights = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("_Highlight"));
                foreach (GameObject highlight in highlights)
                {
                    highlight.AddComponent<ObjectStateSynchronizer>();
                    highlight.SetActive(false);
                }

                UserInterface.Instance.Panel.AddComponent<ObjectStateSynchronizer>();
                UserInterface.Instance.Panel.SetActive(false);
                UserInterface.Instance.ParsingProgressText.SetActive(false);
            };

            UnityMainThreadDispatcher.Instance.Enqueue(IslandGameObjectBuilder.Instance.BuildFromIslandStructures, IslandStructures);
        }

        public Island GetIsland(string name)
        {
            foreach(Island island in Islands)
            {
                if(island.name == name)
                    return island;
            }

            return null;
        }
    }

}