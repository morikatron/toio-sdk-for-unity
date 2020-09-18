using System.Collections.Generic;
using UnityEngine;

namespace toio.tutorial
{
    public class HandleBasic : MonoBehaviour
    {
        public bool useCubeManager = false;

        // Without CubeManager
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        List<CubeHandle> handles;
        bool started = false;

        // With CubeManager
        CubeManager cubeManager;

        async void Start()
        {
            if (!useCubeManager) // Without CubeManager
            {

                var peripheral = await new NearScanner(2).Scan();
                var cubes = await new CubeConnecter().Connect(peripheral);
                this.handles = new List<CubeHandle>();
                foreach (var cube in cubes)
                    this.handles.Add(new CubeHandle(cube));

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
                    foreach (var handle in this.handles)
                        handle.Update();

                    foreach (var handle in this.handles)
                        handle.MoveRaw(-50, 50, 1000);

                    elapsedTime = 0.0f;
                }

            }
            else // With CubeManager
            {

                // ------ Async ------
                foreach (var handle in cubeManager.handles)
                {
                    if (cubeManager.IsControllable(handle))
                    {
                        handle.Update();
                        handle.MoveRaw(-50, 50, 1000);
                    }
                }

                // ------ Sync ------
                // if (cubeManager.synced)
                // {
                //     cubeManager.handles[0].MoveRaw(-50, 50, 1000);
                // }

                // ------ Sync ------
                // foreach (var handle in cubeManager.syncHandles)
                // {
                //     handle.MoveRaw(-50, 50, 1000);
                // }

            }
        }
    }

}
