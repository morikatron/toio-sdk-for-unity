using UnityEngine;
using System.Linq;

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
            cubeManager.navigators[0].AddBorder(20);
            cubeManager.navigators[1].AddBorder(20);

            // Corner is easy to stuck, so add some 45° walls in corners.
            Navigation.Wall[] walls = {
                new Navigation.Wall(50, 150, 150, 50, 10),
                new Navigation.Wall(450, 350, 350, 450, 10),
                new Navigation.Wall(50, 350, 150, 450, 10),
                new Navigation.Wall(450, 150, 350, 50, 10),
            };
            cubeManager.navigators[0].AddWall(walls.ToList());
            cubeManager.navigators[1].AddWall(walls.ToList());
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
