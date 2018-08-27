using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace IslandVis.OSGiParser
{
    public class Package
    {
        private string _name;
        private bool _isExported;
        private long? _linesOfCode;
        private Bundle _bundle;
        private List<CompilationUnit> _compilationUnits;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public bool IsExported {
            get { return _isExported; }
            set { _isExported = value; }
        }

        public long LinesOfCode {
            get {
                if (!_linesOfCode.HasValue)
                    foreach (CompilationUnit compilationUnit in _compilationUnits)
                        _linesOfCode += compilationUnit.LinesOfCode;

                return _linesOfCode.Value;
            }

            private set { }
        }

        public Bundle Bundle {
            get { return _bundle; }
            set { _bundle = value; }
        }

        public List<CompilationUnit> CompilationUnits {
            get { return _compilationUnits; }
            set { _compilationUnits = value; }
        }

        public long CompilationUnitCount {
            get { return _compilationUnits.Count; }
            private set { }
        }

        public Package(Bundle bundle, string name)
        {
            _bundle = bundle;
            _name = name;
            _compilationUnits = new List<CompilationUnit>();
        }

        [Obsolete("getLoc is deprecated, use property LinesOfCode instead.")]
        public long getLOC()
        {
            long result = 0;

            foreach (CompilationUnit cu in _compilationUnits)
            {
                result += cu.LinesOfCode;
            }

            return result;
        }

        [Obsolete("getCuCount is deprecated, use property CompilationUnitCount instead.")]
        public long getCuCount()
        {
            long result = 0;

            foreach (CompilationUnit cu in _compilationUnits)
                result++;

            return result;
        }
    }
}
