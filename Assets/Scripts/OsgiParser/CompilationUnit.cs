using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace IslandVis.OSGiParser
{
    public enum Type
    {
        Class,
        AbstractClass,
        Interface,
        Enum,
        Unknown
    };

    public enum AccessModifier
    {
        Public,
        Private,
        Protected,
        Static,
        Final,
        Default
    };

    public class CompilationUnit
    {
        private string _name;
        private long _linesOfCode;
        private bool _isService;
        private bool _isServiceComponent;
        private Type _type;
        private AccessModifier _accessModifier;
        private Package _package;
        private GameObject _gameObject;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public long LinesOfCode {
            get { return _linesOfCode; }
            set { _linesOfCode = value; }
        }

        public bool IsService {
            get { return _isService; }
            set { _isService = value; }
        }

        public bool IsServiceComponent {
            get { return _isServiceComponent; }
            set { _isServiceComponent = value; }
        }

        public Type Type {
            get { return _type; }
            set { _type = value; }
        }

        public AccessModifier AccessModifier {
            get { return _accessModifier; }
            set { _accessModifier = value; }
        }

        public Package Package {
            get { return _package; }
            set { _package = value; }
        }

        public GameObject GameObject {
            get { return _gameObject; } 
            set { _gameObject = value; }
        }

        public CompilationUnit(Package package, string name, long linesOfCode, Type type, AccessModifier accessModifier)
        {
            _package = package;
            _name = name;
            _linesOfCode = linesOfCode;
            _type = type;
            _accessModifier = accessModifier;

            _isService = false;
            _isServiceComponent = false;
        }
    }
}