using System.Collections.Generic;
using UnityEngine;
using toio;

namespace toio.tutorial
{
    public class Sample_UI_Update : MonoBehaviour
    {
        CubeManager cubeManager;
        Cube cube;
        int mode = 0;

        async void Start()
        {
            cubeManager = new CubeManager();
            cube = await cubeManager.SingleConnect();
        }

        void Update()
        {
            if (cubeManager.IsControllable(cube) && CubeOrderBalancer.Instance.IsIdle(cube))
            {
                if (0 == mode)
                {
                    cube.Move(0, 0, 50);
                }
                else if (1 == mode)
                {
                    cube.Move(30, 60, 100);
                }
                else if (2 == mode)
                {
                    cube.Move(60, 60, 100);
                }
                else if (3 == mode)
                {
                    cube.Move(60, 30, 100);
                }
                else if (4 == mode)
                {
                    cube.Move(-40, -40, 100);
                }
            }
        }

        // update関数内で毎回modeに応じたMove命令を送っているため、時々パケ落ちしても大きな問題にならない。
        // そのため命令の優先度はCube.ORDER_TYPE.Weakにする(デフォルト)
        public void Forward() { mode = 2; Move(30, 30, 100); }
        public void Backward() { mode = 4; Move(-30, -30, 100); }
        public void TurnRight() { mode = 3; Move(30, 10, 100); }
        public void TurnLeft() { mode = 1; Move(10, 30, 100); }
        public void Stop() { mode = 0; Move(0, 0, 50); }
        public void PlayPresetSound1() { cube.PlayPresetSound(1); }
        public void PlayPresetSound2() { cube.PlayPresetSound(2); }
        public void LedOn()
        {
            List<Cube.LightOperation> scenario = new List<Cube.LightOperation>();
            float rad = (Mathf.Deg2Rad * (360.0f / 29.0f));
            for (int i = 0; i < 29; i++)
            {
                byte r = (byte)Mathf.Clamp((128 + (Mathf.Cos(rad * i) * 128)), 0, 255);
                byte g = (byte)Mathf.Clamp((128 + (Mathf.Sin(rad * i) * 128)), 0, 255);
                byte b = (byte)Mathf.Clamp(((Mathf.Abs(Mathf.Cos(rad * i) * 255))), 0, 255);
                scenario.Add(new Cube.LightOperation(100, r, g, b));
            }
            cube.TurnOnLightWithScenario(0, scenario.ToArray());
        }
        public void LedOff() { cube.TurnLedOff(); }

        void Move(int left, int right, int durationMs)
        {
            if (cubeManager.IsControllable(cube))
            {
                cube.Move(left, right, durationMs);
            }
        }
    }
}