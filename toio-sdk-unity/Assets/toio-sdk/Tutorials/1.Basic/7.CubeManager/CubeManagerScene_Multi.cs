using UnityEngine;

namespace toio.tutorial
{
    public class CubeManagerScene_Multi : MonoBehaviour
    {
        CubeManager cubeManager;

        async void Start()
        {
            // CubeManagerからモジュールを間接利用した場合:
            cubeManager = new CubeManager();
            // 一斉に同期接続
            await cubeManager.MultiConnect(12);
        }

        void Update()
        {
            // CubeManagerからモジュールを間接利用した場合:
            foreach(var cube in cubeManager.cubes)
            {
                if (cubeManager.IsControllable(cube))
                {
                    cube.Move(50, -50, 200);
                }
            }
        }
    }
}
