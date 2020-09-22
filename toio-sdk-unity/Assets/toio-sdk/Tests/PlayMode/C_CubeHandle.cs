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
using test = toio.Tests.CubeTester;
using assert = UnityEngine.Assertions.Assert;

namespace toio.Tests
{
    /// <summary>
    /// 便利リンク
    /// 【Assertチートシート】
    /// https://qiita.com/su10/items/67a4a90c648b1ef68ab9#assertチートシート
    /// </summary>
    public class C_CubeHandle : CubePlayModeBase
    {
        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator a_moveRaw()
        {
            Start();

            var handle = new CubeHandle(test.CreateCube(250, 250));
            handle.MoveRaw(100, 100);

            test.update = TestUntil_Seconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator b_move()
        {
            Start();

            var handle = new CubeHandle(test.CreateCube(100, 350));

            test.init = (() =>
            {
                handle.Update();
                handle.Move(80, 25, 2000);
            });

            test.update = TestUntil_Seconds(2);

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator c_Move2Target_Front()
        {
            Start();

            var handle0 = new CubeHandle(test.CreateCube(150, 200, 90));
            var handle1 = new CubeHandle(test.CreateCube(250, 200, 90));
            var handle2 = new CubeHandle(test.CreateCube(350, 200, 90));

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var dt = Time.deltaTime;
                var now = Time.time;

                if (now - last_time > 0.05)
                {
                    handle0.Update(); handle1.Update(); handle2.Update();
                    handle0.Move(handle0.Move2Target(150, 350, maxSpd:20));
                    handle1.Move(handle1.Move2Target(250, 350, maxSpd:60));
                    handle2.Move(handle2.Move2Target(350, 350, maxSpd:100));
                    last_time = now;
                }
                if (6 < Time.time - start_time){
                    Debug.LogFormat("handle0.y-target.y = {0}", handle0.y-350);
                    Debug.LogFormat("handle1.y-target.y = {0}", handle1.y-350);
                    Debug.LogFormat("handle2.y-target.y = {0}", handle2.y-350);
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator c_Move2Target_Back()
        {
            Start();

            var handle0 = new CubeHandle(test.CreateCube(150, 330, -90));
            var handle1 = new CubeHandle(test.CreateCube(250, 310, -90));
            var handle2 = new CubeHandle(test.CreateCube(350, 290, -90));

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var dt = Time.deltaTime;
                var now = Time.time;

                if (now - last_time > 0.05)
                {
                    handle0.Update(); handle1.Update(); handle2.Update();
                    handle0.Move(handle0.Move2Target(150, 350, maxSpd:100));
                    handle1.Move(handle1.Move2Target(250, 350, maxSpd:100));
                    handle2.Move(handle2.Move2Target(350, 350, maxSpd:100));
                    last_time = now;
                }
                if (4 < Time.time - start_time){
                    Debug.LogFormat("handle0.pos-target.pos = {0}", handle0.pos-new Vector(150,350));
                    Debug.LogFormat("handle1.pos-target.pos = {0}", handle1.pos-new Vector(250,350));
                    Debug.LogFormat("handle2.pos-target.pos = {0}", handle2.pos-new Vector(350,350));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator c_Move2Target_Side()
        {
            Start();

            var handle0 = new CubeHandle(test.CreateCube(150, 180, 0));
            var handle1 = new CubeHandle(test.CreateCube(150, 250, 0));
            var handle2 = new CubeHandle(test.CreateCube(150, 320, 0));

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var dt = Time.deltaTime;
                var now = Time.time;

                if (now - last_time > 0.05)
                {
                    handle0.Update(); handle1.Update(); handle2.Update();
                    handle0.Move(handle0.Move2Target(320, 400, maxSpd:100));
                    handle1.Move(handle1.Move2Target(250, 400, maxSpd:100));
                    handle2.Move(handle2.Move2Target(180, 400, maxSpd:100));
                    last_time = now;
                }
                if (4 < Time.time - start_time){
                    Debug.LogFormat("handle0.pos-target.pos = {0}", handle0.pos-new Vector(320,400));
                    Debug.LogFormat("handle1.pos-target.pos = {0}", handle1.pos-new Vector(250,400));
                    Debug.LogFormat("handle2.pos-target.pos = {0}", handle2.pos-new Vector(180,400));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator d_Rotate2Rad()
        {
            Start();

            var handle0 = new CubeHandle(test.CreateCube(150, 300, -90));
            var handle1 = new CubeHandle(test.CreateCube(250, 300, -90));
            var handle2 = new CubeHandle(test.CreateCube(350, 300, -90));

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var dt = Time.deltaTime;
                var now = Time.time;

                var tarRad = Deg2Rad(89);

                if (now - last_time > 0.05)
                {
                    handle0.Update(); handle1.Update(); handle2.Update();
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
        public IEnumerator e_TranslateByDist()
        {
            Start();

            var handle0 = new CubeHandle(test.CreateCube(150, 200, 90));
            var handle1 = new CubeHandle(test.CreateCube(250, 200, 90));
            var handle2 = new CubeHandle(test.CreateCube(350, 200, 90));

            var start_time = Time.time;

            test.init = (() =>
            {
                handle0.Update(); handle1.Update(); handle2.Update();
                handle0.Move(handle0.TranslateByDist(dist:150, translate:20));
                handle1.Move(handle1.TranslateByDist(dist:150, translate:60));
                handle2.Move(handle2.TranslateByDist(dist:150, translate:100));
            });

            test.update =  (() =>
            {
                if (5 < Time.time - start_time){
                    Debug.LogFormat("handle0.y-target.y = {0}", handle0.y-350);
                    Debug.LogFormat("handle1.y-target.y = {0}", handle1.y-350);
                    Debug.LogFormat("handle2.y-target.y = {0}", handle2.y-350);
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }
        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator f_RotateByRad()
        {
            Start();

            var handle0 = new CubeHandle(test.CreateCube(150, 300, 90));
            var handle1 = new CubeHandle(test.CreateCube(250, 300, 90));
            var handle2 = new CubeHandle(test.CreateCube(350, 300, 90));

            var start_time = Time.time;
            var drad = 360 * Mathf.Deg2Rad;
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
