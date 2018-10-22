using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Mapping;
using HoloIslandVis.OSGiParser;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.UX.ToolTips;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis
{
    public class AppManager : SingletonComponent<AppManager>
    {
        private OSGiProject _osgiProject;
        private List<CartographicIsland> _islandStructures;
        private List<Island> _islands;

        private string _filepath;
        private bool _isScanning;
        private bool _isUpdating;

        // Use this for initialization
        void Start()
        {
            _islands = new List<Island>();
            ToolTipManager ttm = gameObject.AddComponent<ToolTipManager>();
            RuntimeCache cache = RuntimeCache.Instance;
            cache.toolTipManager = ttm;
            _isUpdating = false;
            _isScanning = false;

            _filepath = Path.Combine(Application.streamingAssetsPath, "rce_lite.model");
            new Task(() => loadVisualization()).Start();

            //inputListenerDebug();
            initScene();
        }

        public void loadVisualization()
        {
            JSONObject modelData = ModelDataReader.Instance.Read(new Uri(_filepath).AbsolutePath);
            UnityMainThreadDispatcher.Instance.Enqueue(() => 
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done parsing model data.");

            _osgiProject = OSGiProjectParser.Instance.Parse(modelData);
            UnityMainThreadDispatcher.Instance.Enqueue(() => 
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done parsing OSGiProject data.");

            // TODO: Refactor IslandStructureBuilder.
            _islandStructures = IslandStructureBuilder.Instance.BuildFromProject(_osgiProject);
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building island structures.");

            // TODO: Refactor GraphLayoutBuilder.
            GraphLayoutBuilder.Instance.ConstructFDLayout(_osgiProject, 0.25f, 70000);
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building dependency graph.");

            UnityMainThreadDispatcher.Instance.Enqueue(() => buildGameObjects());
            UnityMainThreadDispatcher.Instance.Enqueue(() => buildDocks());
        }

        public void buildGameObjects()
        {
            foreach (CartographicIsland island in _islandStructures)
            {
                GameObject islandGameObject =
                    IslandGameObjectBuilder.Instance.BuildFromIslandStructure(island);
                _islands.Add(islandGameObject.GetComponent<Island>());
            }

            RuntimeCache.Instance.Islands = _islands;
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building island game objects.");

            foreach(Island island in _islands)
            {
                island.gameObject.AddComponent<Interactable>();
                MeshFilter[] islandMeshFilters = island.gameObject.GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combineInstance = new CombineInstance[islandMeshFilters.Length];

                for (int i = 0; i < islandMeshFilters.Length; i++)
                {
                    combineInstance[i].mesh = islandMeshFilters[i].sharedMesh;
                    combineInstance[i].transform = islandMeshFilters[i].transform.localToWorldMatrix;
                }

                GameObject highlight = new GameObject("Highlight");
                highlight.transform.parent = island.gameObject.transform;
                highlight.AddComponent<MeshFilter>().mesh.CombineMeshes(combineInstance);
                MeshRenderer meshRenderer = highlight.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = RuntimeCache.Instance.WireFrame;
                meshRenderer.enabled = false;
            }

            RuntimeCache.Instance.VisualizationContainer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        public void buildDocks()
        {
            List<Island> islands = RuntimeCache.Instance.Islands;
            for (int i = 0; i < islands.Count; i++)
            {
                IslandDockBuilder.Instance.BuildDockForIsland(islands[i]);
                
            }
        }

        public void initScene()
        {
            SpatialScan.Instance.RequestBeginScanning();

            UserInterface.Instance.ScanInstructionText.SetActive(true);
            UserInterface.Instance.ScanProgressBar.SetActive(true);
            _isScanning = true;

            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventArgs) =>
            {
                if (_isUpdating)
                {
                    UserInterface.Instance.ScanInstructionText.SetActive(false);
                    _isUpdating = false;
                }
            };

            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventArgs) =>
            {
                if (SpatialScan.Instance.TargetPlatformCellCount <= SpatialScan.Instance.PlatformCellCount && _isScanning)
                {
                    SpatialScan.Instance.RequestFinishScanning();
                    UserInterface.Instance.ScanProgressBar.SetActive(false);
                    UserInterface.Instance.ContentSurface.SetActive(true);
                    new Task(() => updateSurfacePosition()).Start();
                    _isScanning = false;
                }
            };
        }

        private async void updateSurfacePosition()
        {
            Debug.Log("updateSurfacePosition");
            _isUpdating = true;
            while (_isUpdating)
            {
                await Task.Delay(25);
                UnityMainThreadDispatcher.Instance.Enqueue(() => {
                    if (GazeManager.Instance.HitObject.name.Contains("SurfaceUnderstanding Mesh"))
                    {
                        if (!UserInterface.Instance.ContentSurface.activeInHierarchy)
                            UserInterface.Instance.ContentSurface.SetActive(true);

                            UserInterface.Instance.ContentSurface.transform.position =
                            Vector3.Lerp(UserInterface.Instance.ContentSurface.transform.position, GazeManager.Instance.HitPosition, 0.2f);
                            UserInterface.Instance.ContentSurface.transform.up =
                            Vector3.Lerp(UserInterface.Instance.ContentSurface.transform.up, GazeManager.Instance.HitNormal, 0.5f);
                    }
                });
            }
            

            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                UserInterface.Instance.ContentSurface.layer = LayerMask.NameToLayer("Default");
                GameObject.Find("Water").layer = LayerMask.NameToLayer("Default");
                GameObject.Find("Glow").layer = LayerMask.NameToLayer("Default");
                GameObject.Find("SpatialUnderstanding").SetActive(false);
            });

            UnityMainThreadDispatcher.Instance.Enqueue(() => setupStateMachine());
        }

        public void setupStateMachine()
        {
            Debug.Log("setupStateMachine");
            StateMachine stateMachine = new StateMachine();
            State testState = new State("test");

            Command commandSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Island);
            Command commandDeselect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.None);
            Command commandSurfaceDrag = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command commandSurfaceZoom = new Command(GestureType.TwoHandManipStart, KeywordType.Invariant, InteractableType.Invariant);

            IslandSelectTask islandSelectTask = new IslandSelectTask();
            IslandDeselectTask islandDeselectTask = new IslandDeselectTask();
            SurfaceDragTask surfaceDragTask = new SurfaceDragTask();
            SurfaceZoomTask surfaceZoomTask = new SurfaceZoomTask();

            testState.AddInteractionTask(commandSelect, islandSelectTask);
            testState.AddInteractionTask(commandDeselect, islandDeselectTask);
            testState.AddInteractionTask(commandSurfaceDrag, surfaceDragTask);
            testState.AddInteractionTask(commandSurfaceZoom, surfaceZoomTask);

            stateMachine.AddState(testState);
            stateMachine.Init(testState);
        }

        public void inputListenerDebug()
        {
            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "OneHandTap";
            GestureInputListener.Instance.TwoHandTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TwoHandTap";
            GestureInputListener.Instance.OneHandDoubleTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "OneHandDoubleTap";
            GestureInputListener.Instance.TwoHandDoubleTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TwoHandDoubleTap";
            GestureInputListener.Instance.OneHandManipStart += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "OneHandManipulationStart";
            GestureInputListener.Instance.TwoHandManipStart += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TwoHandManipulationStart";
            GestureInputListener.Instance.ManipulationEnd += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ManipulationEnd";
        }
    }
}