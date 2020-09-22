using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

namespace toio
{
    public class CubeOrderBalancer : MonoBehaviour
    {
        private static CubeOrderBalancer instance = null;

        public static CubeOrderBalancer Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject gameObject = new GameObject();
                    gameObject.name = "~CubeOrderBalancer";
                    instance = (CubeOrderBalancer)gameObject.AddComponent(typeof(CubeOrderBalancer));
                }
                return instance;
            }
        }

        /// <summary>
        /// キューブ毎の命令時間と命令キュー
        /// </summary>
        public class CubeData
        {
            public int strong_cnt = 0;
            public float previousTime = 0;
            public Queue<SendData> sendQueue = new Queue<SendData>();
        }

        /// <summary>
        /// 命令構造体
        /// </summary>
        public struct SendData
        {
            public Cube instance;
            public Action func;
            public Cube.ORDER_TYPE type;
            public int frameStamp;

#if !RELEASE
            public string DEBUG_name;
            public object[] DEBUG_param;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.DEBUG_name + "(");
                foreach (var p in this.DEBUG_param)
                {
                    sb.Append(p.ToString() + ", ");
                }
                sb = sb.Remove(sb.Length - 2, 2).Append(")");
                return sb.ToString();
            }
#endif

            public SendData(Cube _instance, Action _func, Cube.ORDER_TYPE _type)
            {
                this.instance = _instance;
                this.func = _func;
                this.type = _type;
                this.frameStamp = Time.frameCount;

#if !RELEASE
                this.DEBUG_name = "";
                this.DEBUG_param = null;
#endif
            }
        }

        public const int busyThreshold = 2;
        public const float intervalSec = 0.045f; // 45ms
        public Dictionary<Cube, CubeData> cubeTable = new Dictionary<Cube, CubeData>();
        private bool started = false;

        public bool IsBusy(Cube instance)
        {
            return (busyThreshold < this.cubeTable[instance].strong_cnt);
        }

        public bool IsIdle(Cube instance)
        {
            return (0 == this.cubeTable[instance].sendQueue.Count);
        }

        public bool IsOrderable(Cube instance)
        {
            if (!this.cubeTable.ContainsKey(instance))
            {
                this.cubeTable.Add(instance, new CubeData());
            }

            var cube = this.cubeTable[instance];
            return started && (intervalSec < (Time.time - cube.previousTime));
        }

#if !RELEASE
        public void DEBUG_AddOrderParams(Cube instance, Action func, Cube.ORDER_TYPE _type, string DEBUG_name, params object[] DEBUG_plist)
        {
            if (!this.cubeTable.ContainsKey(instance))
            {
                this.cubeTable.Add(instance, new CubeData());
            }

            var cube = this.cubeTable[instance];

            if (this.IsStrong(_type))
            {
                cube.strong_cnt++;
            }

            var data = new SendData(instance, func, _type);
            data.DEBUG_name = DEBUG_name;
            data.DEBUG_param = DEBUG_plist;
            cube.sendQueue.Enqueue(data);
        }
        public void DEBUG_AddOrder(Cube instance, Action func, Cube.ORDER_TYPE _type, string DEBUG_name, object[] DEBUG_plist)
        {
            if (!this.cubeTable.ContainsKey(instance))
            {
                this.cubeTable.Add(instance, new CubeData());
            }

            var cube = this.cubeTable[instance];

            if (this.IsStrong(_type))
            {
                cube.strong_cnt++;
            }

            var data = new SendData(instance, func, _type);
            data.DEBUG_name = DEBUG_name;
            data.DEBUG_param = DEBUG_plist;
            cube.sendQueue.Enqueue(data);
        }
