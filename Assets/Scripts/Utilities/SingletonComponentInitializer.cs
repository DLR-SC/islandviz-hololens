using HoloIslandVis.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HoloIslandVis.Utilities
{
    public class SingletonComponentInitializer : MonoBehaviour
    {
        public delegate void InitializationHandler();
        public event InitializationHandler AllInitialized = delegate { };

        public List<Type> _componentTypes;

        public void AddComponent<T>(T singleton) where T : SingletonComponent<T>
        {
            if(_componentTypes == null)
                _componentTypes = new List<Type>();

            _componentTypes.Add(typeof(T).GetTypeInfo().BaseType);
        }

        public void Initialize()
            => StartCoroutine(Run());

        private IEnumerator Run()
        {
            foreach (Type componentType in _componentTypes)
            {
                bool initialized = false;
                var property = componentType.GetProperty("IsInitialized");

                while (!initialized)
                {
                    initialized = (bool) property.GetValue(null);
                    yield return null;
                }
            }

            _componentTypes.Clear();

            UnityMainThreadDispatcher.Instance.Enqueue(() => AllInitialized());
        }
    }
}