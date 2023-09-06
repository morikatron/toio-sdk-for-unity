using UnityEngine;

namespace toio.tutorial
{
    public class HandleToTarget : MonoBehaviour
    {
        int phase = 0;
        CubeManager cubeManager;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.SingleConnect();
        }

        void Update()
        {
            foreach (var handle in cubeManager.syncHandles)
            {
                if (phase == 0)
                {
                    Debug.Log("---------- Phase 0 - Move2Target(250,250)  ----------");
                    Movement mv = handle.Move2Target(250, 250).Exec();

                    if (mv.reached)
                    {
                        Debug.Log("Move2Target(250,250) Reached.");
                        phase = 1;
                    }
                }
                else if (phase == 1)
                {
                    Debug.Log("---------- Phase 1 - Rotate2Deg(-90)  ----------");
                    Movement mv = handle.Rotate2Deg(-90).Exec();
                    // or equally use Rotate2Rad(-Mathf.PI/2) or Rotate2Target(handle.x, 0)

                    if (mv.reached)
                    {
                        Debug.Log("Rotate2Deg(-90) Reached.");
                        phase = 2;
                    }
                }
                else if (phase == 2)
                {
                    Debug.Log("---------- 【おわり】 ----------");
                    phase = 3;
                }

            }

        }
    }

}
