using System.Collections.Generic;

namespace HoloIslandVis.Interaction.Input
{
    internal class RasaResponse
    {

        public RasaResponse(string apiRequest)
        {
            JSONObject jobj = JSONObject.Create(apiRequest);

            IntentConfidence = jobj.GetField("tracker").GetField("latest_message").GetField("intent").GetField("confidence").n;
            IntentName = jobj.GetField("tracker").GetField("latest_message").GetField("intent").GetField("name").str;
            JSONObject arr = jobj.GetField("tracker").GetField("latest_message").GetField("entities");
            Entities = new List<Entity>();
            foreach (JSONObject element in arr)
            {
                Entities.Add(new Entity(element));
            }
        }

        public double IntentConfidence
        {
            get; private set;
        }

        public string IntentName
        {
            get; private set;
        }

        public List<Entity> Entities;

        public class Entity
        {

            public Entity(JSONObject element)
            {
                Confidence = element.GetField("confidence").n;
                EntityType = element.GetField("entity").str;
                EntityValue = element.GetField("value").str;
            }

            public float Confidence
            {
                get; private set;
            }
            public string EntityType
            {
                get; private set;
            }
            public string EntityValue
            {
                get; private set;
            }
        }

    }


}