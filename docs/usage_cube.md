# 目次

- [1. 概説](usage_cube.md#1-概説)
- [2. 既存 toio™ ライブラリ(toio.js)との比較](usage_cube.md#2-既存toioライブラリtoiojsとの比較)
- [3. Cube クラス API](usage_cube.md#3-Cube-クラス-API)
  - [3.1. 変数](usage_cube.md#31-変数)
  - [3.2. コールバック](usage_cube.md#32-コールバック)
  - [3.3. メソッド](usage_cube.md#33-メソッド)
  <br>

# 1. 概説

Cube クラスは、toio™コア キューブ (以降、キューブ) を操作するための機能が実装されています。<br>
このクラスは、異なる環境向けの実装切り替えが出来るマルチプラットフォーム対応クラスとなっており、
Unity システム上で動くキューブ(以下シミュレータ) と 現実のキューブ の 2 つの実行環境に対応しています。

シミュレータについては[コチラ](usage_simulator.md)のページをご参照下さい。

現実のキューブでは、[toio™コア キューブ技術仕様(通信仕様)](https://toio.github.io/toio-spec/docs/ble_communication_overview.html)に沿って Unity プログラムから Bluetooth 通信を行う事で、現実のキューブを操作します。

### Real/Sim 機能表

現在(2020/03/04)、キューブのファームウェアバージョンは 2 つです。

- 2.0.0 : 公開時の初期バージョン
- 2.1.0 : 公開後の初アップデート

toio SDK for Unity では、現実に動作するキューブクラス(Real 対応)、シミュレータで動作するキューブクラス(Sim 対応)の 2 つの内部実装が用意されています。それぞれ内部実装が異なっているため、対応状況に違いがあります。<br>
以下に実装対応表を示します。

#### ファームウェアバージョン 2.0.0

| 機能タイプ         | 機能                                                                                                                           | Real 対応状況 | Sim 対応状況 |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------ | ------------- | ------------ |
| 読み取りセンサー   | [Position ID](https://toio.github.io/toio-spec/docs/2.0.0/ble_id#position-id)                                                  | o             | o            |
|                    | [Standard ID](https://toio.github.io/toio-spec/docs/2.0.0/ble_id#standard-id)                                                  | o             | o            |
| モーションセンサー | [水平検出](https://toio.github.io/toio-spec/docs/2.0.0/ble_sensor#水平検出)                                                    | o             | o            |
|                    | [衝突検出](https://toio.github.io/toio-spec/docs/2.0.0/ble_sensor#衝突検出)                                                    | o             | ※            |
| ボタン             | [ボタンの状態](https://toio.github.io/toio-spec/docs/2.0.0/ble_button#ボタンの状態)                                            | o             | o            |
| バッテリー         | [バッテリー残量](https://toio.github.io/toio-spec/docs/2.0.0/ble_battery#バッテリー残量)                                       | o             | x            |
| モーター           | [モーター制御](https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#モーター制御)                                             | x             | x            |
|                    | [時間指定付きモーター制御](https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#時間指定付きモーター制御)                     | o             | o            |
| ランプ             | [点灯・消灯](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#点灯-消灯)                                                  | o             | o            |
|                    | [連続的な点灯・消灯](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#連続的な点灯-消灯)                                  | o             | o            |
|                    | [全てのランプを消灯](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#全てのランプを消灯)                                 | x             | x            |
|                    | [特定のランプを消灯](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#特定のランプを消灯)                                 | x             | x            |
| サウンド           | [効果音の再生](https://toio.github.io/toio-spec/docs/2.0.0/ble_sound#効果音の再生)                                             | o             | Δ            |
|                    | [MIDI note number の再生](https://toio.github.io/toio-spec/docs/2.0.0/ble_sound#midi-note-number-の再生)                       | o             | o            |
|                    | [再生の停止](https://toio.github.io/toio-spec/docs/2.0.0/ble_sound#再生の停止)                                                 | o             | o            |
| 設定               | [BLE プロトコルバージョンの要求](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#ble-プロトコルバージョンの要求) | o             | x            |
|                    | [水平検出のしきい値設定](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#水平検出のしきい値設定)                 | o             | o            |
|                    | [衝突検出のしきい値設定](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#衝突検出のしきい値設定)                 | o             | x            |
|                    | [BLE プロトコルバージョンの取得](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#ble-プロトコルバージョンの取得) | o             | x            |

#### ファームウェアバージョン 2.1.0

| 機能タイプ         | 機能                                                                                                                                | Real 対応状況 | Sim 対応状況 |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| モーションセンサー | [ダブルタップ検出（new）](https://toio.github.io/toio-spec/docs/ble_sensor#ダブルタップ検出)                                        | o             | ※            |
|                    | [姿勢検出（new）](https://toio.github.io/toio-spec/docs/ble_sensor#姿勢検出)                                                        | o             | o            |
| モーター           | [モーター制御（指示値範囲変更）](https://toio.github.io/toio-spec/docs/ble_motor#モーターの速度指示値)                              | o             | o            |
|                    | [目標指定付きモーター制御（new）](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御)                         | x             | x            |
|                    | [複数目標指定付きモーター制御（new）](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御)                 | x             | x            |
|                    | [加速度指定付きモーター制御（new）](https://toio.github.io/toio-spec/docs/ble_motor#加速度指定付きモーター制御)                     | x             | x            |
|                    | [目標指定付きモーター制御の応答（new）](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御の応答)             | x             | x            |
|                    | [複数目標指定付きモーター制御の応答（new）](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御の応答)     | x             | x            |
| 設定               | [ダブルタップ検出の時間間隔の設定（new）](https://toio.github.io/toio-spec/docs/ble_configuration#ダブルタップ検出の時間間隔の設定) | x             | x            |

> ※ … シミュレータ側に検出機能は実装されていませんが、インスペクター上から手動で判定の有無を切り替えることが出来ます。 詳細は[【コチラ】](usage_simulator.md#41-CubeSimulator-のインスペクター)をご確認ください。

<br>

# 2. 既存 toio™ ライブラリ(toio.js)との比較

[toio.js](https://github.com/toio/toio.js)は、javascript で書かれた既存の toio ライブラリです。<br>
このライブラリは node.js で作られており、PC 環境で動作します。<br>
scanner、cube の 2 つをクラスを利用してキューブを操作します。

以下に toio.js のサンプルコードを示します。

```javascript
const { NearestScanner } = require("@toio/scanner");

async function main() {
  // start a scanner to find the nearest cube
  const cube = await new NearestScanner().start();

  // connect to the cube
  await cube.connect();

  // move cube
  cube.move(100, 100, 1000);
  //         |    |     `--- duration [ms]
  //         |    `--------- right motor speed
  //          `------------- left motor speed
}

main();
```

toio SDK for Unity は toio.js ユーザーでも使いやすいプログラムを目指し、
scanner、cube の 2 つのクラスを使用してキューブを操作する仕組みにしました。

以下に、同じ挙動をする本プログラムのサンプルコードを示します。

```C#
public class SimpleScene : MonoBehaviour
{
    async void Start()
    {
        // start a scanner to find the nearest cube
        var peripheral = await new NearestScanner().Scan();

        // connect to the cube
        var cube = await new CubeConnecter().Connect(peripheral);

        // move cube
        cube.Move(100, 100, 1000);
        //         |    |     `-- duration [ms]
        //         |    `-------- right motor speed
        //          `------------ left motor speed
    }
}
```

<br>

# 3. Cube クラス API

## 3.1. 変数

```c#
// 接続したキューブのファームウェアバージョン
public string version { get; }

// キューブの固有識別ID
// シミュレータ環境では Cube ゲームオブジェクトの InstanceID が ID になります
public string id { get; protected set; }

// キューブのアドレス
public string addr { get; }

// キューブの接続状態
public bool isConnected { get; }

// キューブのバッテリー状態
public int battery { get; protected set; }

// マット上のキューブのX座標
// キューブの中心位置を基準とした座標となっています。
// マットの上にいる間、自動更新されます。
// コールバック機能：standardIdCallback
public int x { get; protected set; }

// マット上のキューブのY座標
// キューブの中心位置を基準とした座標となっています。
// マットの上にいる間、自動更新されます。
// コールバック機能：standardIdCallback
public int y { get; protected set; }

// マット上のキューブのXY座標
// キューブの中心位置を基準とした座標となっています。
// マットの上にいる間、自動更新されます。
// コールバック機能：standardIdCallback
public Vector2 pos { get; }

// マット上のキューブの角度
// キューブの中心位置を基準とした角度になっています。
// マットの上にいる間、自動更新されます。
// コールバック機能：standardIdCallback
public int angle { get; protected set; }

// マット上のキューブのXY座標
// キューブの光学センサー位置を基準とした座標になっています。
// マットの上にいる間、自動更新されます。
// コールバック機能：standardIdCallback
public Vector2 sensorPos { get; }

// マット上のキューブの角度
// キューブの光学センサー位置を基準とした角度になっています。
// マットの上にいる間、自動更新されます。
// コールバック機能：standardIdCallback
public int sensorAngle { get; protected set; }

// 読み取り可能な特殊ステッカーのID
// キューブの光学センサーから取得したIDとなっています。
// コールバック機能：standardIdCallback
public uint standardId { get; protected set; }

// キューブのボタン押下状態の変数
// コールバック機能：buttonCallback
public bool isPressed { get; protected set; }

// キューブの傾き状態の変数
// コールバック機能：slopeCallback
public bool isSloped { get; protected set; }

// キューブの衝突状態の変数
// 内部実装は継承クラスによって大きく異なる可能性があります。
// コールバック機能：collisionCallback
public bool isCollisionDetected { get; protected set; }

// キューブがマット上にいるか判定する変数
public bool isGrounded { get; protected set; }

// キューブの最高速度を表す変数
// ファームウェアバージョン毎に異なるため用意されています。
public int maxSpd { get; }
```

<br>

## 3.2. コールバック

```C#
public class CallbackProvider
{
    public virtual void AddListener(string key, Action<Cube> listener);
    public virtual void RemoveListener(string key);
    public virtual void ClearListener();
    public virtual void Notify(Cube target);
}

// ボタンコールバック
public virtual CallbackProvider buttonCallback { get; }
// 傾きコールバック
public virtual CallbackProvider slopeCallback { get; }
// 衝突コールバック
public virtual CallbackProvider collisionCallback { get; }
// 座標角度コールバック
public virtual CallbackProvider idCallback { get; }
// 座標角度 Missed コールバック
public virtual CallbackProvider idMissedCallback { get; }
// StandardID コールバック
public virtual CallbackProvider standardIdCallback { get; }
// StandardID Missed コールバック
public virtual CallbackProvider standardIdMissedCallback { get; }
```

## 3.3. メソッド

### Move

```C#
public void Move(int left, int right, int durationMs, ORDER_TYPE order=ORDER_TYPE.Weak);
```

キューブのモーターを制御します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_motor#時間指定付きモーター制御)

- left
  - 定義 : 左モーター速度
  - 範囲 :
    - [Version 2.0.0] -100 ~ -10； -9 ~ 9 は 0 に等価； 10 ~ 100
    - [Version 2.1.0] -115 ~ -8； -7 ~ 7 は 0 に等価； 8 ~ 115
- right
  - 定義 : 右モーター速度
  - 範囲 :
    - [Version 2.0.0] -100 ~ -10； -9 ~ 9 は 0 に等価； 10 ~ 100
    - [Version 2.1.0] -115 ~ -8； -7 ~ 7 は 0 に等価； 8 ~ 115
- durationMs
  - 定義 : 持続時間(ミリ秒)
  - 範囲 :
    - 0~9 : 時間制限なし
    - 10~2550 : 精度は 10ms、1 位が省略される
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### TurnLedOn

```C#
public void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブ底面についている LED を制御します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_light#点灯-消灯)

- red
  - 定義 : 赤色の強さ
  - 範囲 : 0~255
- green
  - 定義 : 緑色の強さ
  - 範囲 : 0~255
- blue
  - 定義 : 青色の強さ
  - 範囲 : 0~255
- durationMs
  - 定義 : 持続時間(ミリ秒)
  - 範囲 :
    - 0~9 : 時間制限なし
    - 10~2550 : 精度は 10ms、1 位が省略される
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### TurnOnLightWithScenario

```C#
// 発光ごとの設定構造体
public struct LightOperation
{
    public Int16 durationMs; // ミリ秒
    public byte red;         // 赤色の強さ
    public byte green;       // 緑色の強さ
    public byte blue;        // 青色の強さ
}
public void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブ底面についている LED を連続的に制御します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_light#連続的な点灯-消灯)

- repeatCount
  - 定義 : 繰り返し回数
  - 範囲 : 0~255
- operations
  - 定義 : 命令配列
  - 個数 : 1~59
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### TurnLedOff

```C#
public void TurnLedOff(ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブ底面についている LED を消灯させます<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_light#全てのランプを消灯)

- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### PlayPresetSound

```C#
public void PlayPresetSound(int soundId, int volume=255, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブからあらかじめ用意された効果音を再生します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#効果音の再生)

- soundId
  - 定義 : サウンド ID
  - 範囲 : 0~10
- volume:
  - 定義 : 音量
  - 範囲 : 0~255
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### PlaySound

キューブから任意の音を再生します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生)

```C#
// 引数版
// 発音ごとの設定構造体
public struct SoundOperation
{
    public Int16 durationMs; // ミリ秒
    public byte volume;      // 音量(0~255)
    public byte note_number; // 音符(0~128)
}
public void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order=ORDER_TYPE.Strong);
```

- repeatCount
  - 定義 : 繰り返し回数
  - 範囲 : 0~255
- operations
  - 定義 : 命令配列
  - 個数 : 1~29
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

```C#
// バッファ版
public void PlaySound(byte[] buff, ORDER_TYPE order=ORDER_TYPE.Strong);
```

- buff
  - 定義 : 命令プロコトル
  - [toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生)
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### StopSound

```C#
public void StopSound(ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブの音再生を停止します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#再生の停止)

- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### ConfigSlopeThreshold

```C#
public void ConfigSlopeThreshold(int angle, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブの水平検出のしきい値を設定します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_configuration#水平検出のしきい値設定)

- angle
  - 定義 : 傾き検知の閾値
  - 範囲 : 1~45
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### ConfigCollisionThreshold

```C#
public void ConfigCollisionThreshold(int level, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブの衝突検出のしきい値を設定します<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_configuration#衝突検出のしきい値設定)

- level
  - 定義 : 衝突検知の閾値
  - 範囲 : 1~10
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>
