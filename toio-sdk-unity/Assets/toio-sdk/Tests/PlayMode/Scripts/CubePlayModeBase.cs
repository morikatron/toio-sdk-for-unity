using System;
using NUnit.Framework;
using UnityEngine;

namespace toio.Tests
{
    public class CubePlayModeBase
    {
        private static GameObject res_stage = null;

        [OneTimeSetUp] // クラスのテストが開始される前に一度だけ実行される
        public void OneTimeSetUp()
        {
            CubeTester.Reset();
            Debug.Log("<color=green>テスト開始</color>");

            // cubeのテスト用シーンを作成
            if (null == res_stage)
            {
                res_stage = (GameObject)Resources.Load("Stage");
            }
            var stage = GameObject.Find("Stage");
            if (null == stage)
            {
                var obj = GameObject.Instantiate(res_stage, Vector3.zero, Quaternion.identity);
                obj.name = "Stage";
            }
        }

        [OneTimeTearDown] // クラスの全てのテストが終了した後に実行される
        public void OneTimeTearDown()
        {
            Debug.Log("<color=green>テスト終了</color>");
        }

        [SetUp] // クラスの各テストが開始される前に実行される
        public void SetUp()
        {
        }

        [TearDown] // クラスの各テストが終了した後に実行される
        public void TearDown()
        {
        }

        public void Start()
        {
            var objStackFrame = new System.Diagnostics.StackFrame(1);
            var methodname = objStackFrame.GetMethod().ReflectedType.FullName;
            var head = methodname.IndexOf('<') + 1;
            var tail = methodname.IndexOf('>');
            methodname = methodname.Substring(head, tail - head);
            Debug.Log("<color=white>" + methodname + "</color>");
            CubeTester.Reset();
        }

        public static Func<bool> TestUntil_Seconds(float seconds)
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
}