using System;
using System.Collections;
using UnityEngine;
using toio;
using Cysharp.Threading.Tasks;


namespace toio.tutorial
{
    public class CubeManagerScene_RawReconnect : MonoBehaviour
    {
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        Cube cube;
        CubeConnecter connecter;

        async void Start()
        {
            // モジュールを直接利用した場合:
            var peripheral = await new NearestScanner().Scan();
            connecter = new CubeConnecter();
            cube = await connecter.Connect(peripheral);

            // 切断・再接続のループを開始
            if (cube != null) StartCoroutine(LoopConnection());
        }

        IEnumerator LoopConnection()
        {
            yield return new WaitForSeconds(3);

            // 切断 （モジュールを直接利用した場合）
            connecter.Disconnect(cube);
            yield return new WaitUntil(() => !cube.isConnected);
            yield return new WaitForSeconds(3);

            // 再接続 （モジュールを直接利用した場合）
            connecter.ReConnect(cube);
            yield return new WaitUntil(() => cube.isConnected);

            StartCoroutine(LoopConnection());
        }

        void Update()
        {
            // 回転（モジュールを直接利用した場合）
            if (null == cube) { return; }
            elapsedTime += Time.deltaTime;
            if (intervalTime < elapsedTime)
            {
                elapsedTime = 0.0f;
                cube.Move(50, -50, 200);
            }
        }
    }
}
