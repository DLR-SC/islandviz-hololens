using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction.Tasking.Task
{
    public class TaskResetPane : DiscreteSpeechInteractionTask
    {
        public override IEnumerator Perform(SpeechInteractionEventArgs eventArgs)
        {
            GameObject.Find("ContentPane").transform.position = GazeManager.Instance.GazeOrigin;
            GameObject.Find("ContentPane").transform.position += GazeManager.Instance.GazeTransform.forward * 2f;
            GameObject.Find("ContentPane").transform.position -= GazeManager.Instance.GazeTransform.up * 0.5f;
            yield return null;
        }
    }
}