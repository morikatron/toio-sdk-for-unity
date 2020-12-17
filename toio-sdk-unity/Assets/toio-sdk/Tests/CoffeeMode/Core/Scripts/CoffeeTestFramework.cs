using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class CoffeeTest : Attribute
    {
        // 優先順位
        private int Order;
        public CoffeeTest() { this.Order = int.MaxValue; }
        public CoffeeTest(int order) { this.Order = order; }
    }

    public interface CoffeeTestInterface
    {
        UniTask Start();
        UniTask Update();
        UniTask End();
    }

    public class BasicCoffeeTest : CoffeeTestInterface
    {
        Func<UniTask> update;
        public BasicCoffeeTest(Func<UniTask> _update)
        {
            this.update = _update;
        }
        public async UniTask Start() { await UniTask.Yield(); }
        public async UniTask Update() { await this.update(); }
        public async UniTask End() { await UniTask.Yield(); }
    }

    public class CoffeeTestFramework
    {
        public Dictionary<string, CoffeeTestInterface> testTable = new Dictionary<string, CoffeeTestInterface>();
        public List<string> testList = new List<string>();
        public Queue<string> testQueue = new Queue<string>();

        public Func<UniTask> oneTimeSetUp = DummyFunc;
        public Func<UniTask> oneTimeTearDown = DummyFunc;
        public Func<UniTask> setUp = DummyFunc;
        public Func<UniTask> tearDown = DummyFunc;

        public async UniTask Start(List<CoffeeTestInterface> tests)
        {
            // 実行コンテクストをUpdateに変更
            UniTask.Yield(PlayerLoopTiming.Update);

            await this.oneTimeSetUp();

            foreach(var test in tests)
            {
                await this.setUp();
                await test.Start();
                await test.Update();
                await test.End();
                await this.tearDown();
            }

            await this.oneTimeTearDown();
        }

        private static async UniTask DummyFunc()
        {
            await UniTask.Yield();
        }
    }
}