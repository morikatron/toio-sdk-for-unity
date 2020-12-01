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
    public class B_Cube_Motor : CubePlayModeBase
    {

        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator  targetMove_speed_8() // speed -> 10未満
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(100,100,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            8,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_timeout() // timeout test
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
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
            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_timeout_0() // timeout -> 0は10s
        {
            Start();
            var cube = test.CreateCube(60, 60, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(450,450,270,0,0,
                            Cube.TargetMoveType.RotatingMove,
                            20,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            test.update = TestUntil_Seconds(20);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_x_noChanged() // x座標 ->　0xffff
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(65535,400,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(4)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_xy_noChanged() // x座標y座標 ->　0xffff
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(65535,65535,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(5)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_angle_noChanged() // RotationType ->　NotRotate
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
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

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(6)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_parameterError() // x座標y座標 ->　0xffff RotationType ->Original
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(65535,65535,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.Original,
                            Cube.ORDER_TYPE.Strong);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(7)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_otherWrite() //
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            var lastTime = Time.time;
            cube.TargetMove(400,400,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            
            cube.TargetMove(400,400,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }
    }
}
