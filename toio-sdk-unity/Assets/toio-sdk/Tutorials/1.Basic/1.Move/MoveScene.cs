using UnityEngine;
using toio;

namespace toio.tutorial
{
    public class MoveScene : MonoBehaviour
    {
        float intervalTime = 2.0f;
        float elapsedTime = 0;
        Cube cube;
        bool started = false;
        int phase = 0;

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

            if (intervalTime < elapsedTime) // 2秒ごとに実行
            {
                if (phase == 0)
                {
                    Debug.Log("---------- Phase 0 - 右回転 ----------");
                    // 右回転：左モーター指令 50、右モーター指令 -50、継続時間 1500ms
                    cube.Move(50, -50, 1500);
                }
                else if (phase == 1)
                {
                    Debug.Log("---------- Phase 1 - 前進 ----------");
                    // MoveRawで前進：左モーター指令 20、右モーター指令 20、継続時間 1500ms
                    cube.Move(20, 20, 1500);
                }
                else if (phase == 3)
                {
                    Debug.Log("---------- Phase 3 - 左回り ----------");
                    // MoveRawで前進：左モーター指令 100、右モーター指令 70、継続時間 1000ms
                    cube.Move(100, 70, 1800);
                }
                else if (phase == 4)
                {
                    Debug.Log("---------- Phase 4 - 後進 ----------");
                    // MoveRawで前進：左モーター指令 -100、右モーター指令 -100、継続時間 500
                    cube.Move(-100, -100, 500);
                }
                else if (phase == 5)
                {
                    Debug.Log("---------- Phase 5 - 左回転 ----------");
                    // moveで前進：前進指令 -100、回転指令 100、(希望)継続時間 2000
                    cube.Move(-100, 100, 2000);
                }
                else if (phase == 6)
                {
                    Debug.Log("---------- 【リセット】 ----------");
                    phase = -1;
                }

                elapsedTime = 0.0f;
                phase += 1;
            }
        }
    }
}