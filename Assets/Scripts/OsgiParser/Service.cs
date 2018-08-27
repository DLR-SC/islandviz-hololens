using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IslandVis.OSGiParser
{
    public class Service
    {
        private string _name;
        private CompilationUnit _linkedCompilationUnit;
        private List<ServiceComponent> _implementingComponents;
        private List<ServiceComponent> _referencingComponents;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public CompilationUnit LinkedCompilationUnit {
            get { return _linkedCompilationUnit; }
            set { _linkedCompilationUnit = value; }
        }

        public List<ServiceComponent> ImplementingComponents {
            get { return _implementingComponents; }
            set { _implementingComponents = value; }
        }

        public List<ServiceComponent> ReferencingComponents {
            get { return _referencingComponents; }
            set { _referencingComponents = value; }
        }

        public Service(string name, CompilationUnit compilationUnit)
        {
            _name = name;
            _linkedCompilationUnit = compilationUnit;
            _implementingComponents = new List<ServiceComponent>();
            _referencingComponents  = new List<ServiceComponent>();
        }
    }
}
