using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System;
using UnityEngine;

using UnityEngine.UI;
using HoloIslandVis.Core;
using HoloIslandVis.Interaction;
using System.Collections.Generic;
using HoloIslandVis.Controller;

namespace HoloIslandVis.Sharing
{
    public class AppStateSynchronizer : SingletonComponent<AppStateSynchronizer>
    {
        public AppConfig AppConfig;
        public SyncManager SyncManager;

        [SyncData]
        public SyncAppStateModel SyncAppStateModel;

        public Observable<string> Command;
        public Observable<string> Focused;
        public Observable<string> Selected;

        private Dictionary<string, Action<SyncString, string>> _remoteChangeEvents;
        private Dictionary<long, SyncPrimitive> _primitveMap;

        protected override void Awake()
        {
            base.Awake();
            AppConfig = GameObject.Find("AppConfig").GetComponent<AppConfig>();
            _primitveMap = new Dictionary<long, SyncPrimitive>();

            Command = new Observable<string>("");
            Focused = new Observable<string>("");
            Selected = new Observable<string>("");

            _remoteChangeEvents = new Dictionary<string, Action<SyncString, string>>();
            _remoteChangeEvents.Add("COMMAND",    (SyncString syncString, string value) => OnRemoteCommandChange(syncString, value));
            _remoteChangeEvents.Add("FOCUSED",    (SyncString syncString, string value) => OnRemoteFocusedChange(syncString, value));
            _remoteChangeEvents.Add("DEFOCUSED",  (SyncString syncString, string value) => OnRemoteDefocusedChange(syncString, value));

            if (SharingStage.Instance != null && SharingStage.Instance.IsConnected)
            {
                InitSyncModel();
                return;
            }

            SyncManager.SyncManagerStarted += OnSyncManagerStarted;
        }

        private void Update()
        {
            
        }

        private void OnSyncManagerStarted()
        {
            if (SharingStage.Instance.IsConnected)
            {
                InitSyncModel();
                return;
            }

            SharingStage.Instance.SharingManagerConnected += InitSyncModel;
        }

        private void InitSyncModel(object sender = null, EventArgs eventArgs = null)
        {
            SharingStage.Instance.SharingManagerConnected -= InitSyncModel;
            SyncAppStateModel = new SyncAppStateModel(gameObject.name);

            if(!AppConfig.IsServerInstance)
                SyncAppStateModel.SyncStringChanged += OnRemoteStringChange;

            if (AppConfig.IsServerInstance)
            {
                Command.ValueChanged += OnCommandChange;
                Focused.ValueChanged += OnFocusedChange;
            }

            SyncObject root = SharingStage.Instance.Root;

            if (root.Element != null)
            {
                OnSyncObjectInitialized(root);
                return;
            }

            root.InitializationComplete += OnSyncObjectInitialized;
        }

        private void OnSyncObjectInitialized(SyncObject syncObj)
        {
            Debug.Log("[SyncSystem] Initializing " + gameObject.name);
            syncObj.InitializationComplete -= OnSyncObjectInitialized;

            Element remoteModelElement = syncObj.Element.GetElement(SyncAppStateModel.FieldName);

            if (remoteModelElement != null)
            {
                SyncAppStateModel.AddFromRemote(remoteModelElement);

                Element remoteSyncCommandElement = SyncAppStateModel.Element.GetElement("SyncCommand");
                Element remoteSyncFocusedElement = SyncAppStateModel.Element.GetElement("SyncFocused");

                SyncPrimitive[] modelPrimitivies = SyncAppStateModel.GetChildren();
                modelPrimitivies[0].AddFromRemote(remoteSyncCommandElement);
                modelPrimitivies[1].AddFromRemote(remoteSyncFocusedElement);

                SyncAppStateModel.AddRemotePrimitive(remoteSyncCommandElement.GetGUID(), SyncAppStateModel.SyncCommand);
                SyncAppStateModel.AddRemotePrimitive(remoteSyncFocusedElement.GetGUID(), SyncAppStateModel.SyncFocused);

            } else SyncAppStateModel.InitializeLocal(syncObj.Element);
        }

