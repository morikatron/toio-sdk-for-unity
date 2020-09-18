using UnityEngine;

namespace toio.tutorial
{
    public class HandleMove : MonoBehaviour
    {
        private const float intervalTime = 1.7f;
        private float elapsedTime = 1.2f;
        private int phase = 0;
        CubeManager cubeManager;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.MultiConnect(2);
            cubeManager.cubes[0].TurnLedOn(255,0,0,0);
            cubeManager.cubes[1].TurnLedOn(0,255,0,0);
        }

        void Update()
        {
            elapsedTime += Time.deltaTime;

            if (intervalTime < elapsedTime)
            {
                cubeManager.handles[0].Update();
                cubeManager.handles[1].Update();

                if (phase == 0)
                {
                    Debug.Log("---------- Phase 0 - 右回転 ----------");
                    Debug.Log("赤キューブは MoveRaw(50, -50, 560)、緑キューブは move(0, 100, 560)で同じ回転をできる。");

                    // MoveRawで右回転：　左モーター指令 50、右モーター指令 -50、継続時間 560
                    cubeManager.handles[0].MoveRaw(50, -50, 560);

                    // moveで右回転：　前進指令 0、回転指令 50、(希望)継続時間 560
                    cubeManager.handles[1].Move(0, 100, 560);
                }
                else if (phase == 1)
                {
                    Debug.Log("---------- Phase 1 - 前進 ----------");
                    Debug.Log("赤キューブは MoveRaw(80, 80, 600)、緑キューブは move(80, 0, 600)で同じ前進をできる。");

                    // MoveRawで前進：　左モーター指令 80、右モーター指令 80、継続時間 600
                    cubeManager.handles[0].MoveRaw(80, 80, 600);

                    // moveで前進：　前進指令 80、回転指令 0、(希望)継続時間 600
                    cubeManager.handles[1].Move(80, 0, 600);
                }
                else if (phase == 2)
                {
                    Debug.Log("---------- Phase 2 - デッドゾン対処 ----------");
                    Debug.Log("MoveRaw と move は入力表現が違う。move の方がもっと直感的になっている。MoveRaw はほぼそのままの入力を Cube.move に渡すが、move は【デッドゾン対処】と【ボーダー制限】を行っている。");
                    Debug.Log("赤キューブは MoveRaw(-9, 1, 100)、緑キューブは等価に move(-4, -10, 100)、デッドゾンは 10。");
                    Debug.Log("緑キューブだけが動いた。");

                    // MoveRawで前進：　左モーター指令 -9、右モーター指令 1、継続時間 100
                    cubeManager.handles[0].MoveRaw(-9, 1, 100);

                    // moveで前進：　前進指令 -4、回転指令 -10、(希望)継続時間 100
                    cubeManager.handles[1].Move(-4, -10, 100);
                }
                else if (phase == 3)
                {
                    Debug.Log("---------- Phase 3 - ボーダー ----------");
                    Debug.Log("赤キューブは MoveRawで前進、緑キューブは moveで同じ前進を。");
                    Debug.Log("緑キューブはボーダー前に止まった。");

                    // MoveRawで前進：　左モーター指令 100、右モーター指令 100、継続時間 1000
                    cubeManager.handles[0].MoveRaw(100, 100, 1000);

                    // moveで前進：　前進指令 100、回転指令 0、(希望)継続時間 1000
                    cubeManager.handles[1].Move(100, 0, 1000);
                }
                else if (phase == 4)
                {
                    Debug.Log("---------- Phase 4 - ボーダー ----------");
                    Debug.Log("ボーダー制限は、予測モデルによって継続時間を制限することで、後退や回転の場合でも効く。");

                    // moveで前進：　前進指令 80、回転指令 30、(希望)継続時間 2000
                    cubeManager.handles[1].Move(-80, 30, 2000);
                }
                else if (phase == 5)
                {
                    Debug.Log("---------- 【おわり】 ----------");
                    Debug.Log("ありがとうございます。");
                }

                elapsedTime = 0.0f;
                phase += 1;
            }

        }
    }

}
