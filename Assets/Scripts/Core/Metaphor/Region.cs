using HoloIslandVis.Model.OSGi;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Voronoi;
using UnityEngine;

namespace HoloIslandVis.Core.Metaphor
{
    public class Region : MonoBehaviour
    {
        private List<Building> _buildings;

        public GameObject RegionArea { get; set; }
        public MeshCollider RegionLevelCollider { get; set; }
        public MeshCollider BuildingLevelCollider { get; set; }
        public Package Package { get; set; }
        public Island Island { get; set; }

        public List<Building> Buildings {
            get {
                if (_buildings == null)
                    _buildings = new List<Building>();

                return _buildings;
            }

            private set { }
        }
    }

}