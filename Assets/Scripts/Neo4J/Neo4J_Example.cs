using System.Collections.Generic;
using UnityEngine;
using Neo4j.Driver.V1;
using System.Linq;
using System.Collections;
using HoloIslandVis.Utilities;

namespace Neo4JDriver
{
    // This example is using the movie example neo4j database.
    public class Neo4J_Example : MonoBehaviour
    {
        Neo4J database;

        // Start is called before the first frame update
        void Start()
        {
            database = new Neo4J("bolt://localhost:7687", "neo4j", "123");
            StartCoroutine(ConnectionStatus());
        }

        public void Query()
        {
            IStatementResult result = database.Transaction("MATCH (n:Bundle) return COUNT(n)");
            string release = result.Single()[0].As<string>();
            Debug.Log(release);
            DebugLog.Instance.SetText(release);
        }

        public IEnumerator ConnectionStatus()
        {
            while (true)
            {
                Debug.Log(database.GetCurrentStatus());
                DebugLog.Instance.SetText(database.GetCurrentStatus().ToString());
                Query();
                yield return new WaitForSeconds(2.0f);
            }
        }

        // An example with a Cypher statement returning one release date.
        public void SingleItemExample()
        {
            IStatementResult result = database.Transaction("MATCH (cloudAtlas {title: \"Cloud Atlas\"}) " +
                "RETURN cloudAtlas.released");
            string release = result.Single()[0].As<string>();
            Debug.Log(release);
        }

        // An example with a Cypher statement returning multiple movie titles.
        public void MultipleItemsExample()
        {
            IStatementResult result = database.Transaction(
                "MATCH (nineties:Movie) WHERE nineties.released >= 1990 AND nineties.released < 2000 " +
                "RETURN nineties.title");

            // Convert the IStatementResult to a IRecord List.
            var results = result.ToList();

            // Debug 
            Debug.Log("Neo4F_Example: " + results.Count + " results were found!");

            // Iterates through all results.
            for (int i = 0; i < results.Count; i++)
            {
                // Print the result to the console.
                Debug.Log(results[i].Values.As<List<string>>()[0]);
            }
        }

        // An example with a Cypher statement returning multiple person names and movie titles.
        public void MultipleItemsExample2()
        {
            IStatementResult result = database.Transaction(
                "MATCH (tom:Person {name: \"Tom Hanks\"})-[:ACTED_IN]->(tomHanksMovies) " +
                "RETURN tom.name,tomHanksMovies.title");

            // Convert the IStatementResult to a IRecord List.
            var results = result.ToList();

            // Debug 
            Debug.Log("Neo4F_Example: " + results.Count + " results were found!");

            for (int i = 0; i < results.Count; i++)
            {
                foreach (var item in results[i].Values.As<List<string>>())
                {
                    // Print the result to the console.
                    Debug.Log(item);
                }
            }
        }

    }
}