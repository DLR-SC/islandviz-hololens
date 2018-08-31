using HoloIslandVis.Component.UI;
using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static HoloIslandVis.Interaction.Input.RasaResponse;

public class FindEntitiesTask : DiscreteInteractionTask
{
    public List<GameObject> IslandGameObjects;
    private TextToSpeech tts;
    private readonly float speed = 0.2f;

    public GameObject FoundObject {
        get; private set;
    }

    public override void Perform(InputEventArgs eventArgs)
    {
        if(tts == null)
            tts = RuntimeCache.Instance.ContentSurface.AddComponent<TextToSpeech>();

        if (IslandGameObjects == null)
            IslandGameObjects = RuntimeCache.Instance.IslandGameObjects;

        Debug.Log("FindEntites.Perform()");
        if (IslandGameObjects == null)
            IslandGameObjects = RuntimeCache.Instance.IslandGameObjects;

        FoundObject = null;
        SpeechInputEventArgs siea = (SpeechInputEventArgs) eventArgs;

        Entity type;
        Entity unitName;

        type = siea.entities.Find(entity => entity.EntityType.Equals("entity_type"));
        unitName = siea.entities.Find(entity => entity.EntityType.Equals("unit_name"));

        if (type != null && unitName != null)
        {
            switch (type.EntityValue)
            {
                case "building":
                    findBuilding(unitName.EntityValue);
                    break;
                case "island":
                    findIsland(unitName.EntityValue);
                    break;
                case "region":
                    findRegion(unitName.EntityValue);
                    break;
                default:
                    Debug.Log("No known Type detected");
                    break;
            }
        }
        else
        {
            Debug.Log("couldnt parse your utterance");
        }

    }


    private void findBuilding(string buildingName)
    {
        FoundObject = IslandGameObjects.Find(island => island.GetComponent<Island>().Regions.Find(region => region.Buildings.Find(building => building.CompilationUnit.Name.ToLower().Contains(buildingName))));
        Debug.Log("found a Building: " + FoundObject.name + "[" + FoundObject.tag + "]");
    }

    private void findIsland(string islandName)
    {
        Debug.Log("Looking for Bundle/Island");
        FoundObject = IslandGameObjects.Find(island => island.GetComponent<Island>().CartographicIsland.Bundle.Name.ToLower().Contains(islandName));
        if (FoundObject != null)
        {
            Vector3 source = FoundObject.transform.position;
            Vector3 visualizationContainerPosition = RuntimeCache.Instance.VisualizationContainer.transform.position;
            Vector3 vectorToIsland = source - RuntimeCache.Instance.ContentSurface.transform.position;
            Vector3 target = visualizationContainerPosition - vectorToIsland;

            new Task(() =>
            {
                int loopcount = 0;
                bool dispatched = false;
                while (Vector3.Distance(target, visualizationContainerPosition) > 0.1f)
                {
                    loopcount++;
                    if (dispatched)
                    {
                        Task.Delay(50);
                        continue;
                    }

                    dispatched = true;
                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        UserInterface.Instance.ParsingProgressText.GetComponent<TextMesh>().text = "Loopcount: " + loopcount;
                        source = FoundObject.transform.position;
                        visualizationContainerPosition = RuntimeCache.Instance.VisualizationContainer.transform.position;
                        vectorToIsland = source - RuntimeCache.Instance.ContentSurface.transform.position;
                        target = visualizationContainerPosition - vectorToIsland;
                        RuntimeCache.Instance.VisualizationContainer.transform.position = Vector3.Lerp(RuntimeCache.Instance.VisualizationContainer.transform.position, new Vector3(target.x, RuntimeCache.Instance.VisualizationContainer.transform.position.y, target.z), speed);
                        dispatched = false;
                    });
                }
            }).Start();
        }
        else
            tts.StartSpeaking("Nothing found for this name");
    }

    private void findRegion(string regionName)
    {
        FoundObject = IslandGameObjects.Find(island => island.GetComponent<Island>().Regions.Find(region => region.Package.Name.ToLower().Contains(regionName)));
        Debug.Log("found a Region: " + FoundObject.name + "[" + FoundObject.tag + "]");
    }
}
