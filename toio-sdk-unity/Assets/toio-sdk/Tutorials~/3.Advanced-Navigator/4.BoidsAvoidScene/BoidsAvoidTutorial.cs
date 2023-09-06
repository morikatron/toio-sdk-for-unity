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
            Debug.Assert(cubeManager.navigators.Count>1, "Need at least 2 cubes.");

            // Choose 1 cube not to be of boids
            navigatorNotBoids = cubeManager.navigators[0];

            if (CubeScanner.actualTypeOfAuto == ConnectType.Simulator)
                foreach (var navigator in cubeManager.navigators)
                    if (navigator.cube.localName == "Cube Not Boids")
                        navigatorNotBoids = navigator;

            // Use LED color to distinguish cubes
            navigatorNotBoids.cube.TurnLedOn(255, 0, 0, 0); // Red

            // Set to BOIDS_AVOID mode, except navigatorNotBoids
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