#endif
        /// <summary>
        /// 命令キューに命令を追加
        /// </summary>
        public void AddOrder(Cube instance, Action func, Cube.ORDER_TYPE _type)
        {
            if (!this.cubeTable.ContainsKey(instance))
            {
                this.cubeTable.Add(instance, new CubeData());
            }

            var cube = this.cubeTable[instance];

            if (this.IsStrong(_type))
            {
                cube.strong_cnt++;
            }

            cube.sendQueue.Enqueue(new SendData(instance, func, _type));
        }

        /// <summary>
        /// 命令キューの先頭にある命令を削除
        /// </summary>
        public void RemoveOrder(Cube instance)
        {
            var cube = this.cubeTable[instance];

            var order = cube.sendQueue.Dequeue();
            if (this.IsStrong(order.type))
            {
                cube.strong_cnt--;
            }
        }

        /// <summary>
        /// 命令キューの先頭にある命令を取り出し
        /// </summary>
        public SendData DequeueOrder(Cube instance)
        {
            var cube = this.cubeTable[instance];
            var order = cube.sendQueue.Dequeue();
            if (this.IsStrong(order.type))
            {
                cube.strong_cnt--;
            }
            return order;
        }

        /// <summary>
        /// 命令キューの先頭にある命令を取り出し
        /// </summary>
        public SendData DequeueOrder(CubeData instance)
        {
            var order = instance.sendQueue.Dequeue();
            if (this.IsStrong(order.type))
            {
                instance.strong_cnt--;
            }
            return order;
        }

        /// <summary>
        /// 命令キューの最後にある命令を取り出し
        /// </summary>
        public SendData DequeueLastOrder(CubeData instance)
        {
            var order = instance.sendQueue.Last();
            if (this.IsStrong(order.type))
            {
                instance.strong_cnt--;
            }
            return order;
        }

        public void ClearOrder(Cube instance)
        {
            var cube = this.cubeTable[instance];

            cube.sendQueue.Clear();
            cube.strong_cnt = 0;
        }

        public void ClearOrder(CubeData cube)
        {
            cube.sendQueue.Clear();
            cube.strong_cnt = 0;
        }

        public void CallOrder(CubeData cube, SendData order)
        {
            if (!order.instance.isConnected) { return; }
            cube.previousTime = Time.time;
            order.func();
        }

        void Start()
        {
            started = true;
        }

        /// <summary>
        /// 各Cube毎に命令送信間隔を判定し、一定時間以上間隔が空いていれば命令キューから命令を取り出して送信処理を行う。
        /// 命令優先度Strongの命令は確実に順次送信され、命令優先度Weakの命令は場合により破棄される。
        /// </summary>
        public void Update()
        {
            var nowFrame = Time.frameCount;
            var now = Time.time;
            SendData next;

            // Cube毎の命令キューを捜査
            foreach (var c in this.cubeTable)
            {
                if (0 == c.Value.sendQueue.Count) { continue; }
                if (0 < c.Value.previousTime && intervalSec > (now - c.Value.previousTime)) { continue; }

                // 命令キューの中身がWeakの命令だけの場合:
                // 最後の命令だけを送信し、他の命令は全て破棄
                if (0 == c.Value.strong_cnt)
                {
#if !RELEASE
                    if (1 < c.Value.sendQueue.Count)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var o in c.Value.sendQueue)
                        {
                            if (0 == o.DEBUG_name.Length) { break; }
                            sb.Append(o.ToString()).Append(", ");
                        }
                        sb = sb.Remove(sb.Length - 2, 2);
                        Debug.Log("[CubeOrderBalancer]弱い命令が2つ以上溜まっています. " + sb.ToString());
                    }
#endif
                    // moveの都合上後ろの命令を実行
                    next = this.DequeueLastOrder(c.Value);
                    CallOrder(c.Value, next);
                    this.ClearOrder(c.Value);
                }
                // 命令キューの中身にStrongの命令がある場合:
                // 最初に取り出されたStrongの命令を送信
                // 取り出されたWeak命令は全て破棄し、命令キューに残された強い命令は保持する
                else
                {
#if !RELEASE
                    if (5 < c.Value.strong_cnt)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var o in c.Value.sendQueue)
                        {
                            if (0 == o.DEBUG_name.Length) { break; }
                            sb.Append(o.ToString()).Append(", ");
                        }
                        sb = sb.Remove(sb.Length - 2, 2);
                        Debug.Log("[CubeOrderBalancer]強い命令が5個以上溜まっています. 命令遅延の可能性があるため修正して下さい. " + sb.ToString());
                    }
#endif
                    while (0 < c.Value.sendQueue.Count)
                    {
                        next = this.DequeueOrder(c.Value);
                        if (this.IsStrong(next.type))
                        {
                            CallOrder(c.Value, next);
                            break;
                        }
                    }
                }
            }
        }

        private bool IsStrong(Cube.ORDER_TYPE order)
        {
            return (Cube.ORDER_TYPE.Strong == order);
        }
    }
}