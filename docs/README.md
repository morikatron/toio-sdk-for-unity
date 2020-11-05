# toio SDK for Unity ドキュメント

## 開発概要

### 【 1 】セットアップ

1. 開発環境を整える [→「事前準備」](preparation.md)
2. Unity で新規プロジェクトを作り、 toio SDK for Unity を読み込む [→「toio SDK for Unity インストール」](download_sdk.md)
<br><br>

### 【 2 】開発

1. 開発前に確認する [→「シミュレータの操作方法」](development_simulator.md)
2. Assets フォルダの下に開発用フォルダを作成する [→「スクリプトの依存関係」](development_basics.md#1-スクリプトの依存関係)
3. 新しいシーンを作って、シミュレータの Prefab を読み込む [→「シーン作成」](tutorials_basic.md#2-シーン作成)
4. Unity Editor で Play してシミュレータで動作確認しながらプログラムを作っていく ★A
<br><br>

### 【 3 】ビルド

1. スマートデバイス向けにビルドしてアプリを作り、端末で動作確認 [→「 iOS ビルド」](build_ios.md) |[「 WebGL ビルド」](build_web.md)　★B
2. ★`A` ★`B` を繰り返してプログラムを仕上げていく

<br>

## チュートリアル

### 基本

- [1. 概要](tutorials_basic.md#1-概要)
- [2. シーン作成](tutorials_basic.md#2-シーン作成)
- [3. 移動する](tutorials_basic.md#3-移動する)
- [4. 音を鳴らす](tutorials_basic.md#4-音を鳴らす)
- [5. LED を発光する](tutorials_basic.md#5-LED-を発光する)
- [6. toio IDの読み取り(Position ID & Standard ID)](tutorials_basic.md#6-toio-IDの読み取りPosition-ID--Standard-ID)
- [7. イベントを検知(ボタン, 傾き, 衝突, 座標と角度, Standard ID)](tutorials_basic.md#7-イベントを検知ボタン-傾き-衝突-座標と角度-Standard-ID)
- [8. 複数のキューブを動かす](tutorials_basic.md#8-複数のキューブを動かす)
- [9. CubeManagerクラスを用いたソースコードの簡略化](tutorials_basic.md#9-cubemanagerクラスを用いたソースコードの簡略化)
- [10. 途中接続/途中切断](tutorials_basic.md#10-途中接続--途中切断)
- [11. UI の作成](tutorials_UI.md)

### 便利な移動機能 - CubeHandle

- [1. CubeManager を使ったキューブの同期制御](tutorials_cubehandle.md#1-CubeManager-を使ったキューブの同期制御)
- [2. CubeHandle](tutorials_cubehandle.md#2-CubeHandle)
  - [2.1. CubeHandle の Move 関数と MoveRaw 関数](tutorials_cubehandle.md#21-CubeHandle-の-Move-関数と-MoveRaw-関数)
  - [2.2. キューブとの通信量を抑える One-shot メソッド](tutorials_cubehandle.md#22-キューブとの通信量を抑える-One-shot-メソッド)
  - [2.3. 指定した座標/方向に到達する Closed-Loop メソッド](tutorials_cubehandle.md#23-指定した座標方向に到達する-Closed-Loop-メソッド)
- [3. Follow TargetPole デモ](tutorials_cubehandle.md#3-Follow-TargetPole-デモ)

### 集団制御 - Navigator

- [1. CubeNavigator](tutorials_navigator.md#1-CubeNavigator)
  - [1.1. CubeManager を使って CubeNavigator を利用する](tutorials_navigator.md#11-CubeManager-を使って-CubeNavigator-を利用する)
    - [1.1.1. 非同期でキューブを制御する場合](tutorials_navigator.md#111-非同期でキューブを制御する場合)
    - [1.1.2. 同期でキューブを制御する場合](tutorials_navigator.md#112-同期でキューブを制御する場合)
    - [1.1.3. CubeManager を使わないで CubeNavigator を利用する](tutorials_navigator.md#113-CubeManager-を使わないで-CubeNavigator-を利用する)
  - [1.2. CubeNavigator による衝突回避](tutorials_navigator.md#12-CubeNavigator-による衝突回避)
    - [1.2.1. 衝突を回避しつつ目標に移動する Navi2Target 関数](tutorials_navigator.md#121-衝突を回避しつつ目標に移動する-Navi2Target-関数)
    - [1.2.2. 目標から離れる NaviAwayTarget 関数](tutorials_navigator.md#122-目標から離れる-NaviAwayTarget-関数)
  - [1.3. ボイドによる集団制御](tutorials_navigator.md#13-ボイドによる集団制御)
  - [1.4. ボイド + 衝突回避](tutorials_navigator.md#14-ボイド--衝突回避)

<br>

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

<br>

## 技術ドキュメント

### システム全体の構成図

<p align="center">
<img src="./res/main/arch.png" width=550></img>
</p>

ローレベル（左）からハイレベル（右）に説明して行きます。
- Simulator：Unity Editor で実行できるシミュレータ
- BLE：スマートデバイスやウェブで、リアルのキューブとブルートゥース通信するモジュール
- Cube：シミュレータとリアルの両方を統一し、キューブを扱うクラス
- CubeHandle：便利な移動機能をまとめたクラス
- Navigator：高度な集団制御を実現したクラス
- CubeManager：複数のキューブと各種の機能を便利に管理するクラス

### 使い方

- [Cubeクラス](usage_cube.md)
- [シミュレータ](usage_simulator.md)
- [CubeHandleクラス](usage_cubehandle.md)
- [Navigatorクラス](usage_navigator.md)

### 機能説明

- [Cubeクラス](sys_cube.md)
- [BLE(Bluetooth Low Energy)](sys_ble.md)
- [シミュレータ](sys_simulator.md)
- [Navigatorクラス](sys_navigator.md)

<br>

## FAQ

- [FAQ](FAQ.md)

<br>

## 留意事項

- シミュレータの効果音の再生機能について  

    シミュレータの効果音の再生機能は実装されていますが、効果音自体は実装されていません。現在は一つだけのダミー効果音が入っています。  
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
    `Cube.SoundOperation` の説明は [Cube クラスの使い方](usage_cube.md#playsound) を参照してください。
