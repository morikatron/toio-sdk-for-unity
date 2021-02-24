using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using System.IO;
using Cysharp.Threading.Tasks;

// 省略
using test = toio.Tests.BasicTestMonoBehaviour;
using assert = UnityEngine.Assertions.Assert;

namespace toio.Tests
{
    /// <summary>
    /// 便利リンク
    ///【Assertチートシート】
    /// https://qiita.com/su10/items/67a4a90c648b1ef68ab9#assertチートシート
    ///【Unity】TestRunnerで使えるNUnit.Frameworknoアトリビュートの使い方メモ
    /// https://qiita.com/okNirvy/items/e183ecec97572a16861e
    /// Unity Test Framework
    /// https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html
    /// UniTask API/Doc
    /// https://github.com/Cysharp/UniTask
    /// https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.UniTask.html
    /// </summary>
    public class A_TestEnv : CubeTestCase
    {
        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _0_HelloTest()
        {
            Debug.Log("Hello Test!!");
            yield return null;
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static  IEnumerator _1_BasicTest_Update関数()
        {
            int cnt = 0;
            test.update = (() =>
            {
                Debug.Log("Update!!");
                return (1 <= cnt++);
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _2_BasicTest_Init関数()
        {
            test.init = (() =>
            {
                Debug.Log("init");
            });

            int cnt = 0;
            test.update = (() =>
            {
                Debug.Log("update");
                return (2 <= cnt++);
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _3_BasicTest_Update秒数指定()
        {
            test.update = test.UpdateForSeconds(3);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(4)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _4_UniTask_UpdateWhile() => UniTask.ToCoroutine(async () =>
        {
            int cnt = 0;
            bool flg = true;
            await UniTaskUtl.UpdateWhile((() => flg), () =>
            {
                flg = (cnt++ < 3);
                Debug.Log("Update() " + Time.frameCount);
            });
        });

        [UnityTest, Order(5)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _5_UniTask_UpdateForSeconds() => UniTask.ToCoroutine(async () =>
        {
            await UniTaskUtl.UpdateForSeconds(0.5f, () =>
            {
                Debug.Log("Update1() " + Time.frameCount);
            });
            await UniTask.Delay(1000);
            await UniTaskUtl.UpdateForSeconds(1, () =>
            {
                Debug.Log("Update2() " + Time.frameCount);
            });
        });

        [UnityTest, Order(6)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _6_UniTask_Delay() => UniTask.ToCoroutine(async () =>
        {
            for(int i = 0; i < 3; i++)
            {
                Debug.LogFormat("i = {0}, Time.frameCount = {1}", i, Time.frameCount);
                await UniTask.Delay(1000);
                Debug.LogFormat("i = {0}, Time.frameCount = {1}", i, Time.frameCount);
                await UniTask.Delay(500);
                Debug.LogFormat("i = {0}, Time.frameCount = {1}", i, Time.frameCount);
            }
        });

        [UnityTest, Order(7)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _7_CubeMove()
        {
            cubeManager.cubes[0].Move(50, -50, 1500);
            yield return new WaitForSeconds(2);

            cubeManager.cubes[0].Move(-50, 50, 1500);
            yield return new WaitForSeconds(2);
        }

        [UnityTest, Order(8)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _8_CubeMove()
        {
            cubeManager.cubes[0].Move(10, 10, 1000);
            yield return new WaitForSeconds(2);

            cubeManager.cubes[0].Move(-10, -10, 1000);
            yield return new WaitForSeconds(2);
        }
    }
}