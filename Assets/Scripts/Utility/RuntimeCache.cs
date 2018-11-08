using HoloIslandVis.Visualization;
using System.Collections;
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
        Dictionary<DockType, GameObject> _dockPrefabs;

        public ToolTipManager toolTipManager { get; internal set; }
        private ConnectionPool _connectionPool;

        private GameObject _importDockPrefab;
        private GameObject _exportDockPrefab;

        public int NumBuildingLevels {
            get { return _numBuildingLevels; }
            private set { }
        }

        public List<Island> Islands { get; set; }
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

            // Object references
            _visualizationContainer = GameObject.Find("VisualizationContainer");
            _dependencyContainer = GameObject.Find("DependencyContainer");
            _connectionPool = GameObject.Find("Content").GetComponent<ConnectionPool>();
            _contentSurface = GameObject.Find("ContentSurface");
            _surfaceGlow = GameObject.Find("Glow");

            _contentSurface.SetActive(false);

            // Stuff
            //_canvas = GameObject.Find("Canvas");
            //_canvas.transform.localScale = Vector3.one * _canvas.transform.position.z * 0.00415f;
            //_progressInfo = _canvas.GetComponentInChildren<Text>();

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