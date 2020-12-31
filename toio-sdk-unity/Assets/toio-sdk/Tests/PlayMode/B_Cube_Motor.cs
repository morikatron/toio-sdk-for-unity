/*
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
        public IEnumerator  targetMove_speed_8() // speed -> 10未満
        {
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_timeout() // timeout test
        {
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
            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_timeout_0() // timeout -> 0は10s
        {
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
            test.update = test.UpdateForSeconds(20);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_x_noChanged() // x座標 ->　0xffff
        {
            var cube = test.CreateCube(250, 250, 270);
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(4)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_xy_noChanged() // x座標y座標 ->　0xffff
        {
            var cube = test.CreateCube(250, 250, 270);
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(5)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_angle_noChanged() // RotationType ->　NotRotate
        {
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(6)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_parameterError_1() // x座標y座標 ->　0xffff RotationType ->Original
        {
            var cube = test.CreateCube(250, 250, 270);
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(7)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_parameterError_2() // x座標y座標 ->　0xffff RotationType ->Original
        {
            var cube = test.CreateCube(250, 250, 270);
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(8)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator targetMove_otherWrite() //
        {
            var cube = test.CreateCube(250, 250, 270);
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

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }
        /*
        [UnityTest, Order(9)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator MultitargetMove_parameterError_1() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.multiTargetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            int[] xl = new int[]{300,-1};
            int[] yl = new int[]{350,-1};
            int[] al = new int[]{80,80};
            Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.Original};

            cube.MultiTargetMove(xl,yl,al,tl,0,20,
                                Cube.TargetMoveType.RotatingMove,30,
                                Cube.TargetSpeedType.UniformSpeed,
                                Cube.MultiWriteType.Write,
                                Cube.ORDER_TYPE.Strong);

            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(10)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator MultitargetMove_parameterError_2() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.multiTargetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            int[] xl = new int[]{300,-1};
            int[] yl = new int[]{350,-1};
            int[] al = new int[]{80,80};
            Cube.TargetRotationType[] tl = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.NotRotate};

            cube.MultiTargetMove(xl,yl,al,tl,0,20,
                                Cube.TargetMoveType.RotatingMove,30,
                                Cube.TargetSpeedType.UniformSpeed,
                                Cube.MultiWriteType.Write,
                                Cube.ORDER_TYPE.Strong);

            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(11)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator MultitargetMove_add() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            int[] xl_1 = new int[]{250,200};
            int[] yl_1 = new int[]{200,450};
            int[] al_1 = new int[]{200,300};
            Cube.TargetRotationType[] tl_1 = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

            cube.MultiTargetMove(xl_1,yl_1,al_1,tl_1,0,20,
                                Cube.TargetMoveType.RotatingMove,30,
                                Cube.TargetSpeedType.UniformSpeed,
                                Cube.MultiWriteType.Write,
                                Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(1);

            int[] xl_2 = new int[]{300,300};
            int[] yl_2 = new int[]{350,250};
            int[] al_2 = new int[]{80,120};
            Cube.TargetRotationType[] tl_2 = new Cube.TargetRotationType[]{
                Cube.TargetRotationType.AbsoluteClockwise,
                Cube.TargetRotationType.AbsoluteClockwise};

            cube.MultiTargetMove(xl_2,yl_2,al_2,tl_2,0,20,
                                Cube.TargetMoveType.RotatingMove,30,
                                Cube.TargetSpeedType.UniformSpeed,
                                Cube.MultiWriteType.Add,
                                Cube.ORDER_TYPE.Strong);

            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(12)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator MultitargetMove_add_otherwrite() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.targetMoveCallback.AddListener("Test",
                (c, configID, res) =>
                {
                    Debug.Log(res);
                }
            );
            int[] xl_1 = new int[]{250,200};
            int[] yl_1 = new int[]{200,200};
            int[] al_1 = new int[]{270,90};
            Cube.TargetRotationType[] tl_1 = new Cube.TargetRotationType[]{
            Cube.TargetRotationType.AbsoluteClockwise,
            Cube.TargetRotationType.AbsoluteClockwise};

            cube.MultiTargetMove(xl_1,yl_1,al_1,tl_1,0,20,
                                Cube.TargetMoveType.RotatingMove,30,
                                Cube.TargetSpeedType.UniformSpeed,
                                Cube.MultiWriteType.Add,
                                Cube.ORDER_TYPE.Strong);

            yield return new WaitForSeconds(1);

            int[] xl_2 = new int[]{350,300};
            int[] yl_2 = new int[]{300,250};
            int[] al_2 = new int[]{270,90};
            Cube.TargetRotationType[] tl_2 = new Cube.TargetRotationType[]{
                Cube.TargetRotationType.AbsoluteClockwise,
                Cube.TargetRotationType.AbsoluteClockwise};

            cube.MultiTargetMove(xl_2,yl_2,al_2,tl_2,0,20,
                                Cube.TargetMoveType.RotatingMove,30,
                                Cube.TargetSpeedType.UniformSpeed,
                                Cube.MultiWriteType.Write,
                                Cube.ORDER_TYPE.Strong);

            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }
        */


        [UnityTest, Order(13)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_forward() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(100,2,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(14)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_backward() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(-100,2,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(15)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_left() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(100,2,-100,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(16)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_right() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(-100,2,-65535,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(17)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_time() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,200,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(18)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_time_0()
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(10);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(18)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator AccMove_AccToAcc() //
        {
            var cube = test.CreateCube(250, 250, 270);
            cube.AccelerationMove(50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            yield return new WaitForSeconds(2);
            cube.AccelerationMove(-50,10,0,Cube.AccPriorityType.Translation,0,Cube.ORDER_TYPE.Strong);
            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }



    }

}
*/