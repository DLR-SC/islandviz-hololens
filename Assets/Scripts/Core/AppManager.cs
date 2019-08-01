using HoloIslandVis.Controller;
using HoloIslandVis.Core.Builders;
using HoloIslandVis.Interaction;
using HoloIslandVis.Interaction.Tasking.Task;
using HoloIslandVis.Model.OSGi;
using HoloIslandVis.UI;
using HoloIslandVis.UI.Component;
using HoloIslandVis.Utilities;
using HoloToolkit.Sharing;
using HoloToolkit.Unity.InputModule;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis.Core
{
    public class AppManager : SingletonComponent<AppManager>
    {
        public AppConfig AppConfig;
        public SyncManager SyncManager;
        public SingletonComponentInitializer Initializer;

        protected override void Awake()
        {
            AppConfig.SharingEnabled = false;
            AppConfig.IsServerInstance = false;

            AppConfig.SharingServerAddress = "192.168.0.99";
            AppConfig.SharingServerPort = 20602;

            Initializer.AllInitialized += StartApplication;

            Initializer.AddComponent(UIManager.Instance);
            Initializer.AddComponent(StateManager.Instance);
            Initializer.AddComponent(VisualizationLoader.Instance);
            Initializer.AddComponent(IslandStructureBuilder.Instance);
            Initializer.AddComponent(UnityMainThreadDispatcher.Instance);
            Initializer.Initialize();
        }

        public void StartApplication()
        {
            UIManager.Instance.Activate(UIElement.SettingsPanel);
        }

        public void LoadVisualization()
        {

            AppConfig.SharingEnabled = GameObject.Find("SharingEnabledToggle").GetComponent<Toggle>().isOn;
            AppConfig.IsServerInstance = GameObject.Find("ServerInstanceToggle").GetComponent<Toggle>().isOn;

            var modelDropdown = GameObject.Find("ProjectModelDropdown").GetComponent<Dropdown>();
            string modelfile = modelDropdown.options[modelDropdown.value].text;

            UIManager.Instance.Deactivate(UIElement.SettingsPanel);
            UIManager.Instance.Activate(UIElement.ContentPane);
            UIManager.Instance.Deactivate(UIElement.InfoPanel);
            UIManager.Instance.Deactivate(UIElement.Toolbar);

            GameObject.Find("ContentPane").transform.position = GazeManager.Instance.GazeOrigin;
            GameObject.Find("ContentPane").transform.position += GazeManager.Instance.GazeTransform.forward*2f;
            GameObject.Find("ContentPane").transform.position -= GazeManager.Instance.GazeTransform.up*0.5f;

            string assetpath = Application.streamingAssetsPath;
            string filepath = Path.Combine(assetpath, modelfile);
            string filecontent = FileReader.Read(new Uri(filepath).AbsolutePath);

            Task.Factory.StartNew(() =>
            {
                JSONObject modeldata = JSONObject.Create(filecontent);
                OSGiProject project = OSGiProjectParser.Instance.Parse(modeldata);

                VisualizationLoader.Instance.VisualizationLoaded += OnVisualizationLoaded;
                VisualizationLoader.Instance.Load(project);
            });
        }

        private void OnVisualizationLoaded()
        {
            Debug.Log("Visualization built.");
            InitializeStateManager();
        }

        private void InitializeStateManager()
        {
            State state_settings        = new State("settings");
            State state_loading         = new State("loading");
            State state_setup           = new State("setup");
            State state_main            = new State("main");
            State state_inspectIsland   = new State("inspect island");
            State state_inspectRegion   = new State("inspect region");
            State state_inspectBuilding = new State("inspect building");

            State state_scenario_manager = new State("scenario manager");

            StateManager.Instance.AddState(state_settings);
            StateManager.Instance.AddState(state_loading);
            StateManager.Instance.AddState(state_setup);
            StateManager.Instance.AddState(state_main);
            StateManager.Instance.AddState(state_inspectIsland);
            StateManager.Instance.AddState(state_inspectRegion);
            StateManager.Instance.AddState(state_inspectBuilding);

            StateManager.Instance.AddState(state_scenario_manager);

            Init_StateSettings(state_settings, state_setup);
            //Init_StateLoading(state_loading, state_setup);

            Init_StateSetup(state_setup, state_main);
            Init_StateMain(state_main, state_setup, state_inspectIsland);
            /*Init_StateSetup(state_setup, state_scenario_manager);
            Init_StateScenario(state_scenario_manager, state_setup, state_main);
            Init_StateMain(state_main, state_scenario_manager, state_inspectIsland);*/

            Init_StateInspectIsland(state_inspectIsland, state_main, state_inspectRegion);
            Init_StateInspectRegion(state_inspectRegion, state_inspectIsland, state_inspectBuilding, state_main);
            Init_StateInspectBuilding(state_inspectBuilding, state_inspectRegion, state_inspectIsland, state_main);

            StateManager.Instance.Init(state_setup);
        }

        public void LoadScenario()
        {
            Debug.Log("Load Scenario");
            UIManager.Instance.Deactivate(UIElement.ScenarioPanel);
            UIManager.Instance.Activate(UIElement.StartScenarioPanel);
        }

        private void Init_StateScenario(State state_scenario_manager, State state_setup, State state_main)
        {
            Command command_startScenario = new Command(GestureType.OneHandTap, KeywordType.None, InteractableType.Widget, InteractableType.None, StaticItem.None);

            state_scenario_manager.AddOpenAction((State state) => UIManager.Instance.Activate(UIElement.ScenarioPanel));
            state_scenario_manager.AddCloseAction((State state) => UIManager.Instance.Deactivate(UIElement.StartScenarioPanel));
            state_scenario_manager.AddStateTransition(command_startScenario, state_main);
        }

        private void Init_StateSettings(State state_settings, State state_setup)
        {
            state_settings.AddOpenAction((State state) => UIManager.Instance.Activate(UIElement.SettingsPanel));
            state_settings.AddCloseAction((State state) => UIManager.Instance.Deactivate(UIElement.SettingsPanel));
        }

        private void Init_StateLoading(State state_loading, State state_setup)
        {
            state_loading.AddOpenAction((State state) => LoadVisualization());

        }

        private void Init_StateSetup(State state_setup, State state_main)
        {
            TaskDragPhysics task_dragPhysics    = new TaskDragPhysics();

            Command command_dragPhysics = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.ContentPane);
            Command command_setPane     = new Command(GestureType.OneHandTap, KeywordType.None, InteractableType.ContentPane, InteractableType.None, StaticItem.None);

            state_setup.AddInteractionTask(command_dragPhysics, task_dragPhysics);
            state_setup.AddStateTransition(command_setPane, state_main);

            state_setup.AddOpenAction((State state) => {
                UIManager.Instance.Activate(UIElement.BoundingBoxRig);
                GameObject.Find("ContentPaneProxy").transform.position = GameObject.Find("ContentPane").transform.position;
                }
            );
            state_setup.AddCloseAction((State state) => UIManager.Instance.Deactivate(UIElement.BoundingBoxRig));
        }

        private void Init_StateMain(State state_main, State state_init, State state_inspectIsland)
        {
            TaskDragProjected task_dragProjected        = new TaskDragProjected();
            TaskZoomProjected task_zoomProjected        = new TaskZoomProjected();
            TaskIslandSelect task_islandSelect          = new TaskIslandSelect();
            TaskIslandDeselect task_islandDeselect      = new TaskIslandDeselect();
            TaskShowDependencies task_showDependencies  = new TaskShowDependencies();
            TaskFitContent task_fitContent              = new TaskFitContent();
            TaskToggleDependency task_toggleDependency  = new TaskToggleDependency();
            TaskResetPane task_resetPane                = new TaskResetPane();
            TaskMoveDirection task_moveDirection        = new TaskMoveDirection();
            TaskZoom task_zoom                          = new TaskZoom();

            Command command_dragProjected       = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command command_zoomProjected       = new Command(GestureType.TwoHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command command_islandSelectGesture = new Command(GestureType.OneHandTap, KeywordType.None, InteractableType.Bundle);
            Command command_exportSelectGesture = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ImportDock);
            Command command_importSelectGesture = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ExportDock);
            Command command_islandSelectSpeech  = new Command(GestureType.None, KeywordType.Select, InteractableType.Bundle);
            Command command_resetPane           = new Command(GestureType.None, KeywordType.Select, InteractableType.ContentPane);
            Command command_zoomIn              = new Command(GestureType.None, KeywordType.ZoomIn, InteractableType.Invariant);
            Command command_zoomOut             = new Command(GestureType.None, KeywordType.ZoomOut, InteractableType.Invariant);
            Command command_moveUp              = new Command(GestureType.None, KeywordType.MoveUp, InteractableType.Invariant);
            Command command_moveDown            = new Command(GestureType.None, KeywordType.MoveDown, InteractableType.Invariant);
            Command command_moveLeft            = new Command(GestureType.None, KeywordType.MoveLeft, InteractableType.Invariant);
            Command command_moveRight           = new Command(GestureType.None, KeywordType.MoveRight, InteractableType.Invariant);
            Command command_adjust              = new Command(StaticItem.Adjust);
            Command command_showDependencies    = new Command(StaticItem.Dependencies);
            Command command_fitContent          = new Command(StaticItem.Fit);

            state_main.AddInteractionTask(command_dragProjected, task_dragProjected);
            state_main.AddInteractionTask(command_zoomProjected, task_zoomProjected);
            state_main.AddInteractionTask(command_showDependencies, task_showDependencies);
            state_main.AddInteractionTask(command_fitContent, task_fitContent);
            state_main.AddInteractionTask(command_islandSelectGesture, task_islandSelect);
            state_main.AddInteractionTask(command_islandSelectSpeech, task_islandSelect);
            state_main.AddInteractionTask(command_importSelectGesture, task_toggleDependency);
            state_main.AddInteractionTask(command_exportSelectGesture, task_toggleDependency);
            state_main.AddInteractionTask(command_resetPane, task_resetPane);
            state_main.AddInteractionTask(command_zoomIn, task_zoom);
            state_main.AddInteractionTask(command_zoomOut, task_zoom);
            state_main.AddInteractionTask(command_moveUp, task_moveDirection);
            state_main.AddInteractionTask(command_moveDown, task_moveDirection);
            state_main.AddInteractionTask(command_moveLeft, task_moveDirection);
            state_main.AddInteractionTask(command_moveRight, task_moveDirection);

            state_main.AddStateTransition(command_adjust, state_init);
            state_main.AddStateTransition(command_islandSelectGesture, state_inspectIsland);
            state_main.AddStateTransition(command_islandSelectSpeech, state_inspectIsland);

            state_main.AddOpenAction((State state) => {
                UIManager.Instance.SetActiveButtons(StaticItem.Adjust, StaticItem.Panel, StaticItem.Fit, StaticItem.Dependencies);

                if (AppConfig.SharingEnabled && !SyncManager.SharingStarted)
                {
                    string address = AppConfig.SharingServerAddress;
                    int port = AppConfig.SharingServerPort;

                    SyncManager.SetServerEndpoint(address, port);
                    SyncManager.StartSharing();
                }

                UIManager.Instance.EnableToolbar(true);
            });
            state_main.AddOpenAction((State state) => Debug.Log("Main State"));

            state_main.AddCloseAction((State state) => UIManager.Instance.DisableToolbar(true));
        }

        private void Init_StateInspectIsland(State state_inspectIsland, State state_main, State state_inspectRegion)
        {
            TaskRegionSelect task_regionSelect = new TaskRegionSelect();
            TaskIslandDeselect task_islandDeselect = new TaskIslandDeselect();
            TaskRotateInspect task_rotateInspect = new TaskRotateInspect();
            TaskShowDependencies task_showDependencies = new TaskShowDependencies();
            TaskToggleDependency task_toggleDependency = new TaskToggleDependency();

            Command command_regionSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Package, InteractableType.Bundle, StaticItem.None);
            Command command_regionSelectSpeech = new Command(GestureType.None, KeywordType.Select, InteractableType.Package, InteractableType.Bundle, StaticItem.None);
            Command command_islandDeselectGesture1 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.None, InteractableType.Bundle, StaticItem.None);
            Command command_islandDeselectGesture2 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane, InteractableType.Bundle, StaticItem.None);
            Command command_islandDeselectGesture3 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Bundle, InteractableType.Bundle, StaticItem.None);
            Command command_islandDeselectSpeech = new Command(GestureType.None, KeywordType.Deselect, InteractableType.Invariant, InteractableType.Bundle, StaticItem.None);
            Command command_exportSelectGesture = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ImportDock);
            Command command_importSelectGesture = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ExportDock);
            Command command_rotateInspect = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command command_showDependencies = new Command(StaticItem.Dependencies);

            state_inspectIsland.AddInteractionTask(command_regionSelect, task_regionSelect);
            state_inspectIsland.AddInteractionTask(command_regionSelectSpeech, task_regionSelect);
            state_inspectIsland.AddInteractionTask(command_islandDeselectGesture1, task_islandDeselect);
            state_inspectIsland.AddInteractionTask(command_islandDeselectGesture2, task_islandDeselect);
            state_inspectIsland.AddInteractionTask(command_islandDeselectGesture3, task_islandDeselect);
            state_inspectIsland.AddInteractionTask(command_islandDeselectSpeech, task_islandDeselect);
            state_inspectIsland.AddInteractionTask(command_rotateInspect, task_rotateInspect);
            state_inspectIsland.AddInteractionTask(command_showDependencies, task_showDependencies);
            state_inspectIsland.AddInteractionTask(command_importSelectGesture, task_toggleDependency);
            state_inspectIsland.AddInteractionTask(command_exportSelectGesture, task_toggleDependency);

            state_inspectIsland.AddStateTransition(command_islandDeselectGesture1, state_main);
            state_inspectIsland.AddStateTransition(command_islandDeselectGesture2, state_main);
            state_inspectIsland.AddStateTransition(command_islandDeselectGesture3, state_main);
            state_inspectIsland.AddStateTransition(command_islandDeselectSpeech, state_main);
            state_inspectIsland.AddStateTransition(command_regionSelect, state_inspectRegion);
            state_inspectIsland.AddStateTransition(command_regionSelectSpeech, state_inspectRegion);

            state_inspectIsland.AddOpenAction((State state) => {
                UIManager.Instance.SetActiveButtons(StaticItem.Done, StaticItem.Panel, StaticItem.Dependencies);
                UIManager.Instance.EnableToolbar(true);
            });
            state_inspectIsland.AddOpenAction((State state) => Debug.Log("Inspect Island State"));
            state_inspectIsland.AddCloseAction((State state) => UIManager.Instance.DisableToolbar(true));
        }

        private void Init_StateInspectRegion(State state_inspectRegion, State state_inspectIsland, State state_inspectBuilding, State state_main)
        {
            TaskBuildingSelect task_buildingSelect = new TaskBuildingSelect();
            TaskRegionDeselect task_regionDeselect = new TaskRegionDeselect();
            TaskRegionSelect task_regionSelect = new TaskRegionSelect();
            TaskRegionSelect task_regionSelectSpeech = new TaskRegionSelect();
            TaskIslandDeselect task_islandDeselect = new TaskIslandDeselect();
            TaskRotateInspect task_rotateInspect = new TaskRotateInspect();
            TaskShowDependencies task_showDependencies = new TaskShowDependencies();

            Command command_buildingSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.CompilationUnit, InteractableType.Package, StaticItem.None);
            Command command_buildingSelectSpeech = new Command(GestureType.None, KeywordType.Select, InteractableType.CompilationUnit, InteractableType.Package, StaticItem.None);
            Command command_regionDeselect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Bundle, InteractableType.Package, StaticItem.None);
            Command command_regionDeselectSpeech = new Command(GestureType.None, KeywordType.Deselect, InteractableType.Bundle, InteractableType.Package, StaticItem.None);
            Command command_regionSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Package, InteractableType.Package, StaticItem.None);
            Command command_regionSelectSpeech = new Command(GestureType.None, KeywordType.Select, InteractableType.Package, InteractableType.Package, StaticItem.None);
            Command command_islandDeselect1 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane, InteractableType.Package, StaticItem.None);
            Command command_islandDeselect2 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.None, InteractableType.Package, StaticItem.None);
            Command command_islandDeselectSpeech1 = new Command(GestureType.None, KeywordType.Deselect, InteractableType.ContentPane, InteractableType.Package, StaticItem.None);
            Command command_islandDeselectSpeech2 = new Command(GestureType.None, KeywordType.Deselect, InteractableType.None, InteractableType.Package, StaticItem.None);
            Command command_rotateInspect = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command command_showDependencies = new Command(StaticItem.Dependencies);

            state_inspectRegion.AddInteractionTask(command_buildingSelect, task_buildingSelect);
            state_inspectRegion.AddInteractionTask(command_buildingSelectSpeech, task_buildingSelect);
            state_inspectRegion.AddInteractionTask(command_regionDeselect, task_regionDeselect);
            state_inspectRegion.AddInteractionTask(command_regionSelect, task_regionSelect);
            state_inspectRegion.AddInteractionTask(command_regionSelectSpeech, task_regionSelect);
            state_inspectRegion.AddInteractionTask(command_islandDeselect1, task_islandDeselect);
            state_inspectRegion.AddInteractionTask(command_islandDeselect2, task_islandDeselect);
            state_inspectRegion.AddInteractionTask(command_islandDeselectSpeech1, task_islandDeselect);
            state_inspectRegion.AddInteractionTask(command_islandDeselectSpeech2, task_islandDeselect);
            state_inspectRegion.AddInteractionTask(command_rotateInspect, task_rotateInspect);
            state_inspectRegion.AddInteractionTask(command_showDependencies, task_showDependencies);

            state_inspectRegion.AddStateTransition(command_buildingSelect, state_inspectBuilding);
            state_inspectRegion.AddStateTransition(command_buildingSelectSpeech, state_inspectBuilding);
            state_inspectRegion.AddStateTransition(command_regionDeselect, state_inspectIsland);
            state_inspectRegion.AddStateTransition(command_regionDeselectSpeech, state_inspectIsland);
            state_inspectRegion.AddStateTransition(command_islandDeselect1, state_main);
            state_inspectRegion.AddStateTransition(command_islandDeselect2, state_main);
            state_inspectRegion.AddStateTransition(command_islandDeselectSpeech1, state_main);
            state_inspectRegion.AddStateTransition(command_islandDeselectSpeech2, state_main);

            state_inspectRegion.AddOpenAction((State state) => {
                UIManager.Instance.SetActiveButtons(StaticItem.Done, StaticItem.Panel, StaticItem.Dependencies);
                UIManager.Instance.EnableToolbar(true);
            });
            state_inspectRegion.AddOpenAction((State state) => Debug.Log("Inspect Region State"));
            state_inspectRegion.AddCloseAction((State state) => UIManager.Instance.DisableToolbar(true));
        }

        private void Init_StateInspectBuilding(State state_inspectBuilding, State state_inspectRegion, State state_inspectIsland, State state_main)
        {
            TaskBuildingSelect task_buildingSelect = new TaskBuildingSelect();
            TaskBuildingDeselect task_buildingDeselect = new TaskBuildingDeselect();
            TaskRegionDeselect task_regionDeselect = new TaskRegionDeselect();
            TaskIslandDeselect task_islandDeselect = new TaskIslandDeselect();
            TaskBuildingSelect task_buildingSelectSpeech = new TaskBuildingSelect();
            TaskBuildingDeselect task_buildingDeselectSpeech = new TaskBuildingDeselect();
            TaskRegionDeselect task_regionDeselectSpeech = new TaskRegionDeselect();
            TaskIslandDeselect task_islandDeselectSpeech = new TaskIslandDeselect();
            TaskRotateInspect task_rotateInspect = new TaskRotateInspect();
            TaskShowDependencies task_showDependencies = new TaskShowDependencies();

            Command command_buildingSelect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.CompilationUnit, InteractableType.CompilationUnit, StaticItem.None);
            Command command_buildingDeselect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Package, InteractableType.CompilationUnit, StaticItem.None);
            Command command_regionDeselect = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.Bundle, InteractableType.CompilationUnit, StaticItem.None);
            Command command_islandDeselect1 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.ContentPane, InteractableType.CompilationUnit, StaticItem.None);
            Command command_islandDeselect2 = new Command(GestureType.OneHandTap, KeywordType.Invariant, InteractableType.None, InteractableType.CompilationUnit, StaticItem.None);
            Command command_buildingSelectSpeech = new Command(GestureType.None, KeywordType.Select, InteractableType.CompilationUnit, InteractableType.CompilationUnit, StaticItem.None);
            Command command_buildingDeselectSpeech = new Command(GestureType.None, KeywordType.Deselect, InteractableType.Package, InteractableType.CompilationUnit, StaticItem.None);
            Command command_regionDeselectSpeech = new Command(GestureType.None, KeywordType.Deselect, InteractableType.Bundle, InteractableType.CompilationUnit, StaticItem.None);
            Command command_islandDeselectSpeech1 = new Command(GestureType.None, KeywordType.Deselect, InteractableType.ContentPane, InteractableType.CompilationUnit, StaticItem.None);
            Command command_islandDeselectSpeech2 = new Command(GestureType.None, KeywordType.Deselect, InteractableType.None, InteractableType.CompilationUnit, StaticItem.None);
            Command command_rotateInspect = new Command(GestureType.OneHandManipStart, KeywordType.Invariant, InteractableType.Invariant);
            Command command_showDependencies = new Command(StaticItem.Dependencies);

            state_inspectBuilding.AddInteractionTask(command_buildingSelect, task_buildingSelect);
            state_inspectBuilding.AddInteractionTask(command_buildingDeselect, task_buildingDeselect);
            state_inspectBuilding.AddInteractionTask(command_regionDeselect, task_regionDeselect);
            state_inspectBuilding.AddInteractionTask(command_islandDeselect1, task_islandDeselect);
            state_inspectBuilding.AddInteractionTask(command_islandDeselect2, task_islandDeselect);
            state_inspectBuilding.AddInteractionTask(command_buildingSelectSpeech, task_buildingSelect);
            state_inspectBuilding.AddInteractionTask(command_buildingDeselectSpeech, task_buildingDeselect);
            state_inspectBuilding.AddInteractionTask(command_regionDeselectSpeech, task_regionDeselect);
            state_inspectBuilding.AddInteractionTask(command_islandDeselectSpeech1, task_islandDeselect);
            state_inspectBuilding.AddInteractionTask(command_islandDeselectSpeech2, task_islandDeselect);
            state_inspectBuilding.AddInteractionTask(command_rotateInspect, task_rotateInspect);
            state_inspectBuilding.AddInteractionTask(command_showDependencies, task_showDependencies);

            state_inspectBuilding.AddStateTransition(command_buildingDeselect, state_inspectRegion);
            state_inspectBuilding.AddStateTransition(command_regionDeselect, state_inspectIsland);
            state_inspectBuilding.AddStateTransition(command_islandDeselect1, state_main);
            state_inspectBuilding.AddStateTransition(command_islandDeselect2, state_main);
            state_inspectBuilding.AddStateTransition(command_buildingDeselectSpeech, state_inspectRegion);
            state_inspectBuilding.AddStateTransition(command_regionDeselectSpeech, state_inspectIsland);
            state_inspectBuilding.AddStateTransition(command_islandDeselectSpeech1, state_main);
            state_inspectBuilding.AddStateTransition(command_islandDeselectSpeech2, state_main);

            state_inspectBuilding.AddOpenAction((State state) => {
                UIManager.Instance.SetActiveButtons(StaticItem.Done, StaticItem.Panel, StaticItem.Dependencies);
                UIManager.Instance.EnableToolbar(true);
            });
            state_inspectBuilding.AddOpenAction((State state) => Debug.Log("Inspect Building State"));
            state_inspectBuilding.AddCloseAction((State state) => UIManager.Instance.DisableToolbar(true));
        }
    }
}
