using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using System.IO;


// 省略
using test = toio.Tests.CubeTester;
using assert = UnityEngine.Assertions.Assert;

namespace toio.Tests
{
    /// <summary>
    /// 便利リンク
    /// 【Assertチートシート】
    /// https://qiita.com/su10/items/67a4a90c648b1ef68ab9#assertチートシート
    /// </summary>
    public class A_TestEnv : CubePlayModeBase
    {
        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator Update関数()
        {
            Start();

            test.update = (() =>
            {
                return true;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator Init関数()
        {
            Start();

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

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator Update秒数指定()
        {
            Start();

            test.update = TestUntil_Seconds(3);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator Cube配置()
        {
            Start();
            test.CreateCube(300, 300);
            test.CreateCube(100, 100);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }
    }
}