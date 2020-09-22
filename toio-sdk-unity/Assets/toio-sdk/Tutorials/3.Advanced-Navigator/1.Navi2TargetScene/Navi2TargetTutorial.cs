using UnityEngine;

namespace toio.tutorial
{
    public class Navi2TargetTutorial : MonoBehaviour
    {
        CubeManager cubeManager;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.MultiConnect(2);

            // By default, each navigator is able to see all others
            // But you can also manually make a navigator "blind"
            cubeManager.navigators[0].ClearOther();

            cubeManager.navigators[1].usePred = true;
        }

        int navigator0_phase = 0; int navigator1_phase = 0;
        void Update()
        {
            if (cubeManager.synced)
            {
                // navigator 0
                {
                    if (navigator0_phase == 0){
                        var mv = cubeManager.navigators[0].Navi2Target(200, 200, maxSpd:50).Exec();
                        if (mv.reached) navigator0_phase = 1;
                    }
                    else if (navigator0_phase == 1){
                        var mv = cubeManager.navigators[0].Navi2Target(350, 350, maxSpd:50).Exec();
                        if (mv.reached) navigator0_phase = 0;
                    }
                }

                // navigator 1
                {
                    if (navigator1_phase == 0){
                        var mv = cubeManager.navigators[1].Navi2Target(180, 350).Exec();
                        if (mv.reached) navigator1_phase = 1;
                    }
                    else if (navigator1_phase == 1){
                        var mv = cubeManager.navigators[1].Navi2Target(330, 180).Exec();
                        if (mv.reached) navigator1_phase = 0;
                    }
                }
            }
        }
    }

}
