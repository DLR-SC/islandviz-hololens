using HoloIslandVis.Model.Graph;
using HoloIslandVis.Model.OSGi.Services;
using QuickGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Model.OSGi
{
    public class OSGiProject
    {
        private long? _compilationUnitCount;

        public long CompilationUnitCount
        {
            get
            {
                if (!_compilationUnitCount.HasValue)
                    foreach (Bundle bundle in Bundles)
                        _compilationUnitCount += bundle.CompilationUnitCount;

                return _compilationUnitCount.Value;
            }
        }

        public string ProjectName { get; set; }
        public int MaximalImportCount { get; set; }
        public long MaximalLinesOfCode { get; set; }
        public List<Bundle> Bundles { get; set; }
        public List<Service> Services { get; set; }
        public BidirectionalGraph<GraphVertex, GraphEdge> DependencyGraph { get; set; }

        public OSGiProject(string projectName)
        {
            ProjectName = projectName;
            MaximalImportCount = 0;
            Bundles = new List<Bundle>();
            Services = new List<Service>();
            DependencyGraph = new BidirectionalGraph<GraphVertex, GraphEdge>(true);
        }

        public CompilationUnit GetCompilationUnit(Vector3 index)
        {
            return GetPackage(new Vector2(index.x, index.y)).CompilationUnits[(int)index.z];
        }

        public Package GetPackage(Vector2 index)
        {
            return Bundles[(int)index.x].Packages[(int)index.y];
        }

        [Obsolete("GetNumberOfCUs is deprecated, use property CompilationUnitCount instead.")]
        public long GetNumberOfCUs()
        {
            long result = 0;
            foreach (Bundle b in Bundles)
            {
                result += b.getCuCount();
            }
            return result;
        }
    }
}
