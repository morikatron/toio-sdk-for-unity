using UnityEngine;

namespace toio.tutorial
{
    public class HandleOneShot : MonoBehaviour
    {
        private const float intervalTime = 2f;
        private float elapsedTime = 1.5f;
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
                    Debug.Log("---------- Phase 0 - 指定距離を前進  ----------");
                    Debug.Log("TranslateByDist で100距離前進、赤の速度指令は40、緑は80。");

                    cubeManager.handles[0].TranslateByDist(dist:100, translate:40).Exec();
                    cubeManager.handles[1].TranslateByDist(dist:100, translate:80).Exec();
                }
                else if (phase == 1)
                {
                    Debug.Log("---------- Phase 1 - 指定角度を回転 ----------");
                    Debug.Log("RotateByDeg、RotateByRad で90度回転、赤の速度指令は40、緑は20。");

                    cubeManager.handles[0].RotateByDeg(90, 40).Exec();
                    cubeManager.handles[1].RotateByRad(Mathf.PI/2, 20).Exec();

                    phase = -1;
                }

                elapsedTime = 0.0f;
                phase += 1;
            }

        }
    }

}
