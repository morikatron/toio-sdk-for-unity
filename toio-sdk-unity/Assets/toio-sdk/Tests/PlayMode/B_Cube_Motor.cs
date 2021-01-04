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
using test = toio.Tests.BasicTestMonoBehaviour;
using assert = UnityEngine.Assertions.Assert;

namespace toio.Tests
{
    /// <summary>
    /// 便利リンク
    /// 【Assertチートシート】
    /// https://qiita.com/su10/items/67a4a90c648b1ef68ab9#assertチートシート
    /// </summary>
    public class B_Cube_Motor : CubeTestCase
    {

        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _00_targetMove_speed_8() // speed -> 10未満
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(100,250,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            8,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _01_targetMove_timeout() // timeout test
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(100,400,270,0,2,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _02_targetMove_timeout_0() // timeout -> 0は10s
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(250,250,270,0,0,
                            Cube.TargetMoveType.RotatingMove,
                            20,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _03_targetMove_x_noChanged() // x座標 ->　0xffff
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(-1,400,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(4)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _04_targetMove_xy_noChanged() // x座標y座標 ->　0xffff
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(-1,-1,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(5)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _05_targetMove_angle_noChanged() // RotationType ->　NotRotate
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(250,350,75,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.NotRotate,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(6)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _06_targetMove_parameterError_1() // x座標y座標 ->　0xffff RotationType ->Original
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(-1,-1,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.Original,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(5);
            yield return null;
        }

        [UnityTest, Order(7)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _07_targetMove_parameterError_2() // x座標y座標 ->　0xffff RotationType ->Original
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(-1,-1,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.NotRotate,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(8)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _08_targetMove_otherWrite() //
        {
            var cube = cubeManager.cubes[0];
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(400,400,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            cube.TargetMove(100,100,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }
        /*
        [UnityTest, Order(9)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _09_AccMove_forward() //
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(100,2,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(10)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _10_AccMove_backward() //
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(-100,2,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(11)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _11_AccMove_left() //
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(100,2,-100,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(12)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _12_AccMove_right() //
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(-100,2,-65535,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }
        */


        [UnityTest, Order(13)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _13_AccMove_time() //
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,200,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(14)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _14_AccMove_time_0()
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }

        [UnityTest, Order(15)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _15_AccMove_AccToAcc() //
        {
            var cube = cubeManager.cubes[0];
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.AccelerationMove(-50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(10);
            yield return null;
        }



    }

}
