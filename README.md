# toio SDK for Unity

> **Switch Language:**　`日本語`　|　[`English`](README_EN.md)

## 概要

**toio SDK for Unity** は、Unity で toio™コア キューブ（以降キューブ）を制御するための開発環境です。

toio SDK for Unity を使用することでキューブを使ったアプリを簡単に開発できるようになります。またシミュレータ機能によって、実機を使わずに画面上でキューブの動作を確認できるため、迅速かつ効率的に開発可能です。


<p align="center">
<img src="./docs/res/main/overview.gif" width=720>
</p>

toio SDK for Unity の一つ大きな特徴は、このような一つのソースコードでシミュレータとリアルのキューブの両方を動かすことが出来ることです。

```csharp
using UnityEngine;
using toio;

public class Hello_Toio : MonoBehaviour
{
    CubeManager cubeManager;
    Cube cube;

    async void Start()
    {
        // create a cube manager
        cubeManager = new CubeManager();
        // connect to the nearest cube
        cube = await cubeManager.SingleConnect();
    }

    void Update()
    {
        // check connection status and order interval
        if(cubeManager.IsControllable(cube))
        {
            cube.Move(100, 70, 200);
            //         |    |   `--- duration [ms]
            //         |    `------- right motor speed
            //         `------------ left motor speed
        }
    }
}
```


## 機能一覧

- シミュレータ
  - Unity Editor でキューブを動かせる
  - Drag&Drop、力で引っ張る、押す等のインタラクションが可能
  - 公式又はカスタムのマットを複数枚設置できる
  - トイオ・コレクションと開発者向けの Standard ID を設置できる
  - シミュレータ用に［マット+ライト+カメラ］を便利なワンセットとして用意
- BLE通信モジュール
  - iOS アプリでキューブと通信する
  - ウェブアプリでキューブと通信する
  - Android アプリでキューブと通信する
  - MacのUnity EditorでPlay時にキューブと通信する
- Cube
  - 一つのソースコードで、シミュレータとリアルの両方のキューブを同じく動かせる
  - キューブをスキャン、接続、再接続できる
- CubeHandle（便利な移動機能）
  - キューブを目標位置、角度へ誘導できる
  - キューブがマットから出ないよう、ボーダー制限をかけられる
  - 一回の呼び出しで一定距離の移動や角度の変化が可能
- Navigator（高度な集団制御）
  - ヒューマンライク衝突回避で、複数台のキューブが互いに衝突回避できる
  - ボイドで、複数台のキューブを群れとして動かせる
  - 衝突回避とボイドは組み合わせて同時実行できる
  - 目標に移動する以外に目標から離れるようなナビゲーションもできる
- Visual Scripting
  - Unity2021から標準となったVisual ScriptingでCube, CubeHandle, Navigatorが利用できる

## 動作環境

- toio™コア キューブ
- toio™専用マット（トイオ・コレクション付属のプレイマット／toio™コア キューブ（単体）付属の簡易プレイマット／toio 開発用プレイマット）
- Mac（macOS ver.10.14以上）
- Windows 10（64 ビット版のみ）
- iOS端末（iOS ver.12以上）
- Android端末(Android OS 9.0以上)
- Unity（2021.2.17f1）
- Unity Visual Scripting(ver 1.7.6)

## ドキュメント

インストール、チュートリアル、サンプル紹介、機能解説などの詳細については以下のドキュメントをご参照ください。

- [「toio SDK for Unity ドキュメント」](docs/README.md)

## サンプルアプリ

- [CubeMarker](https://github.com/morikatron/toio-cube-marker)：オンライン対戦サンプルプログラム

## ライセンス

- [LICENSE](https://github.com/morikatron/toio-sdk-for-unity/blob/main/LICENSE)
- [Third Party Notices](Third-Party-Notices.md)
- [知的財産権表記 / Trademark Notice](Trademark-Notices.md)
