using System;
using System.Threading;
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
        UniTask Start(CancellationToken cancelToken);
        UniTask Update(CancellationToken cancelToken);
        UniTask End(CancellationToken cancelToken);
    }

    public class BasicCoffeeTest : CoffeeTestInterface
    {
        Func<CancellationToken, UniTask> update;
        public BasicCoffeeTest(Func<CancellationToken, UniTask> _update)
        {
            this.update = _update;
        }
        public async UniTask Start(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
        public async UniTask Update(CancellationToken cancelToken) { await this.update(cancelToken); }
        public async UniTask End(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
    }

    public class CoffeeTestFramework
    {
        public Dictionary<string, CoffeeTestInterface> testTable = new Dictionary<string, CoffeeTestInterface>();
        public List<string> testList = new List<string>();
        public Queue<string> testQueue = new Queue<string>();

        public Func<CancellationToken, UniTask> oneTimeSetUp = DummyFunc;
        public Func<CancellationToken, UniTask> oneTimeTearDown = DummyFunc;
        public Func<CancellationToken, UniTask> setUp = DummyFunc;
        public Func<CancellationToken, UniTask> tearDown = DummyFunc;

        public async UniTask Start(CancellationToken cancelToken, List<CoffeeTestInterface> tests)
        {
            // 実行コンテクストをUpdateに変更
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);

            await this.oneTimeSetUp(cancelToken);

            foreach(var test in tests)
            {
                await this.setUp(cancelToken);
                await test.Start(cancelToken);
                await test.Update(cancelToken);
                await test.End(cancelToken);
                await this.tearDown(cancelToken);
            }

            await this.oneTimeTearDown(cancelToken);
        }

        private static async UniTask DummyFunc(CancellationToken cancelToken)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }
    }
}