using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using static HoloIslandVis.Input.RasaResponse;

namespace HoloIslandVis.Input
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