using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

namespace toio.tutorial
{
    public class toioIDScene : MonoBehaviour
    {
        float intervalTime = 0.1f;
        float elapsedTime = 0;
        Cube cube;
        bool started = false;

        async void Start()
        {
            var peripheral = await new NearestScanner().Scan();
            cube = await new CubeConnecter().Connect(peripheral);
            started = true;
        }

        void Update()
        {
            if (!started) return;

            elapsedTime += Time.deltaTime;

            if (intervalTime < elapsedTime) // 0.1秒ごとに実行
            {
                elapsedTime = 0.0f;

                // 手法A： y 座標で発光の強度を決める
                var strength = (510 - cube.y)/2;
                // 手法B： x 座標で発光の強度を決める
                // var strength = (510 - cube.x)/2;
                // 手法C： pos と中央の距離で発光の強度を決める
                // var strength = (int)(255 - (cube.pos-new Vector2(255,255)).magnitude);

                // Standard ID によって発光の色を決める （初期値は０）
                if (cube.standardId == 3670337) // Simple Card "A"
                    cube.TurnLedOn(strength, 0, 0, 0);
                else if (cube.standardId == 3670080) // toio collection skunk yellow
                    cube.TurnLedOn(0, strength, 0, 0);
                else if (cube.standardId == 3670016) // toio collection card typhoon
                    cube.TurnLedOn(0, 0, strength, 0);
                else cube.TurnLedOff();
            }
        }
    }
}
