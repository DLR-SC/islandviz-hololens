using IslandVis.OSGiParser;
using IslandVis.OSGiParser.Graph;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Voronoi;

using TnetMesh = TriangleNet.Mesh;
using VFace = TriangleNet.Topology.DCEL.Face;

// Refactor
public class CartographicIsland
{
    private List<Package> packages;
    private List<List<VFace>> packageCells;
    private List<VFace> coastlineCells;
    //For each Fragment a list of meshes, where each one represents the geometry of a voronoi cell
    private List<List<TnetMesh>> packageTnetMeshes;
    private List<TnetMesh> coastlineTnetMeshes;
    private float radius;
    private Vector3 weightedCenter;

    private Bundle _bundle;
    private GraphVertex _dependencyVertex;
    private BoundedVoronoi _voronoi;
    private GameObject _islandGameObject;

    public Bundle Bundle {
        get { return _bundle; }
        private set { }
    }

    public GraphVertex DependencyVertex {
        get { return _dependencyVertex; }
        set { _dependencyVertex = value; }
    }

    public BoundedVoronoi Voronoi {
        get { return _voronoi; }
        private set {  }
    }

    public GameObject IslandGameObject {
        get { return _islandGameObject; }
        set { _islandGameObject = value; }
    }

    public string Name {
        get { return _bundle.Name; }
        private set { }
    }

    public CartographicIsland(Bundle bundle, BoundedVoronoi voronoi)
    {
        _bundle = bundle;
        _voronoi = voronoi;
        packageCells = new List<List<VFace>>();
        packages = new List<Package>();
        packageTnetMeshes = new List<List<TnetMesh>>();
    }

    // TODO: Refactor
    public float getRadius()
    {
        return radius;
    }
    public Vector3 getWeightedCenter()
    {
        return weightedCenter;
    }
    public List<List<VFace>> getPackageCells()
    {
        return packageCells;
    }
    public List<VFace> getCoastlineCells()
    {
        return coastlineCells;
    }
    public List<List<TnetMesh>> getPackageMeshes()
    {
        return packageTnetMeshes;
    }
    public List<Package> getPackages()
    {
        return packages;
    }

    public List<TnetMesh> getCoastlineMeshes()
    {
        return coastlineTnetMeshes;
    }

    public void addPackageCells(List<VFace> list)
    {
        packageCells.Add(list);
    }
    public void addPackage(Package frag)
    {
        packages.Add(frag);
    }
    public void addPackageMesh(List<TnetMesh> mesh)
    {
        packageTnetMeshes.Add(mesh);
    }
    public void setRadius(float r)
    {
        radius = r;
    }
    public void setWeightedCenter(Vector3 wc)
    {
        weightedCenter = wc;
    }
    public void setCoastlineCells(List<VFace> coastCells)
    {
        coastlineCells = coastCells;
    }
    public void setCoastlineMesh(List<TnetMesh> mesh)
    {
        coastlineTnetMeshes = mesh;
    }

}
