using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction;
using HoloIslandVis.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Mapping;
using HoloIslandVis.OSGiParser;
//using HoloIslandVis.Sharing;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.UX.ToolTips;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using GestureType = HoloIslandVis.Automaton.GestureType;

namespace HoloIslandVis
{
    public class AppManager : SingletonComponent<AppManager>
    {
        private string _filepath;
        private bool _isScanning;
        private bool _isUpdating;

        // Use this for initialization
        void Start()
        {
            RuntimeCache cache = RuntimeCache.Instance;
            _isUpdating = false;
            _isScanning = false;

            _filepath = Path.Combine(Application.streamingAssetsPath, "rce_lite.model");
            //_filepath = Path.Combine(Application.streamingAssetsPath, "rce_23_05_2017.model");

            new Task(() => cache.BuildSceneFromFile(_filepath)).Start();
            IslandDockBuilder.Instance.ConstructionCompleted += () => setupStateMachine();
            initSceneNoScan();
        }

        public void SendChange(SyncTransform transf)
        {
            transf.Position.Value = new Vector3(UnityEngine.Random.value,0,0);
            Debug.Log("Message sent.");
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

        public void initSceneNoScan()
        {
            UserInterface.Instance.ContentSurface.SetActive(true);

            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventArgs) =>
            {
                if (_isUpdating)
                {
                    UserInterface.Instance.ScanInstructionText.SetActive(false);
                    _isUpdating = false;
                }
            };

            new Task(async () =>
            {
                _isUpdating = true;
                while (_isUpdating)
                {
                    await Task.Delay(25);
                    UnityMainThreadDispatcher.Instance.Enqueue(() => {
                        if (!UserInterface.Instance.ContentSurface.activeInHierarchy)
                            UserInterface.Instance.ContentSurface.SetActive(true);

                        UserInterface.Instance.ContentSurface.transform.position =
                        Vector3.Lerp(UserInterface.Instance.ContentSurface.transform.position, GazeManager.Instance.HitPosition, 0.2f);
                        UserInterface.Instance.ContentSurface.transform.up =
                        Vector3.Lerp(UserInterface.Instance.ContentSurface.transform.up, GazeManager.Instance.HitNormal, 0.5f);
                    });
                }

                UnityMainThreadDispatcher.Instance.Enqueue(() => {
                    UserInterface.Instance.ContentSurface.layer = LayerMask.NameToLayer("Default");
                    GameObject.Find("Water").layer = LayerMask.NameToLayer("Default");
                    GameObject.Find("Glow").layer = LayerMask.NameToLayer("Default");
                    GameObject.Find("SpatialUnderstanding").SetActive(false);
                });
            }).Start();
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

            Command commandImportDockSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ImportDock);
            Command commandExportDockSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ExportDock);
            Command commandAllDockSelect = new Command(GestureType.OneHandDoubleTap, KeywordType.Invariant, InteractableType.Invariant);

            Command commandSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Island);
            Command commandDeselect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.None);
            Command commandSurfaceDrag = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command commandSurfaceZoom = new Command(GestureType.TwoHandManipStart, KeywordType.Invariant, InteractableType.Invariant);

            ShowArrowTask showArrowTask = new ShowArrowTask();
            ShowAllArrowsTask showAllArrowsTask = new ShowAllArrowsTask();

            IslandSelectTask islandSelectTask = new IslandSelectTask();
            IslandDeselectTask islandDeselectTask = new IslandDeselectTask();
            SurfaceDragTask surfaceDragTask = new SurfaceDragTask();
            SurfaceZoomTask surfaceZoomTask = new SurfaceZoomTask();

            testState.AddInteractionTask(commandImportDockSelect, showArrowTask);
            testState.AddInteractionTask(commandExportDockSelect, showArrowTask);
            testState.AddInteractionTask(commandAllDockSelect, showAllArrowsTask);

            testState.AddInteractionTask(commandSelect, islandSelectTask);
            testState.AddInteractionTask(commandDeselect, islandDeselectTask);
            testState.AddInteractionTask(commandSurfaceDrag, surfaceDragTask);
            testState.AddInteractionTask(commandSurfaceZoom, surfaceZoomTask);

            stateMachine.AddState(testState);
            stateMachine.Init(testState);

            //SharingClient.Instance.Init(stateMachine);
        }

        public void inputListenerDebug()
        {
            GestureInputListener.Instance.OneHandTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "OneHandTap";
            GestureInputListener.Instance.TwoHandTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TwoHandTap";
            GestureInputListener.Instance.OneHandDoubleTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "OneHandDoubleTap";
            GestureInputListener.Instance.TwoHandDoubleTap += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TwoHandDoubleTap";
            GestureInputListener.Instance.OneHandManipStart += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "OneHandManipulationStart";
            GestureInputListener.Instance.TwoHandManipStart += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "TwoHandManipulationStart";
            GestureInputListener.Instance.ManipulationUpdate += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ManipulationUpdate";
            GestureInputListener.Instance.ManipulationEnd += (GestureInputEventArgs eventData) => UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ManipulationEnd";
        }
    }
}