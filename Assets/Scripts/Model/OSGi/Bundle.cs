using HoloIslandVis.Model.OSGi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HoloIslandVis.Model.OSGi
{
    public class Bundle
    {
        private int _compilationUnitCount;
        private Package _largestPackage;

        public int CompilationUnitCount {
            get {
                if (_compilationUnitCount == 0.0)
                    foreach (Package package in Packages)
                        _compilationUnitCount += package.CompilationUnits.Count;
                return _compilationUnitCount;
            }

            private set { }
        }

        public Package MostCompilationUnits {
            get {
                if (_largestPackage == null)
                {
                    _largestPackage = Packages[0];
                    for (int i = 1; i < Packages.Count; i++)
                        _largestPackage = (_largestPackage.CompilationUnitCount < Packages[i].CompilationUnitCount)
                            ? Packages[i] : _largestPackage;
                }

                return _largestPackage;
            }

            private set { }
        }

        public string Name { get; set; }
        public string SymbolicName { get; set; }
        public OSGiProject OSGiProject { get; set; }
        public List<Package> Packages { get; set; }
        public List<Package> ExportedPackages { get; set; }
        public List<Package> ImportedPackages { get; set; }
        public List<ServiceComponent> ServiceComponents { get; set; }

        public Bundle(OSGiProject project, string name, string symbolicName)
        {
            OSGiProject = project;
            Name = name;
            SymbolicName = symbolicName;

            Packages = new List<Package>();
            ExportedPackages = new List<Package>();
            ImportedPackages = new List<Package>();
            ServiceComponents = new List<ServiceComponent>();
        }

        [Obsolete("getCuCount is deprecated, use property CompilationUnitCount instead.")]
        public long getCuCount()
        {
            long result = 0;

            foreach (Package pckg in Packages)
                result += pckg.getCuCount();

            return result;
        }
    }
}
