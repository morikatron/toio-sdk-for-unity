using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace toio.tutorial
{
    public class CubeManagerScene_Reconnect : MonoBehaviour
    {
        CubeManager cubeManager;
        Cube cube;

        async void Start()
        {
            // CubeManagerからモジュールを間接利用した場合:
            cubeManager = new CubeManager();
            cube = await cubeManager.SingleConnect();

            // 切断・再接続のループを開始
            if (cube != null) StartCoroutine(LoopConnection());
        }

        IEnumerator LoopConnection()
        {
            yield return new WaitForSeconds(3);

            // 切断 （CubeManager 利用した場合）
            cubeManager.DisconnectAll();    // ALT: cubeManager.Disconnect(cube);
            yield return new WaitUntil(() => !cube.isConnected);
            yield return new WaitForSeconds(3);

            // 再接続 （CubeManager 利用した場合）
            cubeManager.ReConnectAll();     // ALT: cubeManager.ReConnect(cube);
            yield return new WaitUntil(() => cube.isConnected);

            StartCoroutine(LoopConnection());
        }

        void Update()
        {
            // CubeManagerからモジュールを間接利用した場合:
            if (cubeManager.IsControllable(cube))
            {
                cube.Move(50, -50, 200);
            }
        }
    }
}
