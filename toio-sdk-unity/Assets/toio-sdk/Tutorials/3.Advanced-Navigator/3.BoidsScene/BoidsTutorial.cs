using UnityEngine;
using toio.Navigation;

namespace toio.tutorial
{
    public class BoidsTutorial : MonoBehaviour
    {
        CubeManager cubeManager;
        bool started = false;

        async void Start()
        {
            cubeManager = new CubeManager();
            await cubeManager.MultiConnect(6);
            Debug.Assert(cubeManager.navigators.Count>1, "Need at least 2 cubes.");

            // Choose 1 cube not to be of boids
            CubeNavigator navigatorNotBoids = cubeManager.navigators[0];

            if (CubeScanner.actualTypeOfAuto == ConnectType.Simulator)
                foreach (var navigator in cubeManager.navigators)
                    if (navigator.cube.localName == "Cube Not Boids")
                        navigatorNotBoids = navigator;

            // Use LED color to distinguish cubes
            foreach (var navigator in cubeManager.navigators)
            {
                if (navigator == navigatorNotBoids) navigator.cube.TurnLedOn(255,0,0,0); // Red
                else navigator.cube.TurnLedOn(0,255,0,0);  // Green
            }

            // Set to BOIDS only mode, except navigatorNotBoids
            foreach (var navigator in cubeManager.navigators)
                if (navigator != navigatorNotBoids) navigator.mode = CubeNavigator.Mode.BOIDS;

            // By default, all navigators are in one group of boids
            // here, separate Red cube from the group
            navigatorNotBoids.SetRelation(cubeManager.navigators, CubeNavigator.Relation.NONE);
            foreach (var navigator in cubeManager.navigators)
                navigator.SetRelation(navigatorNotBoids, CubeNavigator.Relation.NONE);

            started = true;
        }

        void Update()
        {
            if (!started) return;
            // ------ Sync ------
            foreach (var navigator in cubeManager.syncNavigators)
            {
                var mv = navigator.Navi2Target(400, 400, maxSpd:50).Exec();
            }
        }
    }

}