        private void OnCommandChange(string value)
            => SyncAppStateModel.SyncCommand.Value = value;

        private void OnFocusedChange(string value)
            => SyncAppStateModel.SyncFocused.Value = value;

        private void OnSelectedChange(string value)
            => SyncAppStateModel.SyncSelected.Value = value;

        private void OnRemoteStringChange(SyncString syncString)
        {
            string[] token = syncString.Value.Split(new char[] { ' ' }, 2);
            _remoteChangeEvents[token[0]].Invoke(syncString, token[1]);
        }

        private void OnRemoteCommandChange(SyncString syncString, string value)
        {
            Debug.Log(gameObject.name + ": Remote COMMAND received!");
            string[] token = value.Split(' ');

            Command command = GetRemoteCommand(token);
            GestureInteractionEventArgs eventArgs = GetRemoteEventArgs(token);
            StartCoroutine(StateManager.Instance.IssueCommand(eventArgs, command));
        }

        private void OnRemoteFocusedChange(SyncString syncString, string value)
        {
            Debug.Log(gameObject.name + ": Remote FOCUSED change received!");

            GameObject target = GameObject.Find(value);
            var interactable = target.GetComponent<Interactable>();

            Context context = ContextManager.Instance.SafeContext;

            if (context.Selected == interactable)
                return;

            if (context.Focused == interactable)
                return;

            if (interactable.Highlight != null)
                interactable.Highlight.gameObject.SetActive(true);
        }

        private void OnRemoteDefocusedChange(SyncString syncString, string value)
        {
            Debug.Log(gameObject.name + ": Remote DEFOCUSED change received!");

            GameObject target = GameObject.Find(value);
            var interactable = target.GetComponent<Interactable>();

            Context context = ContextManager.Instance.SafeContext;

            if (context.Focused == interactable)
                return;

            if (interactable.Highlight != null)
                interactable.Highlight.gameObject.SetActive(false);
        }

        private Command GetRemoteCommand(string[] token)
        {
            Command command = new Command();

            command.Gesture = (GestureType)Enum.Parse(typeof(GestureType), token[0]);
            command.Keyword = (KeywordType)Enum.Parse(typeof(KeywordType), token[1]);
            command.Focused = (InteractableType)Enum.Parse(typeof(InteractableType), token[2]);
            command.Selected = (InteractableType)Enum.Parse(typeof(InteractableType), token[3]);
            command.Item = (StaticItem)Enum.Parse(typeof(StaticItem), token[4]);

            return command;
        }

        private GestureInteractionEventArgs GetRemoteEventArgs(string[] token)
        {
            GestureInteractionEventArgs eventArgs = new GestureInteractionEventArgs();
            int tokenCounter = 4;

            eventArgs.IsTwoHanded = bool.Parse(token[++tokenCounter]);

            float x = float.Parse(token[++tokenCounter]);
            float y = float.Parse(token[++tokenCounter]);
            float z = float.Parse(token[++tokenCounter]);
            eventArgs.HandOnePos = new Vector3(x, y, z);

            if (eventArgs.IsTwoHanded)
            {
                x = float.Parse(token[++tokenCounter]);
                y = float.Parse(token[++tokenCounter]);
                z = float.Parse(token[++tokenCounter]);
                eventArgs.HandTwoPos = new Vector3(x, y, z);
            }

            eventArgs.Gesture = (GestureType)Enum.Parse(typeof(GestureType), token[++tokenCounter]);

            string focusedName = token[++tokenCounter].Replace('_', ' ');
            string selectedName = token[++tokenCounter].Replace('_', ' ');
            var focusedInteractable = GameObject.Find(focusedName).GetComponent<Interactable>();
            var selectedInteractable = GameObject.Find(selectedName).GetComponent<Interactable>();
            eventArgs.Focused = focusedInteractable;
            eventArgs.Selected = selectedInteractable;

            return eventArgs;
        }
    }
}
