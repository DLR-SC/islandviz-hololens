using HoloIslandVis.Model.OSGi;
using HoloIslandVis.Model.Graph;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Voronoi;

using TNetMesh = TriangleNet.Mesh;
using VFace = TriangleNet.Topology.DCEL.Face;

namespace HoloIslandVis.Core
{
    public class CartographicIsland
    {
        private Bundle _bundle;
        private BoundedVoronoi _voronoi;

        public Bundle Bundle {
            get { return _bundle; }
            private set { }
        }

        public BoundedVoronoi Voronoi {
            get { return _voronoi; }
            private set { }
        }

        public string Name {
            get { return _bundle.Name; }
            private set { }
        }

        public GraphVertex DependencyVertex { get; set; }
        public GameObject IslandGameObject { get; set; }

        public List<TNetMesh> CoastlineMeshes { get; set; }
        public List<VFace> CoastlineCells { get; set; }
        public List<List<TNetMesh>> PackageMeshes { get; set; }
        public List<List<VFace>> PackageCells { get; set; }
        public List<Package> Packages { get; set; }
        public Vector3 WeightedCenter { get; set; }
        public float Radius { get; set; }

        public CartographicIsland(Bundle bundle, BoundedVoronoi voronoi)
        {
            _bundle = bundle;
            _voronoi = voronoi;
            CoastlineMeshes = new List<TNetMesh>();
            CoastlineCells = new List<VFace>();
            PackageMeshes = new List<List<TNetMesh>>();
            PackageCells = new List<List<VFace>>();
            Packages = new List<Package>();
        }
    }
}