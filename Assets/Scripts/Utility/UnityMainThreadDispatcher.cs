using HoloIslandVis;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace HoloIslandVis.Utility
{
    public class UnityMainThreadDispatcher : SingletonComponent<UnityMainThreadDispatcher>
    {
        private ConcurrentQueue<IEnumerator> _executionQueue;
        private bool _isActionActive;

        private void Start()
        {
            _executionQueue = new ConcurrentQueue<IEnumerator>();
            _isActionActive = false;
        }

        private void Update()
        {
            IEnumerator coroutine;
            if (_isActionActive)
                return;

            if (!_executionQueue.IsEmpty && _executionQueue.TryDequeue(out coroutine))
            {
                _isActionActive = true;
                StartCoroutine(coroutine);
            }
        }

        public void Enqueue(Action action)
            => _executionQueue?.Enqueue(actionWrapper(action));

        public void Enqueue<T>(Action<T> action, T arg)
            => _executionQueue?.Enqueue(actionWrapper(action, arg));

        private IEnumerator actionWrapper(Action action)
        {
            action.Invoke();
            _isActionActive = false;
            yield return null;
        }

        private IEnumerator actionWrapper<T>(Action<T> action, T arg)
        {
            action.Invoke(arg);
            _isActionActive = false;
            yield return null;
        }
    }
}