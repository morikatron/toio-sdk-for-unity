using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using toio.Simulator;
using toio.Navigation;


public class Sample_VisualizeNavigator : MonoBehaviour
{
    CubeManager cubeManager;
    Stage stage;
    bool reached = false;

    async void Start()
    {
        cubeManager = new CubeManager();
        await cubeManager.SingleConnect();

        var navi = cubeManager.navigators[0];
        navi.ClearWall();
        // Set new borders
        navi.AddBorder(10, 60, 440, 60, 440);
        // Set triangle shaped walls
        navi.AddWall(new Wall(250, 200, 200, 300, 10));
        navi.AddWall(new Wall(250, 200, 300, 300, 10));
        navi.AddWall(new Wall(200, 300, 300, 300, 10));

        // Get stage
        stage = GameObject.FindObjectOfType<Stage>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var navi in cubeManager.syncNavigators)
        {
            Movement mv = navi.Navi2Target(stage.targetPoleCoord).Exec();

            Visualize(navi);

            if (mv.reached && !reached)
            {
                Debug.Log($"Navi2Target({stage.targetPoleCoord}) Reached.");
                reached = true;
            }
            if (!mv.reached) reached = false;
        }
    }

    void Visualize(CubeNavigator navi)
    {
    #if UNITY_EDITOR

        // Draw Avoid
        if (navi.mode==Navigator.Mode.AVOID || navi.mode==Navigator.Mode.BOIDS_AVOID)
        {
            // Draw Scan Result
            var res = navi.avoid.scanResult;
            for (int i=0; i<res.rads.Length; i++)
            {
                var waypoint = res.points[i];
                Color color = Color.blue;

                // Colorize some waypoints RED if collide
                if (res.isCollision)
                {
                    if (navi.avoid.useSafety)
                    {
                        // Unsafe waypoints colorized RED
                        if (res.safety[i] < navi.avoid.p_waypoint_safety_threshold)
                            color = Color.red;
                    }
                    // All colorized RED
                    else color = Color.red;
                }

                // Colorize Chosen waypoint GREEN
                if (i==navi.avoid.waypointIndex) color = Color.green;

                DrawLine(
                    navi.entity.x, navi.entity.y,
                    navi.entity.x + waypoint.x, navi.entity.y + waypoint.y,
                    color
                );
            }
        }

        // Draw Wall
        foreach (var wall in navi.Walls())
        {
            DrawLine(
                wall.point1.x, wall.point1.y,
                wall.point2.x, wall.point2.y,
                Color.black
            );
        }

    #endif
    }

    void DrawLine(double x1, double y1, double x2, double y2, Color color)
    {
        var upos1 = stage.mat.MatCoord2UnityCoord((float)x1, (int)y1);
        var upos2 = stage.mat.MatCoord2UnityCoord((float)x2, (int)y2);
        Debug.DrawLine(upos1, upos2, color, 0.05f);
    }
}
