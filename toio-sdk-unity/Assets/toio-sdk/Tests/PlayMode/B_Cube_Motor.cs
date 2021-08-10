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
            Debug.Log("正しい動き：cube動かず、応答がNonSupport");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(100,250,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            8,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(3);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _01_targetMove_timeout() // timeout test
        {
            Debug.Log("正しい動き：2秒で timemout 応答し停止");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(100,400,270,0,2,
                            Cube.TargetMoveType.RotatingMove,
                            20,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(4);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _02_targetMove_timeout_0() // timeout -> 0は10s
        {
            Debug.Log("正しい動き：10秒で timemout 応答し停止");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(100,400,270,0,0,
                            Cube.TargetMoveType.RotatingMove,
                            10,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(12);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _03_targetMove_x_noChange() // x座標 ->　0xffff
        {
            Debug.Log("正しい動き：真下へ移動");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(-1,300,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            50,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(3);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(4)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _04_targetMove_xy_noChange() // x座標y座標 ->　0xffff
        {
            Debug.Log("正しい動き：その場右へ回転");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(-1,-1,0,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(2);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(5)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _05_targetMove_notRotate() // RotationType ->　NotRotate
        {
            Debug.Log("正しい動き：移動後に回転せず");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(250,250,75,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            50,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.NotRotate,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(4);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(6)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _06_targetMove_parameterError_1() // x座標y座標 ->　0xffff RotationType ->Original
        {
            Debug.Log("正しい動き：目標と現在状態が同じな為、ParameterError");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(-1,-1,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.Original,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(2);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(7)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _07_targetMove_parameterError_2() // x座標y座標 ->　0xffff RotationType ->Original
        {
            Debug.Log("正しい動き：目標と現在状態が同じな為、ParameterError");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(-1,-1,90,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.NotRotate,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(8)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _08_targetMove_otherWrite() //
        {
            Debug.Log("正しい動き：下へ1秒移動してから、右へ中央まで移動してから下へ回転。応答は OtherWrite");
            var cube = GetCubeFromHomeIdxs(0);
            cube.targetMoveCallback.AddListener("Test", (c, configID, res) => Debug.Log("応答： " + res) );

            cube.TargetMove(-1,400,90,0,0,
                            Cube.TargetMoveType.RotatingMove,
                            50,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            cube.TargetMove(250,-1,90,0,0,
                            Cube.TargetMoveType.RotatingMove,
                            50,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(3);
            cube.targetMoveCallback.ClearListener();
            yield return null;
        }

        [UnityTest, Order(9)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _09_AccMove_forward() //
        {
            Debug.Log("正しい動き：下へ加速しながら移動");
            var cube = GetCubeFromHomeIdxs(0);

            cube.AccelerationMove(100,2,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(3);
            cube.Move(0,0,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            yield return null;
        }

        [UnityTest, Order(10)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _10_AccMove_backward() //
        {
            Debug.Log("正しい動き：上に向いてから、後退で下へ加速しながら移動");
            var cube = GetCubeFromHomeIdxs(0);
            cube.TargetMove(-1,-1,270,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            30,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            cube.AccelerationMove(-100,2,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(3);
            cube.Move(0,0,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            yield return null;
        }

        [UnityTest, Order(11)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _11_AccMove_left() //
        {
            Debug.Log("正しい動き：左へ曲がりながら加速");
            var cube = GetCubeFromHomeIdxs(0);

            cube.AccelerationMove(80,3,-25,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            yield return null;
        }

        [UnityTest, Order(12)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _12_AccMove_right() //
        {
            Debug.Log("正しい動き：右へ曲がりながら加速（後退）");
            var cube = GetCubeFromHomeIdxs(0);

            cube.TargetMove(-1,-1,270,0,255,
                            Cube.TargetMoveType.RotatingMove,
                            80,
                            Cube.TargetSpeedType.UniformSpeed,
                            Cube.TargetRotationType.AbsoluteLeastAngle,
                            Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            cube.AccelerationMove(-80,3,-25,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            yield return null;
        }

        [UnityTest, Order(13)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _13_AccMove_time() //
        {
            Debug.Log("正しい動き：2秒間加速しながら前進");
            var cube = GetCubeFromHomeIdxs(0);

            cube.AccelerationMove(80,4,0,Cube.AccPriorityType.Translation,200,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(3);
            yield return null;
        }

        [UnityTest, Order(14)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _14_AccMove_time_0()
        {
            Debug.Log("正しい動き：0制御時間が無限を意味する（5秒で強制終了）");
            var cube = GetCubeFromHomeIdxs(0);

            cube.AccelerationMove(80,1,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(5);
            Debug.Log("強制終了");
            cube.Move(0,0,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            yield return null;
        }

        [UnityTest, Order(15)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator _15_AccMove_AccToAcc() //
        {
            Debug.Log("正しい動き：連続する加速命令はスムーズに繋がる");
            var cube = GetCubeFromHomeIdxs(0);

            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.AccelerationMove(-50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.AccelerationMove(-50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.Move(0,0,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(1);
            yield return null;
        }
    }

}
