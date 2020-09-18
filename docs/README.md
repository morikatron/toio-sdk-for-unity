# toio SDK for Unity ドキュメント

## 基本

### セットアップ
- [事前準備](preparation.md)
- [toio SDK for Unity のインストール](download_sdk.md)

### ビルド
- [iOS ビルド](build_ios.md)
- [ウェブアプリビルド](build_web.md)

## チュートリアル

### 基本

- [概要](tutorials_basic.md#概要)
- [シーン作成](tutorials_basic.md#シーン作成)
- [移動する](tutorials_basic.md#移動する)
- [音を鳴らす](tutorials_basic.md#音を鳴らす)
- [LED を発光する](tutorials_basic.md#LED-を発光する)
- [イベントを検知(ボタン, 傾き, 衝突, 座標と角度, Standard ID)](tutorials_basic.md#イベントを検知ボタン-傾き-衝突-座標と角度-Standard-ID)
- [複数の Cube を動かす](tutorials_basic.md#複数の-Cube-を動かす)
- [ソースコード簡略化](tutorials_basic.md#cubemanagerクラスを用いたソースコードの簡略化)
- [途中接続/途中切断](tutorials_basic.md#途中接続--途中切断)
- [UI の作成](tutorials_UI.md)

### 便利な移動機能 - CubeHandle

- [CubeManager を使ったキューブの同期制御](tutorials_cubehandle.md#CubeManager-を使ったキューブの同期制御)
- [CubeHandle](tutorials_cubehandle.md#CubeHandle)
  - [CubeHandle の Move 関数と MoveRaw 関数](tutorials_cubehandle.md#CubeHandle-の-Move-関数と-MoveRaw-関数)
  - [キューブとの通信量を抑える One-shot メソッド](tutorials_cubehandle.md#キューブとの通信量を抑える-One-shot-メソッド)
  - [指定した座標/方向に到達する Closed-Loop メソッド](tutorials_cubehandle.md#指定した座標方向に到達する-Closed-Loop-メソッド)
- [Follow TargetPole デモ](tutorials_cubehandle.md#Follow-TargetPole-デモ)

### 集団制御 - Navigator

- [CubeNavigator](tutorials_navigator.md#CubeNavigator)
  - [CubeManager を使って CubeNavigator を利用する](tutorials_navigator.md#CubeManager-を使って-CubeNavigator-を利用する)
    - [非同期でキューブを制御する場合](tutorials_navigator.md#非同期でキューブを制御する場合)
    - [同期でキューブを制御する場合](tutorials_navigator.md#同期でキューブを制御する場合)
    - [CubeManager を使わないで CubeNavigator を利用する](tutorials_navigator.md#CubeManager-を使わないで-CubeNavigator-を利用する)
  - [CubeNavigator による衝突回避](tutorials_navigator.md#CubeNavigator-による衝突回避)
    - [衝突を回避しつつ目標に移動する Navi2Target 関数](tutorials_navigator.md#衝突を回避しつつ目標に移動する-Navi2Target-関数)
    - [目標から離れる NaviAwayTarget 関数](tutorials_navigator.md#目標から離れる-NaviAwayTarget-関数)
  - [ボイドによる集団制御](tutorials_navigator.md#ボイドによる集団制御)
  - [ボイド + 衝突回避](tutorials_navigator.md#ボイド--衝突回避)

## サンプル

### 基礎

- [Sample_Circling](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Circling/)

  CubeNavigator のモードの違いによって、多数台のキューブの挙動がどのように変わるか確認するサンプルです。

- [Sample_Cross](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Cross/)

  CubeNavigator を使って多台数のキューブが衝突回避しながら移動をするサンプルです。

- [Sample_Sensor](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Sensor/)

  キューブの各センサ値を検出し、UIに表示するサンプルです。

### 応用

- [Sample_MultiMat](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_MultiMat/)

  複数のマットを併せて一枚の大きいマットとして使うサンプルです。

- [Sample_Bluetooth](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Bluetooth/)

  低レベルモジュールである BLE インタフェースを直接利用してキューブと通信するサンプルです。

- [Sample_WebGL](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_WebGL/)

  ブラウザで動作するウェブアプリのサンプル集です。

- [Sample_Scenes](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Scenes/)

  リアルのキューブとの接続・シミュレータ上のキューブと相関インスタンスを維持したままに、シーン遷移を扱うサンプルです。

## 技術ドキュメント

### 使い方

- [Cube](usage_cube.md)
- [シミュレータ](usage_simulator.md)
- [CubeHandle](usage_cubehandle.md)
- [Navigator](usage_navigator.md)

### 機能説明

- [Cube](sys_cube.md)
- [BLE(Bluetooth Low Energy)](sys_ble.md)
- [シミュレータ](sys_simulator.md)
- [Navigator](sys_navigator.md)

## FAQ

- [FAQ](FAQ.md)

## 留意事項

シミュレータの効果音の再生機能は実装されていますが、効果音自体は実装されていません。
現在は一つだけのダミー効果音が入っています。

効果音を実装するには、[CubeSimulator クラス](../toio-sdk-unity/Assets/toio-sdk/Scripts/Simulator/CubeSimulator.cs)の `_InitPresetSounds` 関数の中で効果音を定義します。

```c#
// Sound Preset を設定
private void _InitPresetSounds(){
    Cube.SoundOperation[] sounds = new Cube.SoundOperation[3];
    sounds[0] = new Cube.SoundOperation(200, 255, 48);
    sounds[1] = new Cube.SoundOperation(200, 255, 50);
    sounds[2] = new Cube.SoundOperation(200, 255, 52);
    this.presetSounds.Add(sounds);
}
```

`Cube.SoundOperation` の説明は [Cube の使い方](usage_cube.md#playsound) を参照してください。
