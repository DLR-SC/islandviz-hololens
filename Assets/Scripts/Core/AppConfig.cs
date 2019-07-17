using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Core
{
    public class AppConfig : MonoBehaviour
    {
        public bool SharingEnabled;
        public int SharingServerPort;
        public string SharingServerAddress;

        public bool IsServerInstance;
    }
}
