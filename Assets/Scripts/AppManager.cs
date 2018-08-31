using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Mapping;
using HoloIslandVis.OSGiParser;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis
{
    public class AppManager : SingletonComponent<AppManager>
    {
        private OSGiProject _osgiProject;
        private List<CartographicIsland> _islandStructures;
        private List<GameObject> _islandGameObjects;

        private string _filepath;
        private bool _isScanning;
        private bool _isUpdating;

        // Use this for initialization
        void Start()
        {
            _islandGameObjects = new List<GameObject>();
            RuntimeCache cache = RuntimeCache.Instance;
            _isUpdating = false;
            _isScanning = false;

            _filepath = Path.Combine(Application.streamingAssetsPath, "rce_lite.model");
            new Task(() => loadVisualization()).Start();

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
        }

        public void buildGameObjects()
        {
            foreach (CartographicIsland island in _islandStructures)
            {
                GameObject islandGameObject =
                    IslandGameObjectBuilder.Instance.BuildFromIslandStructure(island);
                _islandGameObjects.Add(islandGameObject);
            }

            RuntimeCache.Instance.IslandGameObjects = _islandGameObjects;
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Done building island game objects.");

            RuntimeCache.Instance.VisualizationContainer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
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
                    Debug.Log("Set inactive");
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
            Debug.Log("updateSurfcaePosition");
            _isUpdating = true;
            while (_isUpdating)
            {
                await Task.Delay(50);
                UnityMainThreadDispatcher.Instance.Enqueue(() => {
                    if (GazeManager.Instance.HitObject.name.Contains("SurfaceUnderstanding Mesh"))
                    {
                        if (!UserInterface.Instance.ContentSurface.activeInHierarchy)
                            UserInterface.Instance.ContentSurface.SetActive(true);

                            UserInterface.Instance.ContentSurface.transform.position =
                            Vector3.Lerp(UserInterface.Instance.ContentSurface.transform.position, GazeManager.Instance.HitPosition, 0.1f);
                            UserInterface.Instance.ContentSurface.transform.up =
                            Vector3.Lerp(UserInterface.Instance.ContentSurface.transform.up, GazeManager.Instance.HitNormal, 0.1f);
                    }
                });
            }

            UnityMainThreadDispatcher.Instance.Enqueue(() => {
                UserInterface.Instance.ContentSurface.layer = LayerMask.NameToLayer("Default");
                GameObject.Find("SpatialUnderstanding").SetActive(false);
            });

            UnityMainThreadDispatcher.Instance.Enqueue(() => setupStateMachine());
        }

        public void setupStateMachine()
        {
            Debug.Log("setupStateMachine");
            StateMachine stateMachine = new StateMachine();
            State testState = new State("test");

            Command commandDragStart = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command commandDragUpdate = new Command(GestureType.OneHandManipUpdate, KeywordType.Invariant, InteractableType.Invariant);
            Command commandDragEnd = new Command(GestureType.OneHandManipEnd, KeywordType.Invariant, InteractableType.Invariant);

            Command commandZoomStart = new Command(GestureType.TwoHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command commandZoomUpdate = new Command(GestureType.TwoHandManipUpdate, KeywordType.Invariant, InteractableType.Invariant);
            Command commandZoomEnd = new Command(GestureType.TwoHandManipEnd, KeywordType.Invariant, InteractableType.Invariant);

            ContentSurfaceDrag contentSurfaceDrag = new ContentSurfaceDrag();
            ContentSurfaceZoom contentSurfaceZoom = new ContentSurfaceZoom();

            //Command commandFind = new Command(GestureType.Invariant, KeywordType.Find, InteractableType.Invariant);
            //FindEntitiesTask findEntitiesTask = new FindEntitiesTask();

            testState.AddInteractionTask(commandDragStart, contentSurfaceDrag);
            testState.AddInteractionTask(commandDragUpdate, contentSurfaceDrag);
            testState.AddInteractionTask(commandDragEnd, contentSurfaceDrag);

            testState.AddInteractionTask(commandZoomStart, contentSurfaceZoom);
            testState.AddInteractionTask(commandZoomUpdate, contentSurfaceZoom);
            testState.AddInteractionTask(commandZoomEnd, contentSurfaceZoom);

            //testState.AddInteractionTask(commandFind, findEntitiesTask);

            stateMachine.AddState(testState);
            Debug.Log("IsInitialized is called in AppManager");
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
            GestureInputListener.Instance.OneHandManipEnd += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ManipulationEnd";
        }
    }
}