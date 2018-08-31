using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Utility;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public class ContentSurfaceZoom : ContinuousInteractionTask
    {
        private bool _hasStarted;
        private List<IInputSource> _inputSources;
        private List<uint> _sourceId;

        private Vector3 _startScale;
        private float _startDistance;

        public ContentSurfaceZoom()
        {
            _inputSources = new List<IInputSource>();
            _sourceId = new List<uint>();

            _startDistance = 0;
        }

        public override void StartInteraction(GestureInputEventArgs eventArgs)
        {
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ZoomStart";
            _startScale = RuntimeCache.Instance.VisualizationContainer.transform.localScale;
            _hasStarted = true;

            for(int i = 0; i < eventArgs.InputSources.Length; i++)
            {
                _inputSources.Add(eventArgs.InputSources[i]);
                _sourceId.Add(eventArgs.SourceIds[i]);
            }

            Vector3 handOnePos;
            Vector3 handTwoPos;

            if (_inputSources[0].TryGetGripPosition(_sourceId[0], out handOnePos) &&
                    _inputSources[1].TryGetGripPosition(_sourceId[1], out handTwoPos))
            {
                _startDistance = Vector3.Distance(handOnePos, handTwoPos);
            }

            

        }

        public override void UpdateInteraction(GestureInputEventArgs eventArgs)
        {
            Vector3 handOnePos;
            Vector3 handTwoPos;

            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ZoomUpdate: " + _inputSources.Count;

            if (_hasStarted && _inputSources[0].TryGetGripPosition(_sourceId[0], out handOnePos) &&
                        _inputSources[1].TryGetGripPosition(_sourceId[1], out handTwoPos))
            {
                float scalingFactor = Vector3.Distance(handOnePos, handTwoPos)/_startDistance;
                UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Scaling factor: " + scalingFactor;
                RuntimeCache.Instance.VisualizationContainer.transform.localScale = _startScale * scalingFactor;
            }
        }

        public override void EndInteraction(GestureInputEventArgs eventArgs)
        {
            UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "ZoomEnd";
            _inputSources.Clear();
            _sourceId.Clear();
            _hasStarted = false;
        }
    }
}
