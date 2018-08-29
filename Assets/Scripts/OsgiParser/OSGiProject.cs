using HoloIslandVis.OSGiParser.Graph;
using QuickGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.OSGiParser
{
    public class OSGiProject
    {
        private string _projectName;
        private long? _compilationUnitCount;
        private int _maximalImportCount;
        private long _maximalLinesOfCode;
        private List<Bundle> _bundles;
        private List<Service> _services;
        private BidirectionalGraph<GraphVertex, GraphEdge> _dependencyGraph;
        //private int sizeInMemory;

        public string ProjectName {
            get { return _projectName; }
            set { _projectName = value; }
        }

        public long CompilationUnitCount {
            get {
                if (!_compilationUnitCount.HasValue)
                    foreach (Bundle bundle in _bundles)
                        _compilationUnitCount += bundle.CompilationUnitCount;

                return _compilationUnitCount.Value;
            }
        }

        public int MaximalImportCount {
            get { return _maximalImportCount; }
            set { _maximalImportCount = value; }
        }

        // TODO: private set!
        public long MaximalLinesOfCode {
            get { return _maximalLinesOfCode; }
            set { _maximalLinesOfCode = value; }
        }

        public List<Bundle> Bundles {
            get { return _bundles; }
            set { _bundles = value; }
        }

        public List<Service> Services {
            get { return _services; }
            set { _services = value; }
        }

        public BidirectionalGraph<GraphVertex, GraphEdge> DependencyGraph {
            get { return _dependencyGraph; }
            set { _dependencyGraph = value; }
        }

        public OSGiProject(string projectName)
        {
            _projectName = projectName;
            _maximalImportCount = 0;
            _bundles = new List<Bundle>();
            _services = new List<Service>();
            _dependencyGraph = new BidirectionalGraph<GraphVertex, GraphEdge>(true);
        }

        public CompilationUnit GetCompilationUnit(Vector3 index)
        {
            return GetPackage(new Vector2(index.x, index.y)).CompilationUnits[(int)index.z];
        }

        public Package GetPackage(Vector2 index)
        {
            return _bundles[(int)index.x].Packages[(int)index.y];
        }

        [Obsolete("getNumberOfCUs is deprecated, use property CompilationUnitCount instead.")]
        public long getNumberOfCUs()
        {
            long result = 0;
            foreach (Bundle b in _bundles)
            {
                result += b.getCuCount();
            }
            return result;
        }
    }
}
