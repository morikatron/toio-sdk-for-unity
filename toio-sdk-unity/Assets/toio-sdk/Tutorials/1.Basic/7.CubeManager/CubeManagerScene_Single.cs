using UnityEngine;

namespace toio.tutorial
{
    public class CubeManagerScene_Single : MonoBehaviour
    {
        CubeManager cubeManager;
        Cube cube;

        async void Start()
        {
            // CubeManagerからモジュールを間接利用した場合:
            cubeManager = new CubeManager();
            cube = await cubeManager.SingleConnect();
        }

        void Update()
        {
            // CubeManagerからモジュールを間接利用した場合:
            if (cubeManager.IsControllable(cube))
            {
                cube.Move(50, -50, 200);
            }
        }
    }
}
