using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Interaction
{
    public enum StaticItem
    {
        None,
        Done,
        Adjust,
        Panel,
        Fit,
        Dependencies,
        Reset,
        Invariant
    }

    public class StaticInteractable : MonoBehaviour
    {
        public StaticItem Item;

        void Awake()
        {

        }
    }
}

