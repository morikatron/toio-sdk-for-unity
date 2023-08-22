using UnityEngine;
using toio;

public class Sample_Cross : MonoBehaviour
{
    CubeManager cm;
    int N = 16;
    int[] phases;

    async void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        cm = new CubeManager();
        await cm.MultiConnect(N);

        foreach (var navi in cm.navigators)
        {
            navi.usePred = true;
            // navi.mode = Navigator.Mode.BOIDS_AVOID;
            navi.avoid.useSafety = false;
            navi.handle.p_mv2tar_reach_pred = -0.2; // make easy to collide, so as to avoid deadlock
        }
        phases = new int[N];
    }

    void Update()
    {
        if (cm.synced)
        for (int i=0; i<cm.navigators.Count; i++)
        {
            var navi = cm.navigators[i];
            int M = 350; int m = 150;

            // this is only intended for use of ConnectType.Auto
            if (
                CubeScanner.actualTypeOfAuto == ConnectType.Simulator && navi.cube.localName[5] == '1' ||
                CubeScanner.actualTypeOfAuto == ConnectType.Real      && i < cm.cubes.Count/2
            ){
                if (phases[i] == 0){
                    var mv = navi.Navi2Target(M, M, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]++;
                }
                else if (phases[i] == 1){
                    var mv = navi.Navi2Target(m, m, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]++;
                }
                else if (phases[i] == 2){
                    var mv = navi.Navi2Target(m, M, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]++;
                }
                else if (phases[i] == 3){
                    var mv = navi.Navi2Target(M, m, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]=0;
                }
            }
            else
            {
                if (phases[i] == 0){
                    var mv = navi.Navi2Target(m, m, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]++;
                }
                else if (phases[i] == 1){
                    var mv = navi.Navi2Target(M, m, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]++;
                }
                else if (phases[i] == 2){
                    var mv = navi.Navi2Target(m, M, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]++;
                }
                else if (phases[i] == 3){
                    var mv = navi.Navi2Target(M, M, maxSpd:50, tolerance:50).Exec();
                    if (mv.reached) phases[i]=0;
                }
            }
        }
    }
}
