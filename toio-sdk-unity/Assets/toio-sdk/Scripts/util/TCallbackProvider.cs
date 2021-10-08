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
        public virtual void Notify(T1 p1, T2 p2)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(p1, p2);
            }
        }
    }

    public class TCallbackProvider<T1, T2, T3>
    {
        protected Dictionary<string, Action<T1, T2, T3>> listenerTable = new Dictionary<string, Action<T1, T2, T3>>();
        protected List<Action<T1, T2, T3>> listenerList = new List<Action<T1, T2, T3>>();

        public virtual void AddListener(string key, Action<T1, T2, T3> listener)
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
        public virtual void Notify(T1 p1, T2 p2, T3 p3)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(p1, p2, p3);
            }
        }
    }

    public class TCallbackProvider<T1, T2, T3, T4>
    {
        protected Dictionary<string, Action<T1, T2, T3, T4>> listenerTable = new Dictionary<string, Action<T1, T2, T3, T4>>();
        protected List<Action<T1, T2, T3, T4>> listenerList = new List<Action<T1, T2, T3, T4>>();

        public virtual void AddListener(string key, Action<T1, T2, T3, T4> listener)
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
        public virtual void Notify(T1 p1, T2 p2, T3 p3, T4 p4)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(p1, p2, p3, p4);
            }
        }
    }

    public class TCallbackProvider<T1, T2, T3, T4, T5>
    {
        protected Dictionary<string, Action<T1, T2, T3, T4, T5>> listenerTable = new Dictionary<string, Action<T1, T2, T3, T4, T5>>();
        protected List<Action<T1, T2, T3, T4, T5>> listenerList = new List<Action<T1, T2, T3, T4, T5>>();

        public virtual void AddListener(string key, Action<T1, T2, T3, T4, T5> listener)
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
        public virtual void Notify(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(p1, p2, p3, p4, p5);
            }
        }
    }

    public class TCallbackProvider<T1, T2, T3, T4, T5, T6>
    {
        protected Dictionary<string, Action<T1, T2, T3, T4, T5, T6>> listenerTable = new Dictionary<string, Action<T1, T2, T3, T4, T5, T6>>();
        protected List<Action<T1, T2, T3, T4, T5, T6>> listenerList = new List<Action<T1, T2, T3, T4, T5, T6>>();

        public virtual void AddListener(string key, Action<T1, T2, T3, T4, T5, T6> listener)
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
        public virtual void Notify(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            for (int i = this.listenerList.Count-1; i >= 0; i--)
            {
                this.listenerList[i].Invoke(p1, p2, p3, p4, p5, p6);
            }
        }
    }


}