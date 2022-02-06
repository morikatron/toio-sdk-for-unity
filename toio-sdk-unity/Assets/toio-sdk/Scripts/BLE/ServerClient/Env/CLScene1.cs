using UnityEngine;
using toio;
using toio.ble.net;

public class CLScene1 : MonoBehaviour
{
    public int cubeNum = 20;
    CubeManager cm;
    BLENetClient client;

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        client = new BLENetClient();
        //client.Start(listenAddr:BLENetClient.GetLocalIPAddress(), listenPort:BLENetProtocol.C_PORT, serverAddr:BLENetClient.GetLocalIPAddress(), serverPort:BLENetProtocol.S_PORT);
        client.Start(listenAddr:BLENetClient.GetLocalIPAddress(), listenPort:BLENetProtocol.C_PORT, serverAddr:"192.168.0.9", serverPort:BLENetProtocol.S_PORT);



        cm = new CubeManager(ConnectType.Real);
        cm.MultiConnectAsync(cubeNum:cubeNum, coroutineObject:this, connectedAction:OnConnected);
    }

    void Update()
    {
        client.Update();
    }

    void OnConnected(Cube cube, CONNECTION_STATUS status)
    {
        var cubereal = cube as CubeReal;
        if (status.IsNewConnected)
        {
            client.JoinCube(cm.cubes.Count-1, cubereal);
        }
        else if (status.IsReConnected)
        {
            for (int i = 0; i < cm.cubes.Count; i++)
            {
                if (cube.localName == cm.cubes[i].localName)
                    client.JoinCube(i, cubereal);
            }
        }
    }
}
