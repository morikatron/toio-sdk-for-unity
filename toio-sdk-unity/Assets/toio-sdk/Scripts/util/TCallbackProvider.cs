using System;
using System.Collections.Generic;

namespace toio
{
    public class TCallbackProvider<T>
    {
        protected Dictionary<string, Action<T>> listenerTable = new Dictionary<string, Action<T>>();
        protected List<Action<T>> listenerList = new List<Action<T>>();

        public virtual void AddListener(string key, Action<T> listener)
        {
            if (this.listenerTable.ContainsKey(key))
                this.listenerList.Remove(this.listenerTable[key]);
            this.listenerTable[key] = listener;
            this.listenerList.Add(listener);
        }
        public virtual void RemoveListener(string key)
        {
            if (this.listenerTable.ContainsKey(key))
            {
                this.listenerList.Remove(this.listenerTable[key]);
                this.listenerTable.Remove(key);
            }
        }
        public virtual void ClearListener()
        {
            listenerTable.Clear();
            listenerList.Clear();
        }
        public virtual void Notify(T target)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(target);
            }
        }
    }

    public class TCallbackProvider<T1, T2>
    {
        protected Dictionary<string, Action<T1, T2>> listenerTable = new Dictionary<string, Action<T1, T2>>();
        protected List<Action<T1, T2>> listenerList = new List<Action<T1, T2>>();

        public virtual void AddListener(string key, Action<T1, T2> listener)
        {
            this.listenerTable.Add(key, listener);
            this.listenerList.Add(listener);
        }
        public virtual void RemoveListener(string key)
        {
            if (this.listenerTable.ContainsKey(key))
            {
                this.listenerList.Remove(this.listenerTable[key]);
                this.listenerTable.Remove(key);
            }
        }
        public virtual void ClearListener()
        {
            listenerTable.Clear();
            listenerList.Clear();
        }
        public virtual void Notify(T1 p1, T2 p2)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(p1, p2);
            }
        }
    }
}