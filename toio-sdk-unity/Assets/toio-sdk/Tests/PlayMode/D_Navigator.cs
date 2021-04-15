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
using toio.Navigation;

using test = toio.Tests.BasicTestMonoBehaviour;
using assert = UnityEngine.Assertions.Assert;

namespace toio.Tests
{
    /// <summary>
    /// 便利リンク
    /// 【Assertチートシート】
    /// https://qiita.com/su10/items/67a4a90c648b1ef68ab9#assertチートシート
    /// </summary>
    public class D_Navigator : CubeTestCase
    {
        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator a_ToTarget1()
        {
            var navigator = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            navigator.mode = Navigator.Mode.AVOID;

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator.Update();
                    navigator.Navi2Target(250, 250, maxSpd:80).Exec();
                    last_time = now;
                }

                if (2.5 < Time.time - start_time){
                    Debug.LogFormat("handle.pos-target.pos = {0}", navigator.handle.pos-new Vector(250, 250));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator b_BorderGoOut()
        {
            var cube = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            navigator.mode = Navigator.Mode.AVOID;

            cube.TargetMove(120, 150, 180);

            yield return new WaitForSeconds(1f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator.Update();

                    var res = navigator.Navi2Target(0, 400, 80).Exec();
                    last_time = now;
                }

                if (3 < Time.time - start_time){
                    Debug.LogFormat("handle.pos-target.pos = {0}", navigator.handle.pos-new Vector(0, 400));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }


        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator b_BorderGoIn()
        {
            var cube = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            navigator.mode = Navigator.Mode.AVOID;

            cube.TargetMove(80, 150, 225);

            yield return new WaitForSeconds(1f);

            var start_time = Time.time;
            var last_time = start_time;

            test.init = (() =>
            {
            });

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator.Update();

                    var res = navigator.Navi2Target(0, 400, 80).Exec();
                    last_time = now;
                }

                if (3 < Time.time - start_time){
                    Debug.LogFormat("handle.pos-target.pos = {0}", navigator.handle.pos-new Vector(0, 400));
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator c_Intersect1v1Lateral()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            navigator0.mode = Navigator.Mode.AVOID;
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(3)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(3)];
            navigator1.mode = Navigator.Mode.AVOID;

            cube0.TargetMove(65535, 65535, 45);
            cube1.TargetMove(65535, 65535, 130);

            yield return new WaitForSeconds(0.5f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update();navigator1.Update();
                    navigator0.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator1.Navi2Target(120, 380, maxSpd:80).Exec();
                    last_time = now;
                }

                if (4f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator c_Intersect1v1Longitudinal()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            navigator0.mode = Navigator.Mode.AVOID;
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(7)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(7)];
            navigator1.mode = Navigator.Mode.AVOID;

            cube0.TargetMove(65535, 65535, 45);
            cube1.TargetMove(65535, 65535, 360-135);

            yield return new WaitForSeconds(0.5f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update();navigator1.Update();
                    navigator0.Navi2Target(400, 400, maxSpd:80).Exec();
                    navigator1.Navi2Target(100, 100, maxSpd:80).Exec();
                    last_time = now;
                }

                if (4f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator d_ToTarget4()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(1)];
            var cube2 = cubeManager.cubes[GetCubeIdxFromHomeIdx(4)];
            var navigator2 = cubeManager.navigators[GetCubeIdxFromHomeIdx(4)];
            var cube3 = cubeManager.cubes[GetCubeIdxFromHomeIdx(5)];
            var navigator3 = cubeManager.navigators[GetCubeIdxFromHomeIdx(5)];

            cube0.TargetMove(100, 100, 0);
            cube1.TargetMove(140, 100, 45);
            cube2.TargetMove(100, 140, 0);
            cube3.TargetMove(150, 150, 270);

            yield return new WaitForSeconds(2f);


            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update(usePred:true);
                    navigator1.Update(usePred:true);
                    navigator2.Update(usePred:true);
                    navigator3.Update(usePred:true);

                    navigator0.Navi2Target(350, 350, maxSpd:80).Exec();
                    navigator1.Navi2Target(350, 350, maxSpd:80).Exec();
                    navigator2.Navi2Target(350, 350, maxSpd:80).Exec();
                    navigator3.Navi2Target(350, 350, maxSpd:80).Exec();

                    last_time = now;
                }

                if (4f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator e_Intersect4v4Longitudinal()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(1)];
            var cube2 = cubeManager.cubes[GetCubeIdxFromHomeIdx(4)];
            var navigator2 = cubeManager.navigators[GetCubeIdxFromHomeIdx(4)];
            var cube3 = cubeManager.cubes[GetCubeIdxFromHomeIdx(5)];
            var navigator3 = cubeManager.navigators[GetCubeIdxFromHomeIdx(5)];

            var cube4 = cubeManager.cubes[GetCubeIdxFromHomeIdx(2)];
            var navigator4 = cubeManager.navigators[GetCubeIdxFromHomeIdx(2)];
            var cube5 = cubeManager.cubes[GetCubeIdxFromHomeIdx(3)];
            var navigator5 = cubeManager.navigators[GetCubeIdxFromHomeIdx(3)];
            var cube6 = cubeManager.cubes[GetCubeIdxFromHomeIdx(6)];
            var navigator6 = cubeManager.navigators[GetCubeIdxFromHomeIdx(6)];
            var cube7 = cubeManager.cubes[GetCubeIdxFromHomeIdx(7)];
            var navigator7 = cubeManager.navigators[GetCubeIdxFromHomeIdx(7)];

            cube0.TargetMove(100, 100, 0);
            cube1.TargetMove(140, 100, 45);
            cube2.TargetMove(100, 140, 0);
            cube3.TargetMove(150, 150, 270);
            cube4.TargetMove(350, 350, 90);
            cube5.TargetMove(400, 360, 135);
            cube6.TargetMove(360, 400, 90);
            cube7.TargetMove(400, 400, 180);

            yield return new WaitForSeconds(2f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update(usePred:true);
                    navigator1.Update(usePred:true);
                    navigator2.Update(usePred:true);
                    navigator3.Update(usePred:true);
                    navigator4.Update(usePred:true);
                    navigator5.Update(usePred:true);
                    navigator6.Update(usePred:true);
                    navigator7.Update(usePred:true);

                    navigator0.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator1.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator2.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator3.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator4.Navi2Target(120, 120, maxSpd:80).Exec();
                    navigator5.Navi2Target(120, 120, maxSpd:80).Exec();
                    navigator6.Navi2Target(120, 120, maxSpd:80).Exec();
                    navigator7.Navi2Target(120, 120, maxSpd:80).Exec();

                    last_time = now;
                }

                if (5f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }


        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator e_Intersect4v4Lateral()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(1)];
            var cube2 = cubeManager.cubes[GetCubeIdxFromHomeIdx(4)];
            var navigator2 = cubeManager.navigators[GetCubeIdxFromHomeIdx(4)];
            var cube3 = cubeManager.cubes[GetCubeIdxFromHomeIdx(5)];
            var navigator3 = cubeManager.navigators[GetCubeIdxFromHomeIdx(5)];

