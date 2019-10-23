using HoloIslandVis.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Utilities
{
    public class DebugLog : SingletonComponent<DebugLog>
    {
        private TextMesh _textMesh;

        void Awake()
        {
            base.Awake();
            _textMesh = GetComponent<TextMesh>();
        }

        public void SetText(string text)
        {
            _textMesh.text = text;
        }
    }
}
