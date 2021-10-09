using UnityEngine;
using toio;
using toio.Navigation;
using toio.MathUtils;

public class SVScene3 : MonoBehaviour
{
    CubeManager cm;

    public Navigator.Mode naviMode = Navigator.Mode.BOIDS;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        BLEService.Instance.SetImplement(new BLENetService("192.168.0.9", 50006));

        cm = new CubeManager(ConnectType.Real);
        //await cm.MultiConnect(N);
        cm.MultiConnectAsync(cubeNum:20, coroutineObject:this);

        foreach (var navi in cm.navigators)
        {
            navi.usePred = true;
            navi.mode = naviMode;
            // navi.avoid.useSafety = false;
        }
    }

    void Update()
    {
        var tar = Vector.fromRadMag(Time.time/1, 160) + new Vector(250, 250);
        if (cm.synced){

            for (int i=0; i<cm.navigators.Count; i++)
            {
                var navi = cm.navigators[i];
                navi.mode = naviMode;
                var mv = navi.Navi2Target(tar, maxSpd:60).Exec();
            }
        }
    }
}
