using UnityEngine;
using toio;

public class SVScene2 : MonoBehaviour
{
#if UNITY_EDITOR
    CubeManager cubeManager;

    void Start()
    {
        BLEService.Instance.SetImplement(new BLENetService("192.168.0.9", 50006));

        // CubeManagerからモジュールを間接利用した場合:
        cubeManager = new CubeManager(ConnectType.Real);
        // 任意のタイミングで非同期接続
        cubeManager.MultiConnectAsync(cubeNum:20, coroutineObject:this);
    }

    void Update()
    {
        //foreach (var cube in cubeManager.cubes)
        foreach(var cube in cubeManager.syncCubes)
        {
            cube.Move(50, -50, 200);
            Debug.LogFormat("x: {0}, y: {1}", cube.pos.x, cube.pos.y);
        }
    }
#endif
}