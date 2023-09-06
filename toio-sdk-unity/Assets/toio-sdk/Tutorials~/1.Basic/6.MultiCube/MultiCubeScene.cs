using UnityEngine;

namespace toio.tutorial
{
    public class MultiCubeScene : MonoBehaviour
    {
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        Cube[] cubes;
        bool started = false;

        async void Start()
        {
            var peripherals = await new NearScanner(12).Scan();
            cubes = await new CubeConnecter().Connect(peripherals);
            started = true;
        }

        void Update()
        {
            if (!started) { return; }

            elapsedTime += Time.deltaTime;
            if (intervalTime < elapsedTime)
            {
                elapsedTime = 0.0f;
                foreach(var cube in cubes)
                {
                    cube.Move(60, 20, 200);
                }
            }
        }
    }
}