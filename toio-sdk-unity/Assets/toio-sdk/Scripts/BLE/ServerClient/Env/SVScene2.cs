using System.Net;
using UnityEngine;
using toio;
using toio.ble.net;

public class SVScene2 : MonoBehaviour
{
//#if UNITY_EDITOR
    CubeManager cubeManager;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        BLEService.Instance.SetImplement(new BLENetService(this.gameObject, BLENetClient.GetLocalIPAddress(), BLENetProtocol.S_PORT));

        // CubeManagerからモジュールを間接利用した場合:
        cubeManager = new CubeManager(ConnectType.Real);
        // 任意のタイミングで非同期接続
        cubeManager.MultiConnectAsync(cubeNum:20, coroutineObject:this);
    }

    void Update()
    {
        //foreach (var cube in cubeManager.cubes)
        /*
        foreach(var cube in cubeManager.syncCubes)
        {
            cube.Move(50, -50, 200);
            Debug.LogFormat("x: {0}, y: {1}", cube.pos.x, cube.pos.y);
        }*/

        if (1 <= cubeManager.cubes.Count)
        {
            if (cubeManager.IsControllable(cubeManager.cubes[0])) cubeManager.cubes[0].Move(50, -50, 200);
        }
        if (2 <= cubeManager.cubes.Count)
        {
            if (cubeManager.IsControllable(cubeManager.cubes[1])) cubeManager.cubes[1].Move(-50, 50, 200);
        }
    }
//#endif
}