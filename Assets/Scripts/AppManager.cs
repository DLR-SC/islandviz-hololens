using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Mapping;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HoloIslandVis
{
    public class AppManager : SingletonComponent<AppManager>
    {
        private bool _isScanning;
        private bool _isUpdating;

        // Use this for initialization
        void Start()
        {
            _isUpdating = false;
            _isScanning = false;

            SpeechInputListener.Instance.SpeechResponse += (EventArgs eventData) => Debug.Log("speechEvent");

            initScene();
            //inputListenerDebug();
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