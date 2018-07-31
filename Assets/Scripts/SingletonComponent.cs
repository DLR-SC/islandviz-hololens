using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis
{
    public abstract class SingletonComponent<T> : MonoBehaviour
        where T : SingletonComponent<T>
    {
        private static T _instance;

        public static T Instance {
            get { return _instance; }
        }

        public static bool IsInitialized {
            get { return _instance != null; }
        }

        protected virtual void Awake()
        {
            if (IsInitialized && _instance != this)
            {
                destroyAdditionalInstance();
                return;
            }

            _instance = (T)this;
            DontDestroyOnLoad(transform.root);
        }

        protected virtual void OnDestroy()
            => _instance = null;

        private void destroyAdditionalInstance()
        {
            Debug.LogErrorFormat("Multiple instances of singleton {0} detected. " +
                "Destroying additional instance.", GetType().Name);

            if (Application.isEditor)
            {
                DestroyImmediate(this);
                return;
            }

            Destroy(this);
        }
    }
}
