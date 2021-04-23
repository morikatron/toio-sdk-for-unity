using System.Collections.Generic;
using UnityEngine;

namespace toio.tutorial
{
    public class HandleBasic : MonoBehaviour
    {

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
        }


        public void Disconnect1()
        {
            Debug.Log("Disconnect1_start");
            var cube = cubeManager.cubes[0];
            cubeManager.Disconnect(cube);
            Debug.Log("Disconnect1_over");
        }
        public void Disconnect2()
        {
            Debug.Log("Disconnect2_start");
            var cube = cubeManager.cubes[1];
            cubeManager.Disconnect(cube);
            Debug.Log("Disconnect2_over");
        }
        public async void SingleCon()
        {
            var cube = await cubeManager.SingleConnect();
        }

        public async void Multi2Con()
        {
            var cubes = await cubeManager.MultiConnect(2);
        }

        public async void Multi1Con()
        {
            var cube = await cubeManager.MultiConnect(1);
        }         
    }

}
