using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Core
{
    public struct IDPair
    {
        public int ID_ConnectionPoint_A;
        public int ID_ConnectionPoint_B;

        public IDPair(int a, int b)
        {
            ID_ConnectionPoint_A = a;
            ID_ConnectionPoint_B = b;
        }
    }

    //Bidirectional ConnectionPool for storing all of the Dependency arrows and Service connections
    public class ConnectionPool : MonoBehaviour
    {
        public Dictionary<IDPair, GameObject> Pool { get; private set; }

        void Start()
        {
            Pool = new Dictionary<IDPair, GameObject>();
        }

        public GameObject GetConnection(IDPair key)
        {
            GameObject result;

            bool check = Pool.TryGetValue(key, out result);

            if (check == false)
            {
                IDPair reverseConnection = new IDPair(key.ID_ConnectionPoint_B, key.ID_ConnectionPoint_A);
                check = Pool.TryGetValue(reverseConnection, out result);
            }
            return result;
        }

        public void AddConnection(IDPair key, GameObject connection)
        {
            bool check = Pool.ContainsKey(key);
            IDPair reverseConnection = new IDPair(key.ID_ConnectionPoint_B, key.ID_ConnectionPoint_A);
            bool reverseCheck = Pool.ContainsKey(reverseConnection);

            if (check == false && reverseCheck == false)
                Pool.Add(key, connection);
        }
    }
}
