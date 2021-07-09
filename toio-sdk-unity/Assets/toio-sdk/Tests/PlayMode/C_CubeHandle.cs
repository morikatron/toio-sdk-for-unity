using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using System.IO;
using Vector=toio.MathUtils.Vector;
using static toio.MathUtils.Utils;


// 省略
using test = toio.Tests.BasicTestMonoBehaviour;
using assert = UnityEngine.Assertions.Assert;

namespace toio.Tests
{
    /// <summary>
    /// 便利リンク
    /// 【Assertチートシート】
    /// https://qiita.com/su10/items/67a4a90c648b1ef68ab9#assertチートシート
    /// </summary>
    public class C_CubeHandle : CubeTestCase
    {
        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator a_moveRaw()
        {
            var handle = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            handle.MoveRaw(80, 100);

            test.update = test.UpdateForSeconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator b_Move()
        {
            var handle = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];

            test.init = (() =>
            {
                handle.Update();
                handle.Move(100, 15, 2000);
            });

            test.update = test.UpdateForSeconds(2);

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator b_Move_Outside_In()
        {
            var handle = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            handle.borderRect = new RectInt(150, 150, 300, 300);

            handle.Update();
            handle.Move(60, 40, 2000);

            test.update = test.UpdateForSeconds(2);

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator b_Move_Outside_In2()
        {
            var handle = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            handle.borderRect = new RectInt(150, 150, 300, 300);

            handle.Update();
            handle.Rotate2Deg(-90).Exec();
            yield return new WaitForSeconds(1f);
            handle.Update();
            var mv = handle.Move(-60, 40, 2000);

            test.update = test.UpdateForSeconds(2);

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator b_Move_Outside_Out()
        {
            var handle = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            handle.borderRect = new RectInt(150, 150, 300, 300);
            handle.Update();
            handle.Rotate2Rad(-Mathf.PI/2).Exec();

            yield return new WaitForSeconds(0.5f);

            handle.Update();
            handle.Move(30, 0, 2000);

            test.update = test.UpdateForSeconds(2);

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator c_Move2Target_Front()
        {
            var handle0 = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            var handle1 = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            var handle2 = cubeManager.handles[GetCubeIdxFromHomeIdx(2)];

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;
                handle0.Update(); handle1.Update(); handle2.Update();

                if (now - last_time > 0.05f)
                {
                    handle0.Move(handle0.Move2Target(100, 200, maxSpd:20));
                    handle1.Move(handle1.Move2Target(200, 200, maxSpd:60));
                    handle2.Move(handle2.Move2Target(300, 200, maxSpd:100));
                    last_time = now;
                }
                if (4 < Time.time - start_time){
                    Debug.LogFormat("handle0.y-target.y = {0}", handle0.y-200);
                    Debug.LogFormat("handle1.y-target.y = {0}", handle1.y-200);
                    Debug.LogFormat("handle2.y-target.y = {0}", handle2.y-200);
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator c_Move2Target_Back()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var cube2 = cubeManager.cubes[GetCubeIdxFromHomeIdx(2)];
            var handle0 = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            var handle1 = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            var handle2 = cubeManager.handles[GetCubeIdxFromHomeIdx(2)];

            cube0.TargetMove(100, 120, 90);
            cube1.TargetMove(200, 140, 90);
            cube2.TargetMove(300, 160, 90);

            yield return new WaitForSeconds(1f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;
                handle0.Update(); handle1.Update(); handle2.Update();

                if (now - last_time > 0.05)
                {
                    handle0.Move(handle0.Move2Target(100, 100, maxSpd:100));
                    handle1.Move(handle1.Move2Target(200, 100, maxSpd:100));
                    handle2.Move(handle2.Move2Target(300, 100, maxSpd:100));
                    last_time = now;
                }
                if (2 < Time.time - start_time){
                    Debug.LogFormat("handle0.pos-target.pos = {0}", handle0.pos-new Vector(100,100));
                    Debug.LogFormat("handle1.pos-target.pos = {0}", handle1.pos-new Vector(200,100));
                    Debug.LogFormat("handle2.pos-target.pos = {0}", handle2.pos-new Vector(300,100));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator c_Move2Target_Side()
        {
            var handle0 = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            var handle1 = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            var handle2 = cubeManager.handles[GetCubeIdxFromHomeIdx(2)];

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;
                handle0.Update(); handle1.Update(); handle2.Update();

                if (now - last_time > 0.05f)
                {
                    handle0.Move(handle0.Move2Target(210, 250, maxSpd:100));
                    handle1.Move(handle1.Move2Target(270, 190, maxSpd:100));
                    handle2.Move(handle2.Move2Target(330, 130, maxSpd:100));
                    last_time = now;
                }
                if (4 < Time.time - start_time){
                    Debug.LogFormat("handle0.pos-target.pos = {0}", handle0.pos-new Vector(210,250));
                    Debug.LogFormat("handle1.pos-target.pos = {0}", handle1.pos-new Vector(270,190));
                    Debug.LogFormat("handle2.pos-target.pos = {0}", handle2.pos-new Vector(330,130));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator d_Rotate2Rad()
        {
            var handle0 = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            var handle1 = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            var handle2 = cubeManager.handles[GetCubeIdxFromHomeIdx(2)];

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var dt = Time.deltaTime;
                var now = Time.time;
                handle0.Update(); handle1.Update(); handle2.Update();

                var tarRad = Deg2Rad(-89);

                if (now - last_time > 0.05)
                {
                    handle0.Move(handle0.Rotate2Rad(tarRad, rotateTime:200));
                    handle1.Move(handle1.Rotate2Rad(tarRad, rotateTime:400));
                    handle2.Move(handle2.Rotate2Rad(tarRad, rotateTime:800));
                    last_time = now;
                }
                if (3 < Time.time - start_time){
                    Debug.LogFormat("handle0.rad-target.rad = {0}", handle0.rad-tarRad);
                    Debug.LogFormat("handle1.rad-target.rad = {0}", handle1.rad-tarRad);
                    Debug.LogFormat("handle2.rad-target.rad = {0}", handle2.rad-tarRad);
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator e_TranslateByDist()
        {
            var handle0 = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            var handle1 = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            var handle2 = cubeManager.handles[GetCubeIdxFromHomeIdx(2)];

            var start_time = Time.time;

            test.init = (() =>
            {
                handle0.Update(); handle1.Update(); handle2.Update();
                handle0.Move(handle0.TranslateByDist(dist:100, translate:20));
                handle1.Move(handle1.TranslateByDist(dist:100, translate:60));
                handle2.Move(handle2.TranslateByDist(dist:100, translate:100));
            });

            test.update =  (() =>
            {
                if (5 < Time.time - start_time){
                    handle0.Update(); handle1.Update(); handle2.Update();
                    Debug.LogFormat("handle0.y-target.y = {0}", handle0.y-170);
                    Debug.LogFormat("handle1.y-target.y = {0}", handle1.y-170);
                    Debug.LogFormat("handle2.y-target.y = {0}", handle2.y-170);
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }
        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator f_RotateByRad()
        {
            var handle0 = cubeManager.handles[GetCubeIdxFromHomeIdx(0)];
            var handle1 = cubeManager.handles[GetCubeIdxFromHomeIdx(1)];
            var handle2 = cubeManager.handles[GetCubeIdxFromHomeIdx(2)];

            var start_time = Time.time;
            var drad = 180 * Mathf.Deg2Rad;
            var tarRad = Rad(drad+Deg2Rad(90));

            test.init = (() =>
            {
                handle0.Update(); handle1.Update(); handle2.Update();
                handle0.Move(handle0.RotateByRad(drad:drad, rotate:20));
                handle1.Move(handle1.RotateByRad(drad:drad, rotate:100));
                handle2.Move(handle2.RotateByRad(drad:drad, rotate:200));
            });

            test.update =  (() =>
            {
                if (5 < Time.time - start_time){
                    handle0.Update(); handle1.Update(); handle2.Update();
                    Debug.LogFormat("handle0.rad-target.rad = {0}", handle0.rad-tarRad);
                    Debug.LogFormat("handle1.rad-target.rad = {0}", handle1.rad-tarRad);
                    Debug.LogFormat("handle2.rad-target.rad = {0}", handle2.rad-tarRad);
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

    }
}