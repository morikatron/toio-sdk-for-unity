using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace toio.Tests
{
    public class BasicTestMonoBehaviour : MonoBehaviour, IMonoBehaviourTest
    {
        public static Action init;
        public static Func<bool> update;

        public static void Reset()
        {
            Clear();
        }
        public static void Clear()
        {
            init = (() => { });
            update = (() => { return true; });
        }

        public bool IsTestFinished { get; protected set; }
        private bool isFinish = false;

        private async void Start()
        {
            await UniTask.WaitForFixedUpdate();
            init();
        }

        private void Update()
        {
            if (isFinish) { return; }

            if (update())
            {
                isFinish = true;
                Clear();
                StartCoroutine(DelayFinish());
            }
        }

        private IEnumerator DelayFinish()
        {
            yield return new WaitForSeconds(1.0f);
            this.IsTestFinished = true;
        }

        ////////////////////////////////
        //  便利関数
        ////////////////////////////////
        public static Func<bool> UpdateForSeconds(float seconds)
        {
            var start_time = Time.time;
            return (() =>
            {
                var elapsed = Time.time - start_time;
                if (seconds < elapsed)
                {
                    return true;
                }
                return false;
            });
        }
    }

    public static class UniTaskUtl
    {
        public static async UniTask UpdateForSeconds(float sec, Action action, CancellationToken token=default)
        {
            await UniTaskAsyncEnumerable.EveryUpdate().TakeUntil(UniTask.Delay(TimeSpan.FromSeconds(sec))).ForEachAsync(_ =>
            {
                action?.Invoke();
            }, token);
        }

        public static async UniTask UpdateWhile(Func<bool> whilePredicate, Action action, CancellationToken token=default) { await UpdateWhile((_)=>whilePredicate(), action, token); }
        public static async UniTask UpdateWhile(Func<AsyncUnit, bool> whilePredicate, Action action, CancellationToken token=default)
        {
            await UniTaskAsyncEnumerable.EveryUpdate().TakeWhile(whilePredicate).ForEachAsync(_ =>
            {
                action?.Invoke();
            }, token);
        }
    }
}