using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Sharing
{
    public class Observable<T>
    {
        public event System.Action<T> ValueChanged;

        private T _value;

        public T Value
        {
            get { return _value; }
            set
            {
                if (Equals(value, _value))
                    return;

                _value = value;
                if (ValueChanged != null)
                    ValueChanged.Invoke(_value);
            }
        }

        public Observable(T value) { _value = value; }
    }
}
