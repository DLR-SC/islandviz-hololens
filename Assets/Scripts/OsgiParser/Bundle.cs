using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoloIslandVis.OSGiParser
{
    public class Bundle
    {
        private string _name;
        private string _symbolicName;
        private long? _compilationUnitCount;
        private Package _largestPackage;
        private OSGiProject _project;
        private List<Package> _packages;
        private List<Package> _exportedPackages;
        private List<Package> _importedPackages;
        private List<ServiceComponent> _serviceComponents;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string SymbolicName {
            get { return _symbolicName; }
            set { _symbolicName = value; }
        }

        public long CompilationUnitCount {
            get {
                if(!_compilationUnitCount.HasValue)
                    foreach(Package package in _packages)
                        _compilationUnitCount += package.CompilationUnitCount;

                return _compilationUnitCount.Value;
            }

            private set { }
        }

        public Package MostCompilationUnits {
            get {
                if (_largestPackage == null)
                {
                    _largestPackage = _packages[0];
                    for (int i = 1; i < _packages.Count; i++)
                        _largestPackage = (_largestPackage.CompilationUnitCount < _packages[i].CompilationUnitCount) 
                            ? _packages[i] : _largestPackage;
                }

                return _largestPackage;
            }

            private set { }
        }

        public OSGiProject OSGiProject {
            get { return _project; }
            set { _project = value; }
        }

        public List<Package> Packages {
            get { return _packages; }
            set { _packages = value; }
        }

        public List<Package> ExportedPackages {
            get { return _exportedPackages; }
            set { _exportedPackages = value; }
        }

        public List<Package> ImportedPackages {
            get { return _importedPackages; }
            set { _importedPackages = value; }
        }

        public List<ServiceComponent> ServiceComponents {
            get { return _serviceComponents; }
            set { _serviceComponents = value; }
        }

        public Bundle(OSGiProject project, string name, string symbolicName)
        {
            _project = project;
            _name = name;
            _symbolicName = symbolicName;

            _packages = new List<Package>();
            _exportedPackages = new List<Package>();
            _importedPackages = new List<Package>();
            _serviceComponents = new List<ServiceComponent>();
        }

        [Obsolete("getCuCount is deprecated, use property CompilationUnitCount instead.")]
        public long getCuCount()
        {
            long result = 0;

            foreach (Package pckg in _packages)
                result += pckg.getCuCount();

            return result;
        }
    }
}
