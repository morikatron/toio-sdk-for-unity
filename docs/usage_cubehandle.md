# 目次

- [1. 概説](usage_cubehandle.md#1-概説)
- [2. CubeHandle クラス API](usage_cubehandle.md#2-CubeHandle-クラス-API)
  - [2.1. 変数](usage_cubehandle.md#21-変数)
  - [2.2. Movement 構造体](usage_cubehandle.md#22-Movement-構造体)
  - [2.3. 基本メソッド](usage_cubehandle.md#23-基本メソッド)
  - [2.4. One-Shot メソッド](usage_cubehandle.md#24-One-Shot-メソッド)
  - [2.5. Closed-Loop メソッド](usage_cubehandle.md#25-Closed-Loop-メソッド)

# 1. 概説

Cube クラスには toio™ の仕様通りに基礎機能が実装されています。<br>
それに対し、プログラムからキューブの移動操作をより簡単にできるようにしたのが CubeHandle クラスです。

CubeHandle インスタンスは Cube インスタンスと一対一対応するもので、 Cube を使ってインスタンス化されます。

```c#
Cube cube = ...
CubeHandle handle = new CubeHandle(cube);
```

CubeHandle の内部の構造と Cube とのやり取りは以下の制御ブロック図で示されています。

<div align="center">
  <img src="res/cubehandle/arch.png" title="CubeHandle 制御ブロック図">
</div>

情報の流れに沿って説明すると

1. Cube の状態（座標と角度）が Update メソッドによって CubeHandle に取り込まれると、まず予測モジュールに入り、
   現在速度やラグ後の状態などを推算します
2. 予測結果と Cube の状態と合わせて、制御メソッドに入ります
3. 一つのルートは、ユーザーが指定した目標（座標や角度など）を、
   相応の制御メソッド（例えば Move2Target）に渡し、Movement を出力し Move メソッドに渡します
4. Move メソッドは 3. の Move2Target メソッドの出力した Movement を受けるか、
   或いはもう一つのオーバーロードで、
   ユーザーの指定した前進指示値と回転指示値と継続時間とを受けて、
   デッドゾーン処理とボーダー制限をかけて、Cube.Move メソッドの入力と同じ形式の
   左右指示値と継続時間に変換して、MoveRaw メソッドに渡します
5. MoveRaw メソッドは Cube.Move メソッドと同じ入力形式を持ち、
   指示値を弄ったりもせず、ただ単に指令値を予測モジュールに保存させて、
   そして Cube.Move メソッドに渡して実行させます

<br>

# 2. CubeHandle クラス API

CubeHandle クラスを使ったサンプルなどは[チュートリアル](tutorials_cubehandle.md)で紹介しています。 そちらも参照してください。

## 2.1. 変数

### 定数

```c#
public static double TireWidthDot { get; }      // 左右車輪の間隔（マット座標）
public static double VDotOverU { get; }         // 速度と指示値の比例 (dot/sec) / cmd
public static double DotPerM { get; }           // マット単位とメートルの比例 dot / mm
public static readonly float MotorTau = 0.04f;  // モーターの一次遅れ要素 sec
public double Deadzone { get; }                 // モーター指示値のデッドゾーン（実例化の際固定される）
public int MaxSpd { get; }                      // 最大速度指示値（実例化の際固定される）
```

### パラメーター

```c#
public static double dt = 1.0 / 60 * 3;     // 制御の周期 50ms
public static double lag = 0.130;           // ラグ

public int CenterX = 250;   // マットの中央のｘ座標
public int CenterY = 250;   // マットの中央のｙ座標
public int SizeX = 410;     // マットのｘ軸方向のサイズ
public int SizeY = 410;     // マットのｙ軸方向のサイズ
public int RangeX = 370;    // マットのｘ軸方向の行動範囲（Moveのボーダー制限用）
public int RangeY = 370;    // マットのｙ軸方向の行動範囲（Moveのボーダー制限用）
```

### プロパティ

```c#
// 現在状態
public Cube cube { get; }   // キューブ
public Vector pos { get; }  // 座標
public double x { get; }    // ｘ座標
public double y { get; }    // ｙ座標
public double rad { get; }  // 弧度
public double deg { get; }  // 角度（度）
public Vector dir { get; }  // 単位方向ベクトル
public int lagMS { get; }   // ラグ（ms）
public int dtMS { get; }    // 制限の周期（ms）
public bool touch { get; }  // タッチ状態（リアルだけ有効）
public bool lost { get; }   // ロスト状態（リアルだけ有効）

// 予測結果
public double spdL, spdR;       // 現在左右タイヤの速度
public double radPred, xPred, yPred, spdPredL, spdPredR;    // ラグ後の弧度、ｘｙ座標、左右速度
public double stopRadPred, stopXPred, stopYPred;    // 停止指示を出す場合、停止した後の弧度、ｘｙ座標
public double spd { get; }      // 現在速度大きさ
public Vector v { get; }        // 現在速度ベクトル
public Vector posPred { get; }  // ラグ後の座標
public double spdPred { get; }  // ラグ後の速度大きさ
public Vector vPred { get; }    // ラグ後の速度ベクトル
public double wPred { get; }    // ラグ後の角速度
public Vector stopPosPred { get; }  // 停止指示を出す場合、停止した後の座標
```

<br>

## 2.2. Movement 構造体

Movement 構造体は使いやすくするため、制御メソッドの出力を統合した構造体です。

### 変数

```c#
public CubeHandle handle;   // 誰の指示か
public double translate;    // 前進速度指示値
public double rotate;       // 回転速度指示値
public int durationMs;      // 継続時間（ms）
public bool reached;        // 制御メソッド完了したか
public bool idle;           // この Movement が実行されるか
```

### メソッド

#### Exec

```c#
public Movement Exec();
```

メンバー変数 handle の Move を呼んで実行します

<br>

## 2.3. 基本メソッド

### Update

```c#
public void Update();
```

状態更新、移動先の予測をします

このメソッドは MoveRaw 以外のメソッドを実行するフレームで、それらを実行する前に一回実行する必要があります。

### MoveRaw

```c#
public void MoveRaw(
  double uL, double uR,
  int durationMs = 1000,
  Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak
  );
```

キューブを移動させます

- uL
  - 定義：左モーター速度の指示値
  - 範囲：
    - [Version 2.0.0] -100 ~ -10； -9 ~ 9 は 0 に等価； 10 ~ 100
    - [Version 2.1.0] -115 ~ -8； -7 ~ 7 は 0 に等価； 8 ~ 115
- uR
  - 定義：右モーター速度の指示値
  - 範囲：
    - [Version 2.0.0] -100 ~ -10； -9 ~ 9 は 0 に等価； 10 ~ 100
    - [Version 2.1.0] -115 ~ -8； -7 ~ 7 は 0 に等価； 8 ~ 115
- durationMS
  - 定義：継続時間（ms）
  - 範囲：
    - 0~5 は「時間制限なし」ではなく、10ms の停止命令を代わりに送信
    - 6~9 は「時間制限なし」ではなく、10ms に修正する
    - 10~2550 （精度は 10ms、1 位が省略される）
  - 既定値：1000
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong
  - 既定値：Weak

### Move

```c#
public Movement Move(
    double translate,           // 前進速度の指示値
    double rotate,              // 回転速度の指示値
    int durationMs = 1000,      // 継続時間（ms）
    bool border = true,         // ボーダー制限のありなし
    Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak    // 指示の優先度
    );
```

キューブを移動させます

- translate
  - 定義：前進速度の指示値
  - 範囲：
    - [Version 2.0.0] -100+Abs(rotate)/2 ~ 100-Abs(rotate)/2
    - [Version 2.1.0] -115+Abs(rotate)/2 ~ 115-Abs(rotate)/2
  - 説明：左右モーター指示値（uL, uR）との関係は、`translate = (uL + uR)/2`
- rotate
  - 定義：回転速度の指示値
  - 範囲：
    - [Version 2.0.0] -100+Abs(rotate)/2 ~ 100-Abs(rotate)/2
    - [Version 2.1.0] -115+Abs(rotate)/2 ~ 115-Abs(rotate)/2
  - 説明：左右モーター指示値（uL, uR）との関係は、`rotate = uL - uR`
- durationMS
  - 定義：継続時間（ms）
  - 範囲：
    - 0~5 は「時間制限なし」ではなく、10ms の停止命令を代わりに送信
    - 6~9 は「時間制限なし」ではなく、10ms に修正する
    - 10~2550 （精度は 10ms、1 位が省略される）
  - 既定値：1000
  - 説明：ボーダー制限によって小さくなる可能性がある
- border
  - 定義：ボーダー制限のありなし
  - 説明：ボーダーの範囲は CubeHanlde のパブリック変数で設定できます。<br>
    `public int RangeX = 370;` `public int RangeY = 370;`
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong
  - 既定値：Weak
- 戻り値
  - 定義：実際に実行された [Movement](usage_cubehandle.md#22-Movement-構造体)

#### Overloads

```c#
public Movement Move(
  Movement mv,
  bool border = true,
  Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak
  );
```

[Movement](usage_cubehandle.md#22-Movement-構造体) を実行します

- mv
  - 定義：移動命令
- border
  - 定義：ボーダー制限のありなし
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong
  - 既定値：Weak

```c#
public Movement Move(
  Movement mv,
  int durationMs,
  bool border = true,
  Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak
);
```

[Movement](usage_cubehandle.md#22-Movement-構造体) の継続時間を書き換えて実行します

- mv
  - 定義：移動命令
- durationMS
  - 定義：継続時間（ms）
- border
  - 定義：ボーダー制限のありなし
- order
  - 定義 : [命令の優先度](sys_cube.md#4-命令送信)
  - 種類 : Weak, Strong
  - 既定値：Weak

### Stop

```c#
public void Stop();
```

キューブを停止させます

moveRaw(0,0,100,Cube.ORDER_TYPE.Strong) と等価です。

<br>

## 2.4. One-Shot メソッド

CubeHandle クラスの Closed-loop メソッドは目的に到達するために何度も繰り返し実行する想定です。 処理を実行するたびにキューブと Bluetooth 通信をすることなるため、 移動しながら LED を点滅したり、音を鳴らしたりすると通信量が多くなりすぎてしまいます。

One-shot メソッドはこの問題を解決するための機能で、 目標に達するために一回だけ呼び出せば良いので移動をするのに必要な通信量を抑えることが出来ます。
（Open-Loop なので、結果の保証はありません）

### TranslateByDist

```c#
public Movement TranslateByDist(double dist, double translate);
```

指定距離を指定速度で前進・後退する Movement を計算します

- dist
  - 定義：距離
  - 範囲：-2.55 _ Abs(translate) _ `VDotOverU` ~ 2.55 _ Abs(translate) _ `VDotOverU`
- translate
  - 定義：前進速度の指示値
  - 範囲：
    - [Version 2.0.0] -100~-10； 10~100
    - [Version 2.1.0] -115~-8； 8~115
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

### RotateByRad

```c#
public Movement RotateByRad(double drad, double rotate);
```

指定角度（弧度）を指定回転速度で回転する Movement を計算します

- drad
  - 定義：角度（弧度）
  - 範囲：-2.55 _ Abs(rotate) _ `VDotOverU`/`TireWidthDot` ~ 2.55 _ Abs(rotate) _ `VDotOverU`/`TireWidthDot`
- rotate
  - 定義：回転速度の指示値
  - 範囲：
    - [Version 2.0.0] -200~-20； 20~200
    - [Version 2.1.0] -230~-16； 16~230
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

### RotateByDeg

```c#
public Movement RotateByDeg(double ddeg, double rotate)
```

指定角度（度）を指定回転速度で回転する Movement を計算します

- drad
  - 定義：角度（度）
  - 範囲：-2.55 _ Abs(rotate) _ `VDotOverU`/`TireWidthDot`_180/pi<br>
    ~ 2.55 _ Abs(rotate) * `VDotOverU`/`TireWidthDot`*180/pi
- rotate
  - 定義：回転速度の指示値
  - 範囲：
    - [Version 2.0.0] -200~-20； 20~200
    - [Version 2.1.0] -230~-16； 16~230
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

<br>

## 2.5. Closed-Loop メソッド

繰り返し実行し続けることで、マットの指定した座標、指定した方向に到達するのが Closed-Loop メソッドです。

One-Shot メソッドと異なり 目標を追い続け、結果が保証されますが、高頻度にキューブと通信をするので送信量が多くなります。

### Move2Target

```c#
public Movement Move2Target(
    double tarX,            // 目標ｘ座標
    double tarY,            // 目標ｙ座標
    double maxSpd = 50,     // 最大速度の指示値
    int rotateTime = 250,   // 希望回転時間（ms）
    double tolerance = 8    // 到達判定の閾値
    );
```

目標座標に移動する Movement を計算します

- tarX
  - 定義：目標ｘ座標
  - 範囲：任意
- tarY
  - 定義：目標ｘ座標
  - 範囲：任意
- maxSpd
  - 定義：最大速度の指示値
  - 範囲：
    - [Version 2.0.0] 0~100
    - [Version 2.1.0] 0~115
  - 既定値：50
  - 説明：目標に近づくや回転する具合によって減速する。
- rotateTime
  - 定義：希望回転時間（ms）
  - 範囲：100 ~ 2550
  - 既定値：250
  - 説明：rotateTime で目標に向けるように回転指示値を出す。<br>
    小さい値を入れると早く回転し、大きな値を入れるとゆっくりと回転します。正確な回転時間ではなく、だいたいの目安です。<br>
    200 以下になると不安定になる可能性があります。
- tolerance
  - 定義：到達判定の閾値（距離）
  - 既定値：8
  - 説明：目標との距離が tolerance 以下になると、到達だと判断する。
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

#### Overloads

```c#
public Movement Move2Target(Vector pos, double maxSpd = 50, int rotateTime = 250, double tolerance = 8);
public Movement Move2Target(Vector2 pos, double maxSpd = 50, int rotateTime = 250, double tolerance = 8);
public Movement Move2Target(Vector2Int pos, double maxSpd = 50, int rotateTime = 250, double tolerance = 8)
```

- pos
  - 定義：目標座標

### Rotate2Rad

```c#
public Movement Rotate2Rad(double tarRad, int rotateTime = 400, double tolerance = 0.1);
```

指定角度（弧度）に回転する Movement を計算します

- tarRad
  - 定義：目標角度（弧度）
  - 範囲：任意
- rotateTime
  - 定義：希望回転時間（ms）
  - 範囲：100 ~ 2550
  - 既定値：400
- tolerance
  - 定義：到達判定の閾値（弧度）
  - 既定値：0.1
  - 説明：目標角度（弧度）と差が tolerance 以下になると、到達だと判断する。
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

### Rotate2Deg

```c#
public Movement Rotate2Deg(double tarDeg, int rotateTime = 400, double tolerance = 5);
```

指定角度（度）に回転する Movement を計算します

- tarRad
  - 定義：目標角度（度）
  - 範囲：任意
- rotateTime
  - 定義：希望回転時間（ms）
  - 範囲：100 ~ 2550
  - 既定値：400
- tolerance
  - 定義：到達判定の閾値（度）
  - 既定値：5
  - 説明：目標角度（度）と差が tolerance 以下になると、到達だと判断する。
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

### Rotate2Target

```c#
// tarX, tarY 指定座標、tolerance 到達判定の閾値(弧度)、rotateTime 希望回転時間（ms）
public Movement Rotate2Target(double tarX, double tarY, int rotateTime = 400, double tolerance = 0.1);
```

指定座標の方向に回転する Movement を計算します

- tarX
  - 定義：目標ｘ座標
  - 範囲：任意
- tarY
  - 定義：目標ｙ座標
  - 範囲：任意
- rotateTime
  - 定義：希望回転時間（ms）
  - 範囲：100 ~ 2550
  - 既定値：400
- tolerance
  - 定義：到達判定の閾値（弧度）
  - 既定値：0.1
  - 説明：目標角度（弧度）と差が tolerance 以下になると、到達だと判断する。
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)
