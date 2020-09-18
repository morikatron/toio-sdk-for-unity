using UnityEngine;

namespace toio.tutorial
{
    public class CubeManagerScene_MultiAsync : MonoBehaviour
    {
        CubeManager cubeManager;

        void Start()
        {
            // CubeManagerからモジュールを間接利用した場合:
            cubeManager = new CubeManager();
            // 任意のタイミングで非同期接続
            cubeManager.MultiConnectAsync(
                cubeNum:4,
                coroutineObject:this,
                connectedAction:OnConnected
            );
        }

        void Update()
        {
            foreach (var cube in cubeManager.cubes)
            {
                if (cubeManager.IsControllable(cube))
                {
                    cube.Move(50, -50, 200);
                }
            }
        }

        void OnConnected(Cube cube, CONNECTION_STATUS status)
        {
            if (status.IsNewConnected)
            {
                Debug.Log("new-connected!!");
            }
            else if (status.IsReConnected)
            {
                Debug.Log("re-connected!!");
            }
        }
    }
}
