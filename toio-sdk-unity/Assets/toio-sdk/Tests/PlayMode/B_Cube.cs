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
    public class B_Cube : CubePlayModeBase
    {
        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator move()
        {
            Start();
            var cube = test.CreateCube(300, 300);
            cube.Move(100, 100, 2000);

            test.update = TestUntil_Seconds(5);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator playSound()
        {
            Start();
            var cube = test.CreateCube(300, 300);
            List<Cube.SoundOperation> score = new List<Cube.SoundOperation>();
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.A6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.D6));
            cube.PlaySound(1, score.ToArray(), Cube.ORDER_TYPE.Strong);

            test.update = TestUntil_Seconds(5);

            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator playPresetSound()
        {
            Start();

            var cube = test.CreateCube(300, 300);
            cube.PlayPresetSound(0);

            test.update = TestUntil_Seconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator stopSound()
        {
            Start();
            var cube = test.CreateCube(300, 300);
            List<Cube.SoundOperation> score = new List<Cube.SoundOperation>();
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.A6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.G6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(300, 15, Cube.NOTE_NUMBER.NO_SOUND));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.C6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.D6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.F6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.E6));
            score.Add(new Cube.SoundOperation(150, 15, Cube.NOTE_NUMBER.D6));
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

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator turnLedOn()
        {
            Start();

            var cube = test.CreateCube(300, 300);
            cube.TurnLedOn(255, 0, 0, 1000);

            test.update = TestUntil_Seconds(2);
            yield return new MonoBehaviourTest<test>();
        }

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator turnLedOff()
        {
            Start();

            var cube = test.CreateCube(300, 300);
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

        [UnityTest, Order(2)] // テストの実行の優先度を指定する(昇順)
        public IEnumerator turnOnLightWithScenario()
        {
            Start();
            var cube = test.CreateCube(300, 300);
            List<Cube.LightOperation> score = new List<Cube.LightOperation>();
            score.Add(new Cube.LightOperation(500, 255, 0, 0));
            score.Add(new Cube.LightOperation(500, 0, 255, 0));
            score.Add(new Cube.LightOperation(500, 0, 0, 255));
            cube.TurnOnLightWithScenario(3, score.ToArray());

            test.update = TestUntil_Seconds(5);

            yield return new MonoBehaviourTest<test>();
        }
    }
}