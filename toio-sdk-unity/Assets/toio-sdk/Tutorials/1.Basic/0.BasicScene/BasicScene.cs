using UnityEngine;
using toio;

namespace toio.tutorial
{
    public class BasicScene : MonoBehaviour
    {
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        float connectTime = 0;
        Cube cube;
        CubeConnecter connecter;

        // 非同期初期化
        // C#標準機能であるasync/awaitキーワードを使用する事で、検索・接続それぞれで終了待ちする
        // async: 非同期キーワード
        // await: 待機キーワード
        async void Start()
        {
            // Bluetoothデバイスを検索
            var peripheral = await new NearestScanner().Scan();
            // デバイスへ接続してCube変数を生成
            connecter = new CubeConnecter();
            cube = await connecter.Connect(peripheral);
            connectTime = Time.time;
        }

        void Update()
        {
            // Cube変数の生成が完了するまで早期リターン
            if (null == cube) { return; }
            // 経過時間を計測
            elapsedTime += Time.deltaTime;

            // 5秒間隔で、切断と再接続を繰り返す （リアルキューブのみに有効）
            if (Time.time > connectTime + 5f)
            {
                // 接続済みの場合は、切断
                if (cube.isConnected)
                {
                    connecter.Disconnect(cube);
                }
                // 切断された場合は、再接続
                else
                {
                    connecter.ReConnect(cube);
                }
                connectTime = Time.time;
            }

            // 接続された場合は動く
            if (cube.isConnected)
            {
                // 前回の命令から50ミリ秒以上経過した場合
                if (intervalTime < elapsedTime)
                {
                    elapsedTime = 0.0f;
                    // 左モーター速度:50, 右モーター速度:-50, 制御時間:200ミリ秒
                    cube.Move(50, -50, 200);
                }
            }
        }
    }
}
