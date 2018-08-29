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
            UnityMainThreadDispatcher.Instance.Enqueue(() => Debug.Log("Done parsing model data."));

            _osgiProject = OSGiProjectParser.Instance.Parse(modelData);
            UnityMainThreadDispatcher.Instance.Enqueue(() => Debug.Log("Done parsing OSGiProject data."));

            // TODO: Refactor IslandStructureBuilder.
            _islandStructures = IslandStructureBuilder.Instance.BuildFromProject(_osgiProject);
            UnityMainThreadDispatcher.Instance.Enqueue(() => Debug.Log("Done building island structures."));

            // TODO: Refactor GraphLayoutBuilder.
            GraphLayoutBuilder.Instance.ConstructFDLayout(_osgiProject, 0.25f, 70000);
            UnityMainThreadDispatcher.Instance.Enqueue(() => Debug.Log("Done building dependency graph."));

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

            UnityMainThreadDispatcher.Instance.Enqueue(() => Debug.Log("Done building island game objects."));
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
                    //UserInterface.Instance.ContentSurface.SetActive(true);
                    new Task(() => updateSurfacePosition()).Start();
                    _isScanning = false;
                }
            };
        }

        private async void updateSurfacePosition()
        {
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

            setupStateMachine();
        }

        public void setupStateMachine()
        {
            StateMachine stateMachine = new StateMachine();
            State testState = new State("test");

            Command commandStart = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command commandUpdate = new Command(GestureType.ManipulationUpdate, KeywordType.Invariant, InteractableType.Invariant);
            Command commandEnd = new Command(GestureType.ManipulationEnd, KeywordType.Invariant, InteractableType.Invariant);
            ContentSurfaceDrag contentSurfaceDrag = new ContentSurfaceDrag();

            testState.AddInteractionTask(commandStart, contentSurfaceDrag);
            testState.AddInteractionTask(commandUpdate, contentSurfaceDrag);
            testState.AddInteractionTask(commandEnd, contentSurfaceDrag);
            stateMachine.AddState(testState);
            stateMachine.Init(testState);
        }

        public void inputListenerDebug()
        {
            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventData) => Debug.Log("OneHandTap");
            GestureInputListener.Instance.TwoHandTap += (GestureInputEventArgs eventData) => Debug.Log("TwoHandTap");
            GestureInputListener.Instance.OneHandDoubleTap += (GestureInputEventArgs eventData) => Debug.Log("OneHandDoubleTap");
            GestureInputListener.Instance.TwoHandDoubleTap += (GestureInputEventArgs eventData) => Debug.Log("TwoHandDoubleTap");
            GestureInputListener.Instance.OneHandManipStart += (GestureInputEventArgs eventData) => Debug.Log("OneHandManipulationStart");
            GestureInputListener.Instance.TwoHandManipStart += (GestureInputEventArgs eventData) => Debug.Log("TwoHandManipulationStart");
            GestureInputListener.Instance.ManipulationEnd += (GestureInputEventArgs eventData) => Debug.Log("ManipulationEnd");
        }
    }
}