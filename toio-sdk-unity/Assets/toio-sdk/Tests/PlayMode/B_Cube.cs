using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Assertions;
using System.IO;
using Cysharp.Threading.Tasks;

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
    public class B_Cube : CubeTestCase
    {
        [UnityTest, Order(0)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator move()
        {
            cubeManager.cubes[GetCubeIdxFromHomeIdx(0)].Move(10, 10, 1000);
            test.update = test.UpdateForSeconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(1)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator playSound()
        {
            List<Cube.SoundOperation> score = new List<Cube.SoundOperation>();
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.A6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.D6));
            cubeManager.cubes[GetCubeIdxFromHomeIdx(0)].PlaySound(1, score.ToArray(), Cube.ORDER_TYPE.Strong);

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator playPresetSound()
        {
            cubeManager.cubes[GetCubeIdxFromHomeIdx(0)].PlayPresetSound(0);
            test.update = test.UpdateForSeconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(3)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator stopSound()
        {
            List<Cube.SoundOperation> score = new List<Cube.SoundOperation>();
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.A6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 100, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 100, Cube.NOTE_NUMBER.D6));
            var cube = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            cube.PlaySound(1, score.ToArray(), Cube.ORDER_TYPE.Strong);
            var start_time = Time.time;
            bool playing = true;

            test.update =  (() =>
            {
                var elapsed = Time.time - start_time;
                if (1 < elapsed && playing)
                {
                    playing = false;
                    cube.StopSound();
                }
                if (2 < elapsed)
                {
                    return true;
                }
                return false;
            });

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(4)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator turnLedOn()
        {
            cubeManager.cubes[GetCubeIdxFromHomeIdx(0)].TurnLedOn(255, 0, 0, 1000);
            test.update = test.UpdateForSeconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(5)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator turnLedOff()
        {
            var cube = cubeManager.cubes[GetCubeIdxFromHomeIdx(0)];
            cube.TurnLedOn(0, 0, 255, 1000);

            var start_time = Time.time;
            bool on = true;
            test.update = (() =>
            {
                var elapsed = Time.time - start_time;
                if (1 < elapsed && on)
                {
                    on = false;
                    cube.TurnLedOff();
                }
                if (2 < elapsed)
                {
                    return true;
                }
                return false;
            });
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(6)] // テストの実行の優先度を指定する(昇順)
        public static IEnumerator turnOnLightWithScenario()
        {
            List<Cube.LightOperation> score = new List<Cube.LightOperation>();
            score.Add(new Cube.LightOperation(500, 255, 0, 0));
            score.Add(new Cube.LightOperation(500, 0, 255, 0));
            score.Add(new Cube.LightOperation(500, 0, 0, 255));
            cubeManager.cubes[GetCubeIdxFromHomeIdx(0)].TurnOnLightWithScenario(3, score.ToArray());

            test.update = test.UpdateForSeconds(5);
            yield return new MonoBehaviourTest<test>();
        }
    }
}