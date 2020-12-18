using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio.Tests.Core
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ToioTest : Attribute
    {
        // 優先順位
        private int Order;
        public ToioTest() { this.Order = int.MaxValue; }
        public ToioTest(int order) { this.Order = order; }
    }

    public interface ToioTestCaseInterface
    {
        UniTask OneTimeSetUp(CancellationToken cancelToken);
        UniTask OneTimeTearDown(CancellationToken cancelToken);
        UniTask SetUp(CancellationToken cancelToken);
        UniTask TearDown(CancellationToken cancelToken);
    }

    public interface ToioTestInterface
    {
        UniTask Start(CancellationToken cancelToken);
        UniTask Run(CancellationToken cancelToken);
        UniTask End(CancellationToken cancelToken);
    }

    public class BasicToioTest : ToioTestInterface
    {
        Func<CancellationToken, UniTask> run;
        public BasicToioTest(Func<CancellationToken, UniTask> _run)
        {
            this.run = _run;
        }
        public async UniTask Start(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
        public async UniTask Run(CancellationToken cancelToken) { await this.run(cancelToken); }
        public async UniTask End(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
    }

    public class TestFramework
    {
        public Dictionary<string, ToioTestInterface> testTable = new Dictionary<string, ToioTestInterface>();
        public List<string> testList = new List<string>();
        public Queue<string> testQueue = new Queue<string>();

        public Func<CancellationToken, UniTask> oneTimeSetUp = DummyFunc;
        public Func<CancellationToken, UniTask> oneTimeTearDown = DummyFunc;
        public Func<CancellationToken, UniTask> setUp = DummyFunc;
        public Func<CancellationToken, UniTask> tearDown = DummyFunc;

        public async UniTask Start(CancellationToken cancelToken, List<ToioTestInterface> tests)
        {
            // 実行コンテクストをUpdateに変更
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);

            await this.oneTimeSetUp(cancelToken);

            foreach(var test in tests)
            {
                await this.setUp(cancelToken);
                await test.Start(cancelToken);
                await test.Run(cancelToken);
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