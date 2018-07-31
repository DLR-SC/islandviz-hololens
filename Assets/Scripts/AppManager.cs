using HoloIslandVis.Mapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis
{
    public class AppManager : SingletonComponent<AppManager>
    {

        // Use this for initialization
        void Start()
        {
            SpatialScan.Instance.RequestBeginScanning();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}