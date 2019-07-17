using System.Collections;
using System.Collections.Generic;
using TriangleNet.Voronoi;
using TriangleNet.Topology;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

namespace HoloIslandVis.Core.Metaphor
{
    public class Island : MonoBehaviour, IFocusable
    {
        private List<Region> _regions;

        public CartographicIsland CartographicIsland { get; set; }
        public CapsuleCollider IslandLevelCollider { get; set; }
        public MeshCollider PackageLevelCollider { get; set; }
        public GameObject ImportDock { get; set; }
        public GameObject ExportDock { get; set; }
        public GameObject Coast { get; set; }


        public List<Region> Regions {
            get {
                if (_regions == null)
                    _regions = new List<Region>();

                return _regions;
            }

            private set { }
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
            //RuntimeCache.Instance.ToolTipManager.showToolTip(gameObject);
        }

        public void OnFocusExit()
        {
            //RuntimeCache.Instance.ToolTipManager.hideToolTip();
        }
    }
}