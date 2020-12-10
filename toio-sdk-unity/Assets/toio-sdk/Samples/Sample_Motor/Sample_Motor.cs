using UnityEngine;
using toio;

public class Sample_Motor : MonoBehaviour
{
    Cube[] cubes;
    bool started = false;
    int phase = -1;
    float lastTime = 0;

    async void Start()
    {
        // Cube の接続
        var peripherals = await new NearScanner(2).Scan();
        cubes = await new CubeConnecter().Connect(peripherals);

        foreach (var cube in cubes)
        {
            cube.targetMoveCallback.AddListener("Sample_Motor", TargetMoveCallback);
        }
        started = true;
    }

    void TargetMoveCallback(Cube cube, int configID, Cube.TargetMoveRespondType response)
    {
        Debug.LogFormat("Cube#{0}'s TargetMove ends with response code: {1}", configID, (byte)response);
        phase ++;
    }

    void Update()
    {
        if (!started) return;
        if (cubes.Length==0) return;

        if (Time.time-lastTime < 0.05f) return;
        lastTime = Time.time;

        switch (phase)
        {
            case -1:    // Start TargetMove
            {
                Debug.Log("====== Start TargetMove ======");
                if (cubes.Length==1)
                    cubes[0].TargetMove(targetX:250, targetY:250, targetAngle:270, configID:0);
                else
                {
                    if (cubes[0].x > cubes[1].x)    // cubes[0] on the right, so goes to right target
                    {
                        cubes[0].TargetMove(targetX:270, targetY:250, targetAngle:270, configID:0);
                        cubes[1].TargetMove(targetX:230, targetY:250, targetAngle:90, configID:1);
                    }
                    else                            // cubes[0] on the left, so goes to left target
                    {
                        cubes[1].TargetMove(targetX:270, targetY:250, targetAngle:270, configID:0);
                        cubes[0].TargetMove(targetX:230, targetY:250, targetAngle:90, configID:1);
                    }
                }
                phase = 0;
                break;
            }
            case 1: case 2: // TargetMove ends & Start AccelerationMove
            {
                if (phase==cubes.Length)    // if all cubes' TargetMove end.
                {
                    foreach (var cube in cubes)
                        // Immediately set speed to 30 by setting acceleration to 0
                        cube.AccelerationMove(targetSpeed:30, acceleration:0, rotationSpeed:-100, controlTime:0);
                    phase = 3;
                }
                break;
            }
            case 3:     // Start AccelerationMove
            {
                Debug.Log("====== Start AccelerationMove ======");
                foreach (var cube in cubes)
                    // Accelerate from speed 30 to 115
                    cube.AccelerationMove(targetSpeed:115, acceleration:2, rotationSpeed:-100, controlTime:0);
                phase ++;
                break;
            }
            case 4:     // End AccelerationMove
            {
                bool allOut = false;
                foreach (var cube in cubes)
                {
                    if ( (cube.x-250)*(cube.x-250)+(cube.y-250)*(cube.y-250) > 140*140 ) allOut = true;
                }
                if (allOut) phase = -1;
                break;
            }
        }
    }
}
