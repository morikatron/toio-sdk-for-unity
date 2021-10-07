using System;
using System.Collections.Generic;
using UnityEngine;

public class FunctionWorker : MonoBehaviour
{
    private static object _lock = new object();
    private Queue<Action> backFuncQueue = new Queue<Action>();
    private Queue<Action> forwardFuncQueue = new Queue<Action>();

    public void EnqueueFunc(Action func)
    {
        this.backFuncQueue.Enqueue(func);
    }

    void Update()
    {
        lock (_lock)
        {
            while(0 < this.backFuncQueue.Count)
            {
                this.forwardFuncQueue.Enqueue(this.backFuncQueue.Dequeue());
            }
        }
        while(0 < this.forwardFuncQueue.Count)
        {
            this.forwardFuncQueue.Dequeue().Invoke();
        }
    }
}
