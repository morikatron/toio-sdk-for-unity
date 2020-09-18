using UnityEngine;
using toio.Navigation;

namespace toio.tutorial
{
    public class BoidsAvoidTutorial : MonoBehaviour
    {
        CubeManager cubeManager;
        CubeNavigator navigatorNotBoids;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.MultiConnect(6);

#if UNITY_EDITOR
            foreach (var navigator in cubeManager.navigators){
                if (navigator.cube.id == "Cube (5)")
                    navigatorNotBoids = navigator;
            }
#else
            navigatorNotBoids = cubeManager.navigators[0];
#endif
            navigatorNotBoids.cube.TurnLedOn(255, 0, 0, 0); // Red

            // set to BOIDS_AVOID mode, except Cube (5) (Red)
            foreach (var navigator in cubeManager.navigators){
                navigator.mode = CubeNavigator.Mode.BOIDS_AVOID;
                navigator.usePred = true;
            }

            // By default, all navigators are in one group of boids
            // here, separate Red cube from the group
            foreach (var navigator in cubeManager.navigators)
                navigator.SetRelation(navigatorNotBoids, CubeNavigator.Relation.NONE);
        }

        void Update()
        {
            // ------ Sync ------
            foreach (var navigator in cubeManager.syncNavigators)
            {
                // Cube (5) stay still
                if (navigator != navigatorNotBoids)
                    navigator.Navi2Target(400, 400, maxSpd:50).Exec();
            }
        }
    }

}