            var cube4 = cubeManager.cubes[GetCubeIdxFromHomeIdx(2)];
            var navigator4 = cubeManager.navigators[GetCubeIdxFromHomeIdx(2)];
            var cube5 = cubeManager.cubes[GetCubeIdxFromHomeIdx(3)];
            var navigator5 = cubeManager.navigators[GetCubeIdxFromHomeIdx(3)];
            var cube6 = cubeManager.cubes[GetCubeIdxFromHomeIdx(6)];
            var navigator6 = cubeManager.navigators[GetCubeIdxFromHomeIdx(6)];
            var cube7 = cubeManager.cubes[GetCubeIdxFromHomeIdx(7)];
            var navigator7 = cubeManager.navigators[GetCubeIdxFromHomeIdx(7)];

            cube0.TargetMove(100, 100, 0);
            cube1.TargetMove(140, 100, 45);
            cube2.TargetMove(100, 140, 0);
            cube3.TargetMove(150, 150, 270);
            cube4.TargetMove(360, 100, 0);
            cube5.TargetMove(400, 100, 90);
            cube6.TargetMove(350, 150, 0);
            cube7.TargetMove(400, 140, 45);

            yield return new WaitForSeconds(2f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update(usePred:true);
                    navigator1.Update(usePred:true);
                    navigator2.Update(usePred:true);
                    navigator3.Update(usePred:true);
                    navigator4.Update(usePred:true);
                    navigator5.Update(usePred:true);
                    navigator6.Update(usePred:true);
                    navigator7.Update(usePred:true);

                    navigator0.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator1.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator2.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator3.Navi2Target(380, 380, maxSpd:80).Exec();
                    navigator4.Navi2Target(120, 380, maxSpd:80).Exec();
                    navigator5.Navi2Target(120, 380, maxSpd:80).Exec();
                    navigator6.Navi2Target(120, 380, maxSpd:80).Exec();
                    navigator7.Navi2Target(120, 380, maxSpd:80).Exec();

                    last_time = now;
                }

                if (5f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator f_Rearend()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(1)];

            cube0.TargetMove(150, 200, 30);
            cube1.TargetMove(180, 200, 0);

            yield return new WaitForSeconds(1.5f);

            var start_time = Time.time;
            var last_time = start_time;


            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update();navigator1.Update();

                    navigator0.Navi2Target(400, 150, maxSpd:80).Exec();
                    navigator1.Navi2Target(400, 250, maxSpd:80).Exec();

                    last_time = now;
                }

                if (2f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }


        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator f_SideCollision()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(1)];

            cube0.TargetMove(100, 215, 350);
            cube1.TargetMove(100, 185, 10);

            yield return new WaitForSeconds(1.5f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update();navigator1.Update();

                    navigator0.Navi2Target(350, 200, maxSpd:80).Exec();
                    navigator1.Navi2Target(350, 200, maxSpd:80).Exec();

                    last_time = now;
                }

                if (2f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator g_AwayTarget()
        {
            var cube0 = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            var navigator0 = cubeManager.navigators[GetCubeIdxFromHomeIdx(0)];
            var cube1 = cubeManager.cubes[GetCubeIdxFromHomeIdx(1)];
            var navigator1 = cubeManager.navigators[GetCubeIdxFromHomeIdx(1)];
            navigator0.ClearOther();

            cube0.TargetMove(150, 150, 0);
            cube1.TargetMove(250, 350, 0);

            yield return new WaitForSeconds(2f);

            var start_time = Time.time;
            var last_time = start_time;

            test.update =  (() =>
            {
                var now = Time.time;

                if (now - last_time > 0.05f)
                {
                    navigator0.Update();navigator1.Update();

                    navigator0.NaviAwayTarget(navigator1.handle.pos, maxSpd:80).Exec();
                    navigator1.Navi2Target(navigator0.handle.pos, maxSpd:40).Exec();

                    last_time = now;
                }

                if (5f < Time.time - start_time){
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

    }
}