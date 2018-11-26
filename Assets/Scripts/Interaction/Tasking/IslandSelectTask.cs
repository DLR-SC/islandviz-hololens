using HoloIslandVis.Automaton;
using HoloIslandVis.Component.UI;
using HoloIslandVis.Input;
using HoloIslandVis.Interaction;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HoloIslandVis.Interaction.Tasking
{
    public class IslandSelectTask : DiscreteInteractionTask
    {
        public override void Perform(InputEventArgs eventArgs, Command command)
        {
            foreach (GameObject highlight in RuntimeCache.Instance.Highlights)
            {
                highlight.GetComponent<MeshRenderer>().enabled = false;
                highlight.SetActive(false);
            }

            GameObject currentFocus = RuntimeCache.Instance.CurrentFocus;

            if (currentFocus == null)
                return; 

            GameObject infoPanel = UserInterface.Instance.Panel;
            RuntimeCache cache = RuntimeCache.Instance;
            //Debug.Log(currentFocus.name + "/Highlight");
            Island island = currentFocus.GetComponent<Island>();
            if (island == null)
                return;

            Transform[] transforms = currentFocus.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform trans in transforms)
            {
                if (trans.gameObject.name.Contains("_Highlight"))
                {
                    trans.gameObject.SetActive(true);
                    trans.gameObject.GetComponent<MeshRenderer>().enabled = true;
                    infoPanel.SetActive(true);
                    infoPanel.transform.Find("Canvas").Find("Title").GetComponent<Text>().text = currentFocus.name;
                    infoPanel.transform.Find("Canvas").Find("Maintext").GetComponent<Text>().text =
                        "This Bundle contains:\n"
                        + island.Regions.Count + " packages\n"
                        + island.CartographicIsland.Bundle.CompilationUnitCount + " compilation units\n"
                        + island.CartographicIsland.Bundle.ExportedPackages.Count + " exports\n"
                        + island.CartographicIsland.Bundle.ImportedPackages.Count + " imports\n"
                        + island.CartographicIsland.Bundle.ServiceComponents.Count + " services\n";
                }
            }
        }
    }
}