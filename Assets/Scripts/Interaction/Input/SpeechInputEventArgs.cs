using System.Collections;
using System.Collections.Generic;
using HoloIslandVis.Interaction.Input;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using static HoloIslandVis.Interaction.Input.RasaResponse;

namespace HoloIslandVis.Interaction.Input
{
    public class SpeechInputEventArgs : InputEventArgs
    {
        public List<Entity> entities;
        public string intention;
        public double intentionConfidence;
        public SpeechInputEventArgs(RasaResponse response)
        {   
            this.entities = response.Entities;
            this.intention = response.IntentName;
            this.intentionConfidence = response.IntentConfidence;
        }
    }
}