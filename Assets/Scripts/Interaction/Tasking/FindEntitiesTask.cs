using HoloIslandVis.Interaction.Input;
using HoloIslandVis.Interaction.Tasking;
using HoloIslandVis.Utility;
using HoloIslandVis.Visualization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HoloIslandVis.Interaction.Input.RasaResponse;

public class FindEntitiesTask : DiscreteInteractionTask
{
    public List<GameObject> IslandGameObjects;
    public GameObject FoundObject {
        get; private set;
    }

    public override void Perform(InputEventArgs eventArgs)
    {

        if (IslandGameObjects == null)
            IslandGameObjects = RuntimeCache.Instance.IslandGameObjects;

        FoundObject = null;
        SpeechInputEventArgs siea = (SpeechInputEventArgs) eventArgs;

        string type;
        string unitName;

        type = siea.entities.Find(entity => entity.EntityType.Equals("entity_type")).EntityValue;
        unitName = siea.entities.Find(entity => entity.EntityType.Equals("unit_name")).EntityValue;

        switch (type)
        {
            case "class":
                findBuilding(unitName);
                break;
            case "bundle":
                findIsland(unitName);
                break;
            case "package":
                findRegion(unitName);
                break;
            default:
                Debug.Log("No known Type detected");
                break;
        }

    }


    private void findBuilding(string buildingName)
    {
        FoundObject = IslandGameObjects.Find(island => island.GetComponent<Island>().Regions.Find(region => region.Buildings.Find(building => building.CompilationUnit.Name.Equals(buildingName))));
        Debug.Log("found a Building: " + FoundObject.name + "[" + FoundObject.tag + "]");
    }

    private void findIsland(string islandName)
    {
        FoundObject = IslandGameObjects.Find(island => island.GetComponent<Island>().CartographicIsland.Bundle.Name.Equals(islandName));
        Debug.Log("found a Island: " + FoundObject.name + "[" + FoundObject.tag + "]");
    }

    private void findRegion(string regionName)
    {
        FoundObject = IslandGameObjects.Find(island => island.GetComponent<Island>().Regions.Find(region => region.Package.Name.Equals(regionName)));
        Debug.Log("found a Region: " + FoundObject.name + "[" + FoundObject.tag + "]");
    }


    // Use this for initialization
    void Start () {
        if (IslandGameObjects == null)
            IslandGameObjects = RuntimeCache.Instance.IslandGameObjects;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
