using HoloIslandVis.Core.Builders;
using HoloIslandVis.Core.Metaphor;
using HoloIslandVis.Model.OSGi;
using HoloIslandVis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloIslandVis.Core
{
    public enum DockType
    {
        Import,
        Export
    }

    public class VisualizationLoader : SingletonComponent<VisualizationLoader>
    {
        public delegate void VisualizationLoadedHandler();
        public event VisualizationLoadedHandler VisualizationLoaded = delegate { };

        public Material DefaultMaterial;
        public Material ArrowHeadMaterial;
        public Material ImportArrowMaterial;
        public Material ExportArrowMaterial;

        public AppConfig AppConfig;
        public SyncManager SyncManager;

        public IslandStructureBuilder IslandStructureBuilder;
        public GraphLayoutBuilder GraphLayoutBuilder;
        public IslandGameObjectBuilder IslandGameObjectBuilder;
        public ServiceGameObjectBuilder GetServiceGameObjectBuilder;
        public DependencyBuilder DependencyBuilder;
        public HighlightObjectBuilder HighlightObjectBuilder;
        public SyncObjectBuilder SyncObjectBuilder;

        public List<GameObject> CUPrefabs
        {
            get { return _cuPrefabs; }
            private set { }
        }

        public List<GameObject> SIPrefabs
        {
            get { return _siPrefabs; }
            private set { }
        }

        public List<GameObject> SDPrefabs
        {
            get { return _sdPrefabs; }
            private set { }
        }

        public Dictionary<DockType, GameObject> DockPrefabs
        {
            get { return _dockPrefabs; }
            private set { }
        }

        private List<GameObject> _cuPrefabs;
        private List<GameObject> _siPrefabs;
        private List<GameObject> _sdPrefabs;
        private Dictionary<DockType, GameObject> _dockPrefabs;

        private GameObject _importDockPrefab;
        private GameObject _exportDockPrefab;
        private OSGiProject _osgiProject;

        void Start()
        {
            _dockPrefabs = new Dictionary<DockType, GameObject>();

            // Prefabs
            _cuPrefabs = Resources.LoadAll<GameObject>("Prefabs/CompilationUnit/LOD0").ToList();
            _siPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceImpl/LOD0").ToList();
            _sdPrefabs = Resources.LoadAll<GameObject>("Prefabs/ServiceDecl/LOD0").ToList();

            _dockPrefabs.Add(DockType.Import, (GameObject)Resources.Load("Prefabs/Docks/iDock_1"));
            _dockPrefabs.Add(DockType.Export, (GameObject)Resources.Load("Prefabs/Docks/eDock_1"));
        }

        public void Load(OSGiProject project)
        {
            _osgiProject = project;
            List<CartographicIsland> islandStructures = IslandStructureBuilder.Instance.BuildFromProject(project);
            Debug.Log("Island Structures built...");

            Vector3 min = new Vector3(150.0f, 150.0f, 150.0f);
            Vector3 max = new Vector3(500.0f, 500.0f, 500.0f);

            GraphLayoutBuilder.Instance.BuildRandomGraphLayout(project, min, max, 50.0f, 100);
            Debug.Log("Force directed graph layout computed...");

            IslandGameObjectBuilder.Instance.IslandGameObjectsBuilt += OnIslandGameObjectsBuilt;

            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                StartCoroutine(IslandGameObjectBuilder.Instance.BuildFromIslandStructures(islandStructures));
            });
        }

        private void OnIslandGameObjectsBuilt(IslandGameObjectsBuiltEventArgs eventArgs)
        {
            Debug.Log("Island Game Objects built...");
            ServiceGameObjectBuilder.Instance.ServiceGameObjectsBuilt += OnServiceGameObjectsBuilt;
            StartCoroutine(ServiceGameObjectBuilder.Instance.Construct(_osgiProject.Services, eventArgs.Islands));
        }

        private void OnServiceGameObjectsBuilt(ServiceGameObjectsBuiltEventArgs eventArgs)
        {
            Debug.Log("Service Game Objects built...");
            List<Island> islands = eventArgs.Islands;
            DependencyBuilder.Instance.DependenciesBuilt += OnDependenciesBuilt;
            StartCoroutine(DependencyBuilder.Instance.BuildDependenciesForIslands(islands));
        }

        private void OnDependenciesBuilt(DependenciesBuiltEventArgs eventArgs)
        {
            Debug.Log("Dependency connections established...");

            List<Island> islands = eventArgs.Islands;
            HighlightObjectBuilder.Instance.HighlightObjectsBuilt += OnHighlightObjectsBuilt;
            StartCoroutine(HighlightObjectBuilder.Instance.Build(islands));
        }

        private void OnHighlightObjectsBuilt()
        {
            Debug.Log("Highlights added...");

            if (AppConfig.SharingEnabled)
            {
                SyncObjectBuilder.Instance.SyncObjectsBuilt += OnSyncObjectsBuilt;
                StartCoroutine(SyncObjectBuilder.Instance.BuildSyncObjects());
                return;
            }

            VisualizationLoaded();
        }

        private void OnSyncObjectsBuilt()
        {
            Debug.Log("Synchronization objects instatiated...");
            VisualizationLoaded();
        }
    }
}
