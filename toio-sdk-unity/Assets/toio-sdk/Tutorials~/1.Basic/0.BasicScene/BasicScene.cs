using UnityEngine;
using toio;

namespace toio.tutorial
{
    public class BasicScene : MonoBehaviour
    {
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        Cube cube;

        // 非同期初期化
        // C#標準機能であるasync/awaitキーワードを使用する事で、検索・接続それぞれで終了待ちする
        // async: 非同期キーワード
        // await: 待機キーワード
        async void Start()
        {
            // Bluetoothデバイスを検索
            var peripheral = await new NearestScanner().Scan();
            // デバイスへ接続してCube変数を生成
            cube = await new CubeConnecter().Connect(peripheral);
        }

        void Update()
        {
            // Cube変数の生成が完了するまで早期リターン
            if (null == cube) { return; }
            // 経過時間を計測
            elapsedTime += Time.deltaTime;

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
