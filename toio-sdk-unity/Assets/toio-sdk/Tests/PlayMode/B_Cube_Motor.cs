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
        public IEnumerator move_top_RotatingMove()
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            cube.TargetMove(300, 100, 0, targetMoveType:Cube.TargetMoveType.RotatingMove);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move_bottom_RotatingMove()
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.TargetMove(300, 400, 0, targetMoveType:Cube.TargetMoveType.RotatingMove);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move_bottom_RoundForwardMove()
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.TargetMove(300, 400, 0, targetMoveType:Cube.TargetMoveType.RoundForwardMove);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move_bottom_RoundBeforeMove()
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.TargetMove(300, 400, 0, targetMoveType:Cube.TargetMoveType.RoundBeforeMove);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move_bottom_AbsoluteCounterClockwise()
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.TargetMove(300, 400, 0,
                targetMoveType:Cube.TargetMoveType.RotatingMove,
                targetRotationType:Cube.TargetRotationType.AbsoluteCounterClockwise);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move_bottom_RelativeCounterClockwise()
        {
            Start();
            var cube = test.CreateCube(250, 250, 270);
            cube.TargetMove(300, 400, 600,
                targetMoveType:Cube.TargetMoveType.RotatingMove,
                targetRotationType:Cube.TargetRotationType.RelativeCounterClockwise);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move_top_VariableSpeed()
        {
            Start();
            var cube = test.CreateCube(250, 400, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );

            cube.TargetMove(300, 100, 0,
                targetMoveType: Cube.TargetMoveType.RotatingMove,
                targetRotationType: Cube.TargetRotationType.NotRotate,
                targetSpeedType: Cube.TargetSpeedType.VariableSpeed);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

    }
}
