using System;
using System.Collections;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio.Tests.Core
{
    public abstract partial class ToioTestCase : ToioTestCaseInterface
    {
        public static GameObject gameObject;
        public static MonoBehaviour ownerScript;

        public virtual async UniTask OneTimeSetUp(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
        public virtual async UniTask OneTimeTearDown(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
        public virtual async UniTask SetUp(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
        public virtual async UniTask TearDown(CancellationToken cancelToken) { await UniTask.Yield(PlayerLoopTiming.Update, cancelToken); }
    }

    public partial class BasicTest : MonoBehaviour
    {
        public List<ToioTestInterface> tests = new List<ToioTestInterface>();

        // Start is called before the first frame update
        async void Start()
        {
            /* note
               UniTask専用のワーカースレッドがあるため無限ループ処理を入れるとスレッドがそのまま残る
               キャンセルトークンを全ての関数で伝搬する必要がある
            */
            var cancelToken = this.GetCancellationTokenOnDestroy();

            // テストオブジェクトにコールバックを設定
            TestFramework framework = new TestFramework();
            framework.oneTimeSetUp = this.OneTimeSetUp;
            framework.oneTimeTearDown = this.OneTimeTearDown;
            framework.setUp = this.SetUp;
            framework.tearDown = this.TearDown;

            ToioTestCase.gameObject = this.gameObject;
            ToioTestCase.ownerScript = this;

            await framework.Start(cancelToken, tests);
        }

        async UniTask OneTimeSetUp(CancellationToken cancelToken)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }

        async UniTask OneTimeTearDown(CancellationToken cancelToken)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }

        async UniTask SetUp(CancellationToken cancelToken)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }

        async UniTask TearDown(CancellationToken cancelToken)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, cancelToken);
        }
    }

    // コチラでは独立した便利関数を定義
    public abstract partial class ToioTestCase
    {
        public static async UniTask UpdateForSeconds(float sec, Action action, CancellationToken token)
        {
            await UniTaskAsyncEnumerable.EveryUpdate().TakeUntil(UniTask.Delay(TimeSpan.FromSeconds(sec))).ForEachAsync(_ =>
            {
                action?.Invoke();
            }, token);
        }

        public static async UniTask UpdateWhile(Func<bool> whilePredicate, Action action, CancellationToken token) { await UpdateWhile((_)=>whilePredicate(), action, token); }
        public static async UniTask UpdateWhile(Func<AsyncUnit, bool> whilePredicate, Action action, CancellationToken token)
        {
            await UniTaskAsyncEnumerable.EveryUpdate().TakeWhile(whilePredicate).ForEachAsync(_ =>
            {
                action?.Invoke();
            }, token);
        }
    }
}