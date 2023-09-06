using System.Collections.Generic;
using UnityEngine;
using toio.Navigation;

namespace toio.tutorial
{
    public class NavigatorBasic : MonoBehaviour
    {
        public bool useCubeManager = false;

        // Without CubeManager
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        List<CubeNavigator> navigators;
        bool started = false;

        // With CubeManager
        CubeManager cubeManager;

        async void Start()
        {
            if (!useCubeManager) // Without CubeManager
            {

                var peripheral = await new NearScanner(2).Scan();
                var cubes = await new CubeConnecter().Connect(peripheral);
                this.navigators = new List<CubeNavigator>();
                foreach (var cube in cubes)
                    this.navigators.Add(new CubeNavigator(cube));

                this.started = true;

            }
            else // With CubeManager
            {

                cubeManager = new CubeManager();
                await cubeManager.MultiConnect(2);

            }
        }

        void Update()
        {

            if (!useCubeManager) // Without CubeManager
            {

                if (!started) return;

                elapsedTime += Time.deltaTime;

                if (intervalTime < elapsedTime) // intervalTime=0.05秒 ごとに実行
                {
                    foreach (var navigator in this.navigators)
                        navigator.Update();

                    foreach (var navigator in this.navigators)
                        navigator.handle.MoveRaw(-50, 50, 1000);

                    elapsedTime = 0.0f;
                }

            }
            else // With CubeManager
            {

                // ------ Async ------
                foreach (var navigator in cubeManager.navigators)
                {
                    if (cubeManager.IsControllable(navigator))
                    {
                        navigator.Update();
                        navigator.handle.MoveRaw(-50, 50, 1000);
                    }
                }

                // ------ Sync ------
                // if (cubeManager.synced)
                // {
                //     cubeManager.navigators[0].handle.MoveRaw(-50, 50, 1000);
                // }

                // ------ Sync ------
                // foreach (var navigator in cubeManager.syncNavigators)
                // {
                //     navigator.handle.MoveRaw(-50, 50, 1000);
                // }

            }
        }
    }

}
