using System;
using UnityEngine;

namespace HoloIslandVis.Utilities
{
    public class TransformChangeEventArgs : EventArgs
    {
        public Transform transform;

        public TransformChangeEventArgs(Transform t)
        {
            transform = t;
        }
    }

    public class TransformChangeObserver : MonoBehaviour
    {
        public delegate void TransformChangeHandler(TransformChangeEventArgs eventArgs);
        public event TransformChangeHandler TransformChange = delegate { };

        void Update()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                var eventArgs = new TransformChangeEventArgs(transform);
                TransformChange(eventArgs);
            }
        }
    }
}
