using HoloIslandVis.OSGiParser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Visualization
{
    public class Building : MonoBehaviour
    {
        private CompilationUnit _compilationUnit;

        public CompilationUnit CompilationUnit {
            get { return _compilationUnit; }
            set { _compilationUnit = value; }
        }
    }
}
