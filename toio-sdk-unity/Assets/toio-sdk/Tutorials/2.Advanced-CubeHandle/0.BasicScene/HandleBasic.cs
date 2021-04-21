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
            cubeManager = new CubeManager();
            await cubeManager.MultiConnect(1);
        }

        public void bu1()
        {
            Debug.Log(126666666666663);
            var cube = cubeManager.cubes[0];
            cubeManager.Disconnect(cube);
        }
        public async void bu2()
        {
            var cube = await cubeManager.SingleConnect();
        }         
    }

}
