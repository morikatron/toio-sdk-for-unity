using UnityEngine;

namespace toio.tutorial
{
    public class CubeManagerScene_RawSingle : MonoBehaviour
    {
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        Cube cube;

        async void Start()
        {
            // モジュールを直接利用した場合:
            var peripheral = await new NearestScanner().Scan();
            cube = await new CubeConnecter().Connect(peripheral);
        }

        void Update()
        {
            // モジュールを直接利用した場合:
            if (null == cube) { return; }
            elapsedTime += Time.deltaTime;
            if (intervalTime < elapsedTime)
            {
                elapsedTime = 0.0f;
                cube.Move(50, -50, 200);
            }
        }
    }
}
