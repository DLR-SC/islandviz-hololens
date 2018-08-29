using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.OSGiParser
{
    public class ServiceComponent {

        private string _name;
        private CompilationUnit _implCompilationUnit;
        private List<Service> _providedServices;
        private List<Service> _referencedServices;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public CompilationUnit ImplCompilationUnit {
            get { return _implCompilationUnit; } 
            set { _implCompilationUnit = value; }
        }

        public List<Service> ProvidedServices {
            get { return _providedServices; } 
            set { _providedServices = value; }
        }

        public List<Service> ReferencedServices {
            get { return _referencedServices; }
            set { _referencedServices = value; }
        }

        public ServiceComponent(string name, CompilationUnit implCompilationUnit)
        {
            _name = name;
            _providedServices = new List<Service>();
            _referencedServices = new List<Service>();
            _implCompilationUnit = implCompilationUnit;
        }
    }
}