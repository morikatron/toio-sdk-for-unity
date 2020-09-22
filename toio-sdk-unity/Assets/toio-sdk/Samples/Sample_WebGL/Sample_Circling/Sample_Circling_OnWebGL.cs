using UnityEngine;
using toio;
using toio.Navigation;
using toio.MathUtils;

public class Sample_Circling_OnWebGL : MonoBehaviour
{
    CubeManager cm;
    public Navigator.Mode naviMode = Navigator.Mode.BOIDS;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        cm = new CubeManager();
    }

    public async void Connect()
    {
        await cm.SingleConnect();

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
