using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Core
{
    public class AppConfig : MonoBehaviour
    {
        public bool SharingEnabled;
        public bool IsServerInstance;

        public string SharingServiceAddress;
        public int SharingServicePort;

        public string NLUServiceAddress;
        public int NLUServicePort;
    }
}
