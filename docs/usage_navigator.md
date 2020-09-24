# 技術ドキュメント - 使い方 - Navigatorクラス

## 目次

- [1. 概説](usage_navigator.md#1-概説)
- [2. CubeNavigator クラス API](usage_navigator.md#2-CubeNavigator-クラス-API)
  - [2.1. 列挙型](usage_navigator.md#21-列挙型)
  - [2.2. パラメーター](usage_navigator.md#22-パラメーター)
  - [2.3. プロパティ](usage_navigator.md#23-プロパティ)
  - [2.4. NaviResult 構造体](usage_navigator.md#24-NaviResult-構造体)
  - [2.5. メソッド](usage_navigator.md#25-メソッド)
- [3. HLAvoid クラス API](usage_navigator.md#3-HLAvoid-クラス-API)
  - [3.1. スキャン結果構造体 ScanResult](usage_navigator.md#31-スキャン結果構造体-ScanResult)
  - [3.2. パラメーター](usage_navigator.md#32-パラメーター)
  - [3.3. 取得できる情報](usage_navigator.md#33-取得できる情報)
  - [3.4. メソッド](usage_navigator.md#34-メソッド)
- [4. Boids クラス API](usage_navigator.md#4-Boids-クラス-API)
  - [4.1. パラメーター](usage_navigator.md#41-パラメーター)
  - [4.2. メソッド](usage_navigator.md#42-メソッド)

# 1. 概説

Navigator は、複数のロボット（キューブ）が存在する時、
お互いのロボットの動きを考慮しながら上手く移動するために作られたアルゴリズムです。

このアルゴリズムは主に「ヒューマンライク衝突回避」(HLAvoid)と「ボイド」(Boids)二つのアルゴリズムに基づいています。

- HLAvoid は自然に回避する手法
- Boids は群れとして、同調した動作をする手法

Navigator に実装されたクラスはすべて `toio.Navigation` 名前空間に属しています。

### クラスダイアグラム

<div align="center"><img width=600 src="res/navigator/arch.png"></div>

Navigator クラスが２つのアルゴリズム HLAvoid と Boids を使っています。<br>
CubeNavigator クラスが Navigator クラスを継承して、インターフェイスとして CubeHandle クラスと連携しています。

詳しくは[Navigator の機能説明ドキュメント](sys_navigator.md)を参考にしてください。

### モード

Navigator クラスには 3 つのモード（Navigator.mode）があります。

- AVOID： 衝突回避のみ
- BOIDS： ボイドのみ
- BOIDS_AVOID： ボイドと衝突回避の組み合わせ

<br>

# 2. CubeNavigator クラス API

ユーザーがナビゲーションの機能を使う際、直接に触れるのは Navigator クラスを継承した CubeNavigator クラスです。

## 2.1. 列挙型

```c#
// ナビゲーターのモード
public enum Mode : byte
{
    AVOID = 0,
    BOIDS = 1,
    BOIDS_AVOID = 2,
}

// 他者をボイドと認識するか
public enum Relation : byte
{
    NONE = 0,   // 他者をボイドとしない
    BOIDS = 1,  // 他者をボイドとする、動きを同調する
}
```

## 2.2. パラメーター

```c#
// CubeNavigator
public bool usePred = false;    // CubeHandle の予測値を使うか

// Navigator
public Mode mode = Mode.AVOID;

// Navigator.GetWaypointTo用
public double p_surrogate_target = 1;       // BOIDS_AVOID モード時、ボイドの方向への影響係数
public double p_speedratio_boidsavoid = 1;  // BOIDS_AVOID モード時、ボイドの速度係数への影響係数
public double p_speedratio_boids = 1;       // BOIDS モード時、ボイドの速度係数への影響係数
```

## 2.3. プロパティ

```c#
public Cube cube{ get; }
public CubeHandle handle{ get; }
public NaviResult result{ get; }    // 計算結果を保存

// Navigator から継承したプロパティ
public Entity entity { get; }   // 自身
public Boids boids { get; }     // ボイドアルゴ
public HLAvoid avoid { get; }   // 回避アルゴ
```

## 2.4. NaviResult 構造体

ナビゲーターの計算結果を持つ構造体です。<br>
以下のプロパティを持っています。

```c#
public Vector waypoint { get; }     // ウェイポイント
public double speedRatio { get; }   // （ボイドからの）速度係数（既定値は１）
public double speedLimit { get; }   // （回避からの）速度上限（既定値はDoubleの最大値）
public bool isCollision { get; }    // 衝突状態

public Mode mode { get; }           // ナビゲーターのモード（バックアップ情報）
public Vector avoidVector { get; }  // 回避の結果ベクトル（バックアップ情報）
public Vector boidsVector { get; }  // ボイドの結果ベクトル（バックアップ情報）
```

## 2.5. メソッド

### 壁を設定

#### AddWall

```c#
public void AddWall(Wall wall);
```

壁を追加します

- wall
  - 定義：壁

```c#
public void AddWall(List<Wall> walls);
```

複数の壁を追加します

- walls
  - 定義：壁リスト

#### RemoveWall

```c#
public void RemoveWall(Wall wall);
```

壁を削除します

- wall
  - 定義：壁

#### ClearWall

```c#
public void ClearWall();
```

壁をすべて削除します

#### AddBorder

```c#
public void AddBorder(int width=60, int x1=0, int x2=500, int y1=0, int y2=500);
```

マットにボーダー付けたい場合、東西南北に壁を一斉に作るメソッドです。<br>
CubeNavigator をインスタンス化する際、自動的に `AddBorder(70);` が呼ばれます。

- width
  - 定義：壁の厚さの半分
  - 既定値：60
- x1
  - 定義：ｙ方向の一本目の壁の中央のｘ座標
  - 既定値：0
- x2
  - 定義：ｙ方向の二本目の壁の中央のｘ座標
  - 既定値：500
- y1
  - 定義：ｘ方向の一本目の壁の中央のｙ座標
  - 既定値：0
- y2
  - 定義：ｘ方向の二本目の壁の中央のｙ座標
  - 既定値：500

既定値を例として、x 座標 -60 ~ 60, 440 ~ 560 と y 座標 -60 ~ 60, 440 ~ 560 は壁になってナビゲーターに回避されます。

### 認識できる他者を設定

#### AddOther

```c#
public void AddOther(Navigator other, Relation relation=Relation.BOIDS);
```

他ナビゲーターを認識できるようにします

- other
  - 定義：他 Navigator
- relation
  - 定義：ボイドか否か
  - 既定値：[Relation.BOIDS](usage_navigator.md#21-列挙型)

```c#
public void AddOther(List<CubeNavigator> others, Relation relation=Relation.BOIDS);
public void AddOther(List<Navigator> others, Relation relation=Relation.BOIDS);
```

複数の他ナビゲーターを認識できるようにします

- others
  - 定義：他 Navigator リスト

#### RemoveOther

```c#
public void RemoveOther(Navigator other);
```

他のナビゲーターを認識できなくします

- other
  - 定義：他 Navigator

#### ClearOther

```c#
public void ClearOther();
```

すべての認識できる対象を削除します

#### ClearGNavigators

```c#
public static void ClearGNavigators();
```

全ての CubeNavigator を持つ静的リスト`gNavigators`をクリアします

`gNavigators`は CubeNavigator が新規にインスタンス化された際、
自動的に認識できる他者を設定する為のリストです。

CubeNavigator のインスタンスが作り直された場合、
本メソッドを呼び出してクリアするか、
各 CubeNavigator インスタンスが `ClearOther`と`AddOther`で手動的に設定するか、
いずれかを行ってください。

### ボイドとする他者を設定

#### SetRelation

```c#
public void SetRelation(Navigator other, Relation relation);
```

他ナビゲーターをボイドとするかしないかを設定します

- other
  - 定義：他 Navigator
- relation
  - 定義：ボイドか否か [（Relation）](usage_navigator.md#21-列挙型)

```c#
public void SetRelation(List<CubeNavigator> others, Relation relation);
public void SetRelation(List<Navigator> others, Relation relation);
```

複数の他ナビゲーターをボイドとするかしないかを設定します

- others
  - 定義：他 Navigator リスト

### 状態の更新

```c#
public void Update();
```

計算に使うキューブの状態を更新します

ナビゲーションの計算を行うフレームで、計算の前に一回実行してください。

```c#
public void Update(bool usePred);
```

状態予測のありなしを指定して計算に使うキューブの状態を更新します

- usePred
  - 定義：状態予測のありなし

### ウェイポイントの計算

#### GetWaypointTo

```c#
public NaviResult GetWaypointTo(double x, double y);
```

目標座標に移動するウェイポイントを計算します

- x
  - 定義：目標ｘ座標
  - 範囲：任意
- y
  - 定義：目標ｙ座標
  - 範囲：任意
- 戻り値
  - 定義：ナビゲーション計算結果 [（NaviResult）](usage_navigator.md#24-NaviResult-構造体)

```c#
public NaviResult GetWaypointTo(Vector pos);
public NaviResult GetWaypointTo(Vector2 pos);
public NaviResult GetWaypointTo(Vector2Int pos);
```

目標座標に移動するウェイポイントを計算します

- pos
  - 目標座標

```c#
public NaviResult GetWaypointTo(Entity target);
public NaviResult GetWaypointTo(Navigator target);
```

目標個体に移動するウェイポイントを計算します

- target
  - 目標個体

#### GetWaypointAway

```c#
public NaviResult GetWaypointAway(double x, double y);
```

目標座標から離れるウェイポイントを計算します

- x
  - 定義：目標ｘ座標
  - 範囲：任意
- y
  - 定義：目標ｙ座標
  - 範囲：任意
- 戻り値
  - 定義：ナビゲーション計算結果 [（NaviResult）](usage_navigator.md#24-NaviResult-構造体)

```c#
public NaviResult GetWaypointAway(Vector pos);
public NaviResult GetWaypointAway(Vector2 pos);
public NaviResult GetWaypointAway(Vector2Int pos);
```

目標座標から離れるウェイポイントを計算します

- pos
  - 目標座標

```c#
public NaviResult GetWaypointAway(Entity target);
public NaviResult GetWaypointAway(Navigator target);
```

目標個体から離れるウェイポイントを計算します

- target
  - 目標個体

<br>

サンプルコード

```c#
// ウェイポイントを計算
NaviResult res = cubeNavigator.GetWaypointTo(x, y);
// 目標速度 targetSpd を spd に調整
double spd = Min(res.speedLimit, targetSpd * res.speedRatio);
// ウェイポイントに向かう Movement を計算
Movement mv = cubeNavigator.handle.Move2Target(res.waypoint, maxSpd:spd);
// Movement を実行
mv.Exec();
```

<br>

### ナビゲーターを実行

前述ウェイポイントの計算と計算結果を利用して Movement の計算との過程を統合し、使いやすいメソッドを用意しています。

計算過程をカスタマイズしない場合は直接にこちらのメソッドを使えば良いです。

#### Navi2Target

```c#
public virtual Movement Navi2Target(double x, double y, int maxSpd=70, int rotateTime=250, double tolerance=20);
```

目標座標にナビゲーションする Movement を計算します

- x
  - 定義：目標ｘ座標
  - 範囲：任意
- y
  - 定義：目標ｙ座標
  - 範囲：任意
- maxSpd
  - 定義：最大速度の指示値
  - 範囲：
    - [Version 2.0.0] 0~100
  - 既定値：70
- rotateTime
  - 定義：希望回転時間（ms）
  - 範囲：100 ~ 2550
  - 既定値：250
  - 説明：rotateTime で目標に向けるように回転指示値を出す。<br>
    小さい値を入れると早く回転し、大きな値を入れるとゆっくりと回転します。正確な回転時間ではなく、だいたいの目安です。<br>
    200 以下になると不安定になる可能性があります。
- tolerance
  - 定義：到達判定の閾値（距離）
  - 既定値：20
  - 説明：目標との距離が tolerance 以下になると、到達だと判断する。
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)

```c#
public virtual Movement Navi2Target(Vector pos, int maxSpd=70, int rotateTime=250, double tolerance=20);
public virtual Movement Navi2Target(Vector2 pos, int maxSpd=70, int rotateTime=250, double tolerance=20);
public virtual Movement Navi2Target(Vector2Int pos, int maxSpd=70, int rotateTime=250, double tolerance=20);
```

目標座標にナビゲーションする Movement を計算します

- pos
  - 定義：目標座標

#### NaviAwayTarget

```c#
public virtual Movement NaviAwayTarget(double x, double y, int maxSpd=70, int rotateTime=250);
```

目標座標から離れるようにナビゲーションする Movement を計算します

- x
  - 定義：目標ｘ座標
  - 範囲：任意
- y
  - 定義：目標ｙ座標
  - 範囲：任意
- maxSpd
  - 定義：最大速度の指示値
  - 範囲：
    - [Version 2.0.0] 0~100
  - 既定値：70
- rotateTime
  - 定義：希望回転時間（ms）
  - 範囲：100 ~ 2550
  - 既定値：250
  - 説明：rotateTime で目標に向けるように回転指示値を出す。<br>
    小さい値を入れると早く回転し、大きな値を入れるとゆっくりと回転します。正確な回転時間ではなく、だいたいの目安です。<br>
    200 以下になると不安定になる可能性があります。
- 戻り値
  - 定義：移動命令 [（Movement）](usage_cubehandle.md#22-Movement-構造体)
  - 説明：

Navi2Target が返す Movement.reached は目標への距離で判定していますが、<br>
NaviAwayTarget の場合は明確な「到達」の定義がないため、<br>ウェイポイントへ移動する Move2Target の Movement を返します。

```c#
public virtual Movement NaviAwayTarget(Vector pos, int maxSpd=70, int rotateTime=250);
public virtual Movement NaviAwayTarget(Vector2 pos, int maxSpd=70, int rotateTime=250);
public virtual Movement NaviAwayTarget(Vector2Int pos, int maxSpd=70, int rotateTime=250);
```

目標座標から離れるようにナビゲーションする Movement を計算します。

- pos
  - 定義：目標座標

<br>

簡潔化したサンプルコード

```c#
// 目標へナビゲーションする Movement を計算
Movement mv = cubeNavigator.Navi2Target(x, y);
// 実行
mv.Exec();
```

或いは

```c#
// 目標へナビゲーションする Movement を計算 ＆ 実行
Movement mv = cubeNavigator.Navi2Target(x, y).Exec();
```

<br>

# 3. HLAvoid クラス API

衝突回避アルゴリズムを実装したクラスです。

CubeNavigator クラスが HLAvoid のインスタンスを持っているので、
CubeNavigator クラスから HLAvoid のパラメーターを変更したり、情報を取得したりするには以下のようにします。

```c#
CubeNavigator navigator = ...
// 例として、パラメーター range を変更
navigator.avoid.range = 220;
```

## 3.1. スキャン結果構造体 ScanResult

```c#
public struct ScanResult
{
    public bool isCollision;    // 衝突状態
    public double[] rads;       // スキャンの方向
    public double[] dists;      // 距離
    public double[] safety;     // 安全性
    public Vector[] points;     // 方向と距離で決まるポイントの相対座標

    // 初期化済みの ScanResult を作成用
    public static ScanResult init(double[] rads, double maxRange);
    // デバッグ用
    public void print(Action<string> func);
    // rads と dists で points を計算
    public void calcPoints();
}
```

## 3.2. パラメーター

```c#
public double range = 200;  // 見える距離
public int nsample = 19;    // 周囲をスキャンする時の角度の数、奇数を勧める
public double margin = 22;  // 回避用の自分のマージン

// RunTowards 用
public bool useSafety = true;                       // ScanResult.safety を使うか
public double p_waypoint_safety_threshold = 0.15;   // ウェイポイント選択時の safety の閾値

// RunAway 用
public double p_runaway_penalty_away_k = 5;     // 目標に近づかないためのペナルティ
public double p_runaway_penalty_keeprad_k = 10; // 方向を維持するためのペナルティ
public double p_runaway_range = 250;            // スキャン結果をp_runaway_range/range倍に拡大
```

## 3.3. 取得できる情報

デバッグに使うのが便利です。

```c#
// 保存した最後の計算結果
public ScanResult scanResult;   // スキャン結果
public int waypointIndex = 0;   // 選択したウェイポイントのインデックス
```

## 3.4. メソッド

Navigator の GetWaypointTo, GetWaypointAway に呼ばれています。

直に使う必要がないです。

### RunTowards

```c#
public virtual (Vector, bool, double) RunTowards(List<Navigator> others, Entity target, List<Wall> walls);
```

目標個体へナビゲーションします。

- others
  - 定義：他 Navigator リスト
- target
  - 定義：目標個体
- walls
  - 定義：壁リスト
- 戻り値: ウェイポイント、衝突状態、速度上限

### RunAway

```c#
public virtual (Vector, bool, double) RunAway(List<Navigator> others, Entity target, List<Wall> walls);
```

目標個体から逃げるようにナビゲーションします。

- others
  - 定義：他 Navigator リスト
- target
  - 定義：目標個体
- walls
  - 定義：壁リスト
- 戻り値: ウェイポイント、衝突状態、速度上限

<br>

# 4. Boids クラス API

ボイドアルゴリズムを実装したクラスです。

CubeNavigator クラスが Boids のインスタンスを持っているので、
CubeNavigator クラスから Boids のパラメーターを変更したり、情報を取得したりするには以下のようにします。

```c#
CubeNavigator navigator = ...
// 例として、パラメーター range を変更
navigator.boids.range = 180;
```

## 4.1. パラメーター

```c#
public double fov = Deg2Rad(120);       // 視野
public double range = 150;              // 見える距離
public double margin = 25;              // ボイド用のマージン

public double p_weight_attraction = 50; // 目標に移動するベクトルの重み
public double p_weight_separation = 1;  // 離れるベクトルの重み
public double p_max_separation = 100;   // 離れるベクトルの上限
public double p_weight_cohesion = 0.3;  // 平均位置に移動するベクトルの重み
public double p_max_cohesion = 50;      // 平均位置に移動するベクトルの上限
public double p_weight_alignment = 0.3; // 平均方向に向かうベクトルの重み
public double p_max_alignment = 30;     // 平均方向に向かうベクトルの上限
public double p_max_all = 100;          // 合ベクトルの上限
```

## 4.2. メソッド

### Run

```c#
public Vector Run(List<Navigator> others, Vector tarPos);
```

目標へのベクトルを含め、合ベクトルを計算します

- others
  - 定義：他 Navigator リスト
- tarPos
  - 定義：目標座標
- 戻り値: 合ベクトル

```c#
// 合ベクトルを計算、目標なし
public Vector Run(List<Navigator> others);
```

目標へのベクトルを除き、合ベクトルを計算します

- others
  - 定義：他 Navigator リスト
- 戻り値: 合ベクトル
