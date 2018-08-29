using HoloIslandVis.OSGiParser;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Voronoi;
using UnityEngine;

namespace HoloIslandVis.Visualization
{
    public class Region : MonoBehaviour
    {
        private Package _package;
        private Island _island;
        private List<Building> _buildings;
        private GameObject _regionArea;

        public Package Package {
            get { return _package; }
            set { _package = value; }
        }

        public Island Island {
            get { return _island; }
            set { _island = value; }
        }

        public List<Building> Buildings {
            get {
                if (_buildings == null)
                    _buildings = new List<Building>();

                return _buildings;
            }

            private set { }
        }

        public GameObject RegionArea {
            get { return _regionArea; }
            set { _regionArea = value; }
        }
    }

}