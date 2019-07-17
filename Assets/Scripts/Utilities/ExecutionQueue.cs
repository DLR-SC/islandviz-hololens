using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionQueue : MonoBehaviour
{
    private struct Executable
    {
        public Action Action;
        public bool Await;

        //public Executable(Action action, bool await)
        //{
        //    Action = action;
        //    //Await = await;
        //}
    }

    private Queue<Executable> _queue;
    private bool _block;

    void Start()
    {
        _queue = new Queue<Executable>();
        _block = false;
    }

    void Update()
    {
        if(_queue.Count != 0 && !_block)
        {
            Executable executable = _queue.Dequeue();
            _block = executable.Await;

            // StartCoroutine
        }
    }

    //public void Enqueue(Action action, bool await)
    //{
    //    Executable executable = new Executable(action, await);
    //    _queue.Enqueue(executable);
    //}

    public IEnumerator Execute(Action action, bool await)
    {
        
        yield return null;
    }
}
