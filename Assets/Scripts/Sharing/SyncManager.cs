using HoloIslandVis.Core;
using HoloToolkit.Sharing;
using UnityEngine;

public class SyncManager : SingletonComponent<SyncManager>
{
    public delegate void SyncManagerStartHandler();
    public event SyncManagerStartHandler SyncManagerStarted = delegate { };

    public SharingStage SharingStage;
    public AppConfig AppConfig;

    public bool SharingStarted { get; private set; }

    public void StartSharing()
    {
        gameObject.SetActive(true);
        SharingStage.gameObject.SetActive(true);
        SharingStarted = true;
        SyncManagerStarted();
    }

    public void SetServerEndpoint(string address, int port)
    {
        SharingStage.ServerAddress = address;
        SharingStage.ServerPort = port;
    }
}
