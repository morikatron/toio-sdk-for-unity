using System.Collections.Generic;
using UnityEngine;

namespace toio.tutorial
{
    public class LEDScene : MonoBehaviour
    {
        float intervalTime = 5.0f;
        float elapsedTime = 0;
        Cube cube;
        bool started = false;

        // Start is called before the first frame update
        async void Start()
        {
            var peripheral = await new NearestScanner().Scan();
            cube = await new CubeConnecter().Connect(peripheral);
            // 最初に単発発光命令
            cube.TurnLedOn(255, 0, 0, 2000);
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!started) return;

            elapsedTime += Time.deltaTime;

            if (intervalTime < elapsedTime) // 5秒ごとに実行
            {
                elapsedTime = 0.0f;
                // 発光シナリオ
                List<Cube.LightOperation> scenario = new List<Cube.LightOperation>();
                float rad = (Mathf.Deg2Rad * (360.0f / 29.0f));
                for (int i = 0; i < 29; i++)
                {
                    byte r = (byte)Mathf.Clamp((128 + (Mathf.Cos(rad * i) * 128)), 0, 255);
                    byte g = (byte)Mathf.Clamp((128 + (Mathf.Sin(rad * i) * 128)), 0, 255);
                    byte b = (byte)Mathf.Clamp(((Mathf.Abs(Mathf.Cos(rad * i) * 255))), 0, 255);
                    scenario.Add(new Cube.LightOperation(100, r, g, b));
                }
                cube.TurnOnLightWithScenario(3, scenario.ToArray());
            }
        }
    }
}