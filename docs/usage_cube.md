# 技術ドキュメント - 使い方 - Cubeクラス

## 目次

- [1. 概説](usage_cube.md#1-概説)
- [2. 既存 toio™ ライブラリ(toio.js)との比較](usage_cube.md#2-既存-toio-ライブラリtoiojsとの比較)
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

現在(2020/12/15)、キューブのファームウェアバージョンは 3 つです。

`2.0.0`　`2.1.0`　`2.2.0`

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

> ※ … シミュレータ側に検出機能は実装されていませんが、インスペクター上から手動で判定の有無を切り替えることが出来ます。 詳細は[【コチラ】](usage_simulator.md#41-CubeSimulator-のインスペクター)をご確認ください。

#### ファームウェアバージョン 2.1.0

| 機能タイプ         | 機能                                                                                                                                | Real 対応状況 | Sim 対応状況 |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| モーションセンサー | [ダブルタップ検出](https://toio.github.io/toio-spec/docs/ble_sensor#ダブルタップ検出)                                        | o             | ※            |
|                    | [姿勢検出](https://toio.github.io/toio-spec/docs/ble_sensor#姿勢検出)                                                        | o             | o            |
| モーター           | [モーター制御（指示値範囲変更）](https://toio.github.io/toio-spec/docs/ble_motor#モーターの速度指示値)                       | o             | o            |
|                    | [目標指定付きモーター制御](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御)                         | o             | o            |
|                    | [複数目標指定付きモーター制御](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御)                 | o             | o            |
|                    | [加速度指定付きモーター制御](https://toio.github.io/toio-spec/docs/ble_motor#加速度指定付きモーター制御)                     | o             | o            |
|                    | [目標指定付きモーター制御の応答](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御の応答)             | o             | o            |
|                    | [複数目標指定付きモーター制御の応答](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御の応答)     | o             | o            |
| 設定               | [ダブルタップ検出の時間間隔の設定](https://toio.github.io/toio-spec/docs/ble_configuration#ダブルタップ検出の時間間隔の設定) | o             | x            |

#### ファームウェアバージョン 2.2.0

| 機能タイプ         | 機能                                                                                                                                       | Real 対応状況 | Sim 対応状況 |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------- | ------------ |
| モーションセンサー | [シェイク検出](https://toio.github.io/toio-spec/docs/ble_sensor#シェイク検出)                                                       | o             | o            |
| 磁気センサー       | [磁気センサー情報の要求](https://toio.github.io/toio-spec/docs/ble_magnetic_sensor#磁気センサー情報の要求)                          | x             | x            |
|                    | [磁気センサー情報の取得](https://toio.github.io/toio-spec/docs/ble_magnetic_sensor#磁気センサー情報の取得)                          | x             | x            |
| モーター           | [モーターの速度情報の取得](https://toio.github.io/toio-spec/docs/ble_motor#モーターの速度情報の取得)                                | o             | o            |
| 設定               | [磁気センサーの設定](https://toio.github.io/toio-spec/docs/ble_configuration#磁気センサーの設定)                                    | x             | x            |
|                    | [磁気センサーの設定の応答](https://toio.github.io/toio-spec/docs/ble_configuration#[磁気センサーの設定の応答)                       | x             | x            |
|                    | [モーターの速度情報の取得の設定](https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定)            | o             | o            |
|                    | [モーターの速度情報の取得の設定の応答](https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定の応答)| o             | o            |
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

// キューブの最低速度を表す変数
// ファームウェアバージョン毎に異なるため用意されています。
public int deadzone { get; }

// ver2.1.0
// キューブのダブルタップ状態
// 一度タップされてから一定時間内に再度タップされます。
// コールバック機能：doubleTapCallback
public bool isDoubleTap { get; protected set; }

// キューブの姿勢
// キューブの水平面に対する姿勢が変化したときに値が変わります。
// コールバック機能：poseCallback
public PoseType pose { get; protected set; }

// ver2.2.0
// キューブのシェイク状態
// キューブを振ると振った強さに応じて値が変わります。
// コールバック機能：shakeCallback
public bool isShake { get; protected set; }

// キューブのモーター ID 1（左）の速度
// コールバック機能：motorSpeedCallback
public int leftSpeed { get; protected set; }

// キューブのモーター ID 2（右）の速度
// コールバック機能：motorSpeedCallback
public int rightSpeed { get; protected set; }
```

<br>

## 3.2. コールバック

```C#
// CallbackProvider<T1>
// CallbackProvider<T1, T2>
// CallbackProvider<T1, T2, T3>
// CallbackProvider<T1, T2, T3, T4>
// ※疑似コード
public class CallbackProvider<T...>
{
    public virtual void AddListener(string key, Action<T...> listener);
    public virtual void RemoveListener(string key);
    public virtual void ClearListener();
    public virtual void Notify(T... args);
}

// ボタンコールバック
public virtual CallbackProvider<Cube> buttonCallback { get; }
// 傾きコールバック
public virtual CallbackProvider<Cube> slopeCallback { get; }
// 衝突コールバック
public virtual CallbackProvider<Cube> collisionCallback { get; }
// 座標角度コールバック
public virtual CallbackProvider<Cube> idCallback { get; }
// 座標角度 Missed コールバック
public virtual CallbackProvider<Cube> idMissedCallback { get; }
// StandardID コールバック
public virtual CallbackProvider<Cube> standardIdCallback { get; }
// StandardID Missed コールバック
public virtual CallbackProvider<Cube> standardIdMissedCallback { get; }

// ver2.1.0
// ダブルタップコールバック
public virtual CallbackProvider<Cube> doubleTapCallback { get; }
// 姿勢検出コールバック
public virtual CallbackProvider<Cube> poseCallback { get; }
// 目標指定付きモーター制御の応答コールバック
public virtual CallbackProvider<Cube, int, TargetMoveRespondType> targetMoveCallback { get; }
// 複数目標指定付きモーター制御の応答コールバック
public virtual CallbackProvider<Cube, int, TargetMoveRespondType>  multiTargetMoveCallback { get; }

// ver2.2.0
// シェイクコールバック
public virtual CallbackProvider<Cube> shakeCallback { get; }
// モータースピードコールバック
public virtual CallbackProvider<Cube> motorSpeedCallback { get; }
```

## 3.3. メソッド

### Move

```C#
public void Move(int left, int right, int durationMs, ORDER_TYPE order=ORDER_TYPE.Weak);
```

キューブのモーターを制御します。<br>
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

キューブ底面についている LED を制御します。<br>
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
    public int durationMs; // ミリ秒
    public byte red;         // 赤色の強さ
    public byte green;       // 緑色の強さ
    public byte blue;        // 青色の強さ
}
public void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブ底面についている LED を連続的に制御します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_light#連続的な点灯-消灯)

- repeatCount
  - 定義 : 繰り返し回数
  - 範囲 : 0~255
- operations
  - 定義 : 命令配列
  - 個数 : 1~29
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### TurnLedOff

```C#
public void TurnLedOff(ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブ底面についている LED を消灯させます。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_light#全てのランプを消灯)

- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### PlayPresetSound

```C#
public void PlayPresetSound(int soundId, int volume=255, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブ内に用意されている効果音を再生します。<br>
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

キューブから任意の音を再生します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生)

```C#
// 引数版
// 発音ごとの設定構造体
public struct SoundOperation
{
    public int durationMs; // ミリ秒
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
  - 個数 : 1~59
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

```C#
// バッファ版
public void PlaySound(byte[] buff, ORDER_TYPE order=ORDER_TYPE.Strong);
```

- buff
  - 定義 : [toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生)で定義されたデータブロック
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### StopSound

```C#
public void StopSound(ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブの音再生を停止します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_sound#再生の停止)

- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### ConfigSlopeThreshold

```C#
public void ConfigSlopeThreshold(int angle, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブの水平検出のしきい値を設定します。<br>
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

キューブの衝突検出のしきい値を設定します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_configuration#衝突検出のしきい値設定)

- level
  - 定義 : 衝突検知の閾値
  - 範囲 : 1~10
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### ConfigDoubleTapInterval

```C#
public void ConfigDoubleTapInterval(int interval, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブのダブルタップ検出の時間間隔を設定します <br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_configuration#ダブルタップ検出の時間間隔の設定)

- interval
  - 定義 : ダブルタップ検出の時間間隔
  - 範囲 : 1~7
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### TargetMove

```C#
public void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            byte configID = 0,
            byte timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            byte maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            TargetRotationType targetRotationType = TargetRotationType.AbsoluteLeastAngle,
            ORDER_TYPE order = ORDER_TYPE.Strong);
```
キューブのモーターを目標指定付き制御します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御)

- targetX
  - 定義 : 目標地点の X 座標値
  - 範囲 : -1, 0~65534
    - -1の場合、X 座標は書き込み操作時と同じに設定
- targetY
  - 定義 : 目標地点の Y 座標値
  - 範囲 : -1, 0~65534
    - -1の場合、Y 座標は書き込み操作時と同じに設定
- targetAngle
  - 定義 : 目標地点でのキューブの角度Θ
  - 範囲 : 0~8191
- configID
  - 定義 : 制御識別値、制御の応答を識別するための値。ここで設定した値が対応する応答にも含まれる
  - 範囲 : 0~255
- timeOut
  - 定義 : タイムアウト時間
  - 範囲 : 0~255
    -  0 のみ例外的に 10 秒になる
- targetMoveType
  - 定義 : 移動タイプ
  - 種類 :
    - RotatingMove : 回転しながら移動
    - RoundForwardMove : 回転しながら移動（後退なし）
    - RoundBeforeMove : 回転してから移動
- maxSpd
  - 定義 : モーターの最大速度指示値
  - 範囲 : 10~255
- targetSpeedType
  - 定義 : モーターの速度変化タイプ
  - 種類 :
    - UniformSpeed : 速度一定
    - Acceleration : 目標地点まで徐々に加速
    - Deceleration : 目標地点まで徐々に減速
    - VariableSpeed : 中間地点まで徐々に加速し、そこから目標地点まで減速
- targetRotationType
  - 定義 : 目標地点でのキューブの角度Θのタイプ（意味）
  - 種類 :
    - AbsoluteLeastAngle : 絶対角度・回転量が少ない方向
    - AbsoluteClockwise : 絶対角度・正方向(時計回り)
    - AbsoluteCounterClockwise : 絶対角度・負方向(反時計回り)
    - RelativeClockwise : 相対角度・正方向(時計回り)
    - RelativeCounterClockwise : 相対角度・負方向(反時計回り)
    - NotRotate : 回転しない
    - Original : 書き込み操作時と同じ・回転量が少ない方向
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

### ConfigMotorRead

```C#
public UniTask ConfigMotorRead(bool valid, float timeOutSec=0.5f, Action<bool,Cube> callback=null, ORDER_TYPE order=ORDER_TYPE.Strong);
```

キューブのモーター速度情報の取得の有効化・無効化を設定します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定)

- valid
  - 定義 : 有効無効フラグ
  - 種類 : true, false
- timeOutSec
  - 定義 : タイムアウト(秒)
  - 範囲 : 0.5~
- callback
  - 定義 : 終了コールバック(設定成功フラグ, キューブ)
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### MultiTargetMove

```C#
public void MultiTargetMove(
            int[] targetXList,
            int[] targetYList,
            int[] targetAngleList,
            TargetRotationType[] multiRotationTypeList = null,
            byte configID = 0,
            byte timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            byte maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            MultiWriteType multiWriteType = MultiWriteType.Write,
            ORDER_TYPE order = ORDER_TYPE.Strong);
```

キューブの複数目標指定付きモーター制御を実行します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御)

- targetXList
  - 定義 : 各目標地点の X 座標値の配列
  - 範囲 : -1, 0~65534
    - -1の場合、X 座標は書き込み操作時と同じに設定
- targetYList
  - 定義 : 各目標地点の Y 座標値の配列
  - 範囲 : -1, 0~65534
    - -1の場合、Y 座標は書き込み操作時と同じに設定
- targetAngleList
  - 定義 : 目標地点でのキューブの角度Θの配列
  - 範囲 : 0~8191
- multiRotationTypeList
  - 定義 : 目標地点でのキューブの角度Θのタイプ（意味）を表す配列
  - 種類 :
    - AbsoluteLeastAngle : 絶対角度・回転量が少ない方向
    - AbsoluteClockwise : 絶対角度・正方向(時計回り)
    - AbsoluteCounterClockwise : 絶対角度・負方向(反時計回り)
    - RelativeClockwise : 相対角度・正方向(時計回り)
    - RelativeCounterClockwise : 相対角度・負方向(反時計回り)
    - NotRotate : 回転しない
    - Original : 書き込み操作時と同じ・回転量が少ない方向
- configID
  - 定義 : 制御識別値、制御の応答を識別するための値。ここで設定した値が対応する応答にも含まれる
  - 範囲 : 0~255
- timeOut
  - 定義 : タイムアウト時間
  - 範囲 : 0~255
    -  0 のみ例外的に 10 秒になる
- targetMoveType
  - 定義 : 移動タイプ
  - 種類 :
    - RotatingMove : 回転しながら移動
    - RoundForwardMove : 回転しながら移動（後退なし）
    - RoundBeforeMove : 回転してから移動
- maxSpd
  - 定義 : モーターの最大速度指示値
  - 範囲 : 10~115
- targetSpeedType
  - 定義 : モーターの速度変化タイプ
  - 種類 :
    - UniformSpeed : 速度一定
    - Acceleration : 目標地点まで徐々に加速
    - Deceleration : 目標地点まで徐々に減速
    - VariableSpeed : 中間地点まで徐々に加速し、そこから目標地点まで減速
- multiWriteType
  - 定義 : 書き込み操作の追加設定
  - 種類 :
    - Write : 上書き
    - Add : 追加
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong

<br>

### AccelerationMove

```C#
public void AccelerationMove(
            int targetSpeed,
            int acceleration,
            ushort rotationSpeed = 0,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            byte controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong);
```

キューブの加速度指定付きモーター制御を実行します。<br>
[toio™コア キューブ 技術仕様（通信仕様）](https://toio.github.io/toio-spec/docs/ble_motor#加速度指定付きモーター制御)

- targetSpeed
  - 定義 : キューブが進行方向に対して進む速度　マイナスを付けると後退になる
  - 範囲 : 8~115
- acceleration
  - 定義 : キューブの加速度
  - 範囲 : 0~255
    -  0 の場合「キューブの並進速度」で指定した速度になる
- rotationSpeed
  - 定義 : キューブの向きの回転速度[度/秒]　マイナスを付けると負方向(反時計回り)になる
  - 範囲 : 0~65535

- accPriorityType
  - 定義 : 優先指定
  - 種類 :
    - Translation : 並進速度を優先し、回転速度を調整する
    - Rotation : 回転速度を優先し、並進速度を調整する
- controlTime
  - 定義 : 制御時間[10ms]
  - 範囲 : 0~255
    -  0 は「時間制限無し」という意味になる
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong
<br>