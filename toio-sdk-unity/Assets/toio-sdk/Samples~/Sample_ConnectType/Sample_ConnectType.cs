using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class Sample_ConnectType : MonoBehaviour
{
    public ConnectType connectType;

    CubeManager cm;
    async void Start()
    {
        // ConnectType.Auto - ビルド対象に応じて内部実装が自動的に変わる
        // ConnectType.Simulator - ビルド対象に関わらずシミュレータのキューブで動作する
        // ConnectType.Real - ビルド対象に関わらずリアル(現実)のキューブで動作する
        cm = new CubeManager(connectType);
        await cm.MultiConnect(2);
    }


    void Update()
    {
        foreach(var cube in cm.syncCubes)
        {
            cube.Move(50, -50, 100);
        }
    }
}
