using System.Collections;
using System.Collections.Generic;
using TriangleNet.Voronoi;
using TriangleNet.Topology;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloIslandVis.Utility;

namespace HoloIslandVis.Visualization
{
    public class Island : MonoBehaviour, IFocusable
    {
        private CartographicIsland _cartographicIsland;
        private List<Region> _regions;
        private GameObject _coast;
        private GameObject _importDock;
        private GameObject _exportDock;

        public CartographicIsland CartographicIsland {
            get { return _cartographicIsland; }
            set { _cartographicIsland = value; }
        }

        public List<Region> Regions {
            get {
                if (_regions == null)
                    _regions = new List<Region>();

                return _regions;
            }

            private set { }
        }

        public GameObject Coast {
            get { return _coast; }
            set { _coast = value; }
        }

        public GameObject ImportDock {
            get { return _importDock; }
            set { _importDock = value; }
        }

        public GameObject ExportDock {
            get { return _exportDock; }
            set { _exportDock = value; }
        }

        //Returns true if island does not contain a single CU. Returns false otherwise.
        public bool IsIslandEmpty()
        {
            foreach (Region region in _regions)
            {
                if (region.Buildings.Count > 0)
                    return false;
            }

            return true;
        }

        public void OnFocusEnter()
        {
            RuntimeCache.Instance.ToolTipManager.showToolTip(gameObject);
        }

        public void OnFocusExit()
        {
            RuntimeCache.Instance.ToolTipManager.hideToolTip();
        }
    }
}