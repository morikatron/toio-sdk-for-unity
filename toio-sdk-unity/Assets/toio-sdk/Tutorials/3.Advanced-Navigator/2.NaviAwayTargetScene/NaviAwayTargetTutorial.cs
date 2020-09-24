using UnityEngine;

namespace toio.tutorial
{
    public class NaviAwayTargetTutorial : MonoBehaviour
    {
        CubeManager cubeManager;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.MultiConnect(2);
            Debug.Assert(cubeManager.navigators.Count>1, "Need at least 2 cubes.");

            // By default, each navigator is able to see all others
            // But you can also manually make a navigator "blind"
            cubeManager.navigators[0].ClearOther();
            cubeManager.navigators[1].ClearOther();
        }

        void Update()
        {
            // ------ Sync ------
            if (cubeManager.synced)
            {
                var navi0 = cubeManager.navigators[0];
                var navi1 = cubeManager.navigators[1];

                // navigator 0
                {
                    var mv = navi0.Navi2Target(navi1.handle.pos, maxSpd:50).Exec();
                }

                // navigator 1
                {
                    var mv = navi1.NaviAwayTarget(navi0.handle.pos, maxSpd:80).Exec();
                }
            }
        }
    }

}
