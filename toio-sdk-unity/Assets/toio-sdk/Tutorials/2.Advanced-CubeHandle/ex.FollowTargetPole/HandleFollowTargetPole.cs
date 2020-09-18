using UnityEngine;
using toio.Simulator;

namespace toio.tutorial
{
    public class HandleFollowTargetPole : MonoBehaviour
    {
        CubeManager cubeManager;
        Stage stage;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.SingleConnect();

            // Get stage
            stage = GameObject.FindObjectOfType<Stage>();
        }

        bool reached = false;
        void Update()
        {

            foreach (var handle in cubeManager.syncHandles)
            {
                Movement mv = handle.Move2Target(stage.targetPoleCoord).Exec();

                if (mv.reached && !reached)
                {
                    Debug.Log($"Move2Target({stage.targetPoleCoord}) Reached.");
                    reached = true;
                }
                if (!mv.reached) reached = false;
            }

        }
    }

}
