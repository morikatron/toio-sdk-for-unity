# チュートリアル(CubeNavigator)

## 目次

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

# CubeNavigator

CubeNavigator を使うことで、複数のキューブがお互いの動きを考慮しながら一斉にうまく移動することが出来ます。

CubeNavigator の詳細については[【コチラ】](usage_navigator.md)を参照してください。

## CubeManager を使って CubeNavigator を利用する

> ※ この章のサンプルファイルは、「Assets/toio-sdk/Tutorials/2.Advanced-Navigator/0.BasicScene/」 にあります。<br>
> ※ この章のウェブサンプルは[【コチラ】](https://morikatron.github.io/t4u/navi/basic/)です。

CubeNavigator は CubeManager がキューブ接続時に自動的に作成してメンバ変数のリストに入れています。

### 非同期でキューブを制御する場合

下記のサンプルコードでは、 Update の中で、CubeNavigator の制御可能状態を確認してから、制御を行っています。

```c#
public class NavigatorBasic : MonoBehaviour
{
    CubeManager cubeManager;
    async void Start()
    {
        cubeManager = new CubeManager();
        await cubeManager.MultiConnect(2);
    }

    void Update()
    {
        foreach (var navigator in cubeManager.navigators)
        {
            // Controllable になるタイミング（フレーム）はキューブそれぞれ、つまり非同期
            if (cubeManager.IsControllable(navigator))
            {
                navigator.Update(); // 非同期版の場合、必ずこのメソッドを呼んでください。
                navigator.handle.MoveRaw(-50, 50, 1000);
            }
        }
    }
}
```

制御可能状態は皆それぞれなので、「非同期」になります。

### 同期でキューブを制御する場合

以下のようにすると、すべての navigator が、50ms ごとの同じフレームで制御されます。

```c#
public class NavigatorBasic : MonoBehaviour
{
    CubeManager cubeManager;
    void Start()
    {
        cubeManager = new CubeManager();
        cubeManager.MultiConnect(2);
    }

    void Update()
    {
        // 同期
        if (cubeManager.synced)
        {
            // navigator の Update も synced の呼び出し際に内部でやってくれる。
            // 個別の navigator を操作できる。
            cubeManager.navigators[0].handle.MoveRaw(-50, 50, 1000);
        }
    }
}
```

上記は、単純に同期して各 navigator を個別に操作しています。

すべての navigator を一斉に動かす場合は、syncNavigators を使うと便利です。

```c#
public class NavigatorBasic : MonoBehaviour
{
    CubeManager cubeManager;
    async void Start()
    {
        cubeManager = new CubeManager();
        await cubeManager.MultiConnect(2);
    }

    void Update()
    {
        // すべてのキューブが controllable になったら、syncNavigators が navigators を提供
        foreach (var navigator in cubeManager.syncNavigators)
        {
            // navigator の Update も syncNavigators の呼び出し際に内部でやってくれる。
            navigator.handle.MoveRaw(-50, 50, 1000);
        }
    }
}
```

### CubeManager を使わないで CubeNavigator を利用する

CubeManager を使わない場合には、以下のように Cube クラスを使って CubeNavigator インスタンスを作成してください。

```c#
public class NavigatorBasic : MonoBehaviour
{
    float intervalTime = 0.05f;
    float elapsedTime = 0;
    List<CubeNavigator> navigators;
    bool started = false;

    async void Start()
    {
        var peripheral = await new NearScanner(2).Scan();
        var cubes = await new CubeConnecter().Connect(peripheral);

        // create navigators
        this.navigators = new List<CubeNavigator>();
        foreach (var cube in cubes)
            // create navigator and add to navigators
            this.navigators.Add(new CubeNavigator(cube));

        this.started = true;
    }

    void Update()
    {
        if (!started) return;

        elapsedTime += Time.deltaTime;

        if (intervalTime < elapsedTime)
        {
            foreach (var navigator in this.navigators)
                // update state of navigator (including internal handle)
                navigator.Update();

            foreach (var navigator in this.navigators)
                // use internal handle to rotate cube
                navigator.handle.MoveRaw(-50, 50, 1000);

            elapsedTime = 0.0f;
        }
    }
}
```

## CubeNavigator による衝突回避

### 衝突を回避しつつ目標に移動する Navi2Target 関数

> ※ この章のサンプルファイルは、「Assets/toio-sdk/Tutorials/2.Advanced-Navigator/1.Navi2TargetScene/」 にあります。<br>
> ※ この章のウェブサンプルは[【コチラ】](https://morikatron.github.io/t4u/navi/navi2target/)です。

衝突を回避しつつキューブを目標に移動させるには Navi2Target 関数を利用します。<br>
この関数は CubeHandle クラスの Move2Target 関数に相当します。

```c#
// x,ｙ 目標座標、maxSpd 最大速度、rotateTime 希望回転時間（CubeHandle使い方に参考）、tolerance 到達判定閾値（目標との距離）
public virtual Movement Navi2Target(double x, double y, int maxSpd=70, int rotateTime=250, double tolerance=20);
```

#### 例 1:　目標座標の設定で、2 つのキューブを交差する方向で往復運動させる

以下の例では、2 つのキューブに互いに回避しあって往復運動をさせています。

<div align="center"><img width=256 src="res/tutorial_navigator/avoid_basic.gif"></div>

```c#
public class NavigatorHLAvoid : MonoBehaviour
{
    CubeManager cubeManager;
    void Start()
    {
        cubeManager = new CubeManager();
        cubeManager.MultiConnect(2);
    }

    int navigator0_phase = 0; int navigator1_phase = 0;
    void Update()
    {
        if (cubeManager.synced)
        {
            // navigator 0
            {
                if (navigator0_phase == 0){
                    var mv = cubeManager.navigators[0].Navi2Target(200, 200, maxSpd:50).Exec();
                    if (mv.reached) navigator0_phase = 1;
                }
                else if (navigator0_phase == 1){
                    var mv = cubeManager.navigators[0].Navi2Target(350, 350, maxSpd:50).Exec();
                    if (mv.reached) navigator0_phase = 0;
                }
            }

            // navigator 1
            {
                if (navigator1_phase == 0){
                    var mv = cubeManager.navigators[1].Navi2Target(180, 350).Exec();
                    if (mv.reached) navigator1_phase = 1;
                }
                else if (navigator1_phase == 1){
                    var mv = cubeManager.navigators[1].Navi2Target(330, 180).Exec();
                    if (mv.reached) navigator1_phase = 0;
                }
            }
        }
    }
}
```

#### 例 2:　一つのキューブがもう一つを無視するようにする

初期化設定を変更することで、特定のキューブは衝突回避をしないようにすることも可能です。

```c#
// CubeNavigator のメソッド
// すべての認識できる対象を削除する
public void ClearOther();
```

以下の例では、例 1 の初期化設定を変更することで、一つのキューブがもう一つを無視するようにしています。

<div align="center"><img width=256 src="res/tutorial_navigator/avoid_priority.gif"></div>

```c#
async void Start()
{
    cubeManager = new CubeManager();
    await cubeManager.MultiConnect(2);

    // By default, each navigator is able to see all others
    // But you can also manually make a navigator "blind"
    cubeManager.navigators[0].ClearOther();
}
```

> この例では、ラグによって回避がちょっと不自然です。

#### 例 3:　例 2 の初期化設定を変更することで、予測を起用して、ラグの影響を軽減させる

さらに、初期化設定を変更することで、キューブの移動を予測してラグの影響を軽減させ より自然に衝突回避を行うことが出来ます。<br>

```c#
// CubeNavigator のメンバー変数
public bool usePred = false;    // CubeHandle の予測値を使うか
```

以下の例では、例 2 の初期化設定を変更することで、予測を起用して、ラグの影響を軽減しています。

<div align="center"><img width=256 src="res/tutorial_navigator/avoid_pred.gif"></div>

```c#
async void Start()
{
    cubeManager = new CubeManager();
    await cubeManager.MultiConnect(2);

    cubeManager.navigators[0].ClearOther();

    // use prediction
    cubeManager.navigators[1].usePred = true;
}
```

### 目標から離れる NaviAwayTarget 関数

> ※ この章のサンプルファイルは、「Assets/toio-sdk/Tutorials/2.Advanced-Navigator/2.NaviAwayTargetScene/」 にあります。<br>
> ※ この章のウェブサンプルは[【コチラ】](https://morikatron.github.io/t4u/navi/navi_away_target/)です。

NaviAwayTarget 関数を使うと、 Navi2Target 関数とは逆にキューブが目標から離れます。<br>
(キューブの視野内に一番離れるところに移動します。)

```c#
public virtual Movement NaviAwayTarget(double x, double y, int maxSpd=70, int rotateTime=250);
```

#### 例:　鬼ごっこ

以下の例では 2 台のキューブが 「鬼ごっこ」 をするサンプルです。<br>
キューブ 0 はキューブ 1 を追いかけ、反対にキューブ 1 はキューブ 0 から逃げるように動きます。

衝突回避をしてしまうと鬼ごっこにならない（捕まえられない）ので、この例では 互いに回避しないようにするために ClearOther 関数を呼ぶようにしています。

<div align="center"><img width=256 src="res/tutorial_navigator/avoid_oni.gif"></div>

```c#
public class NaviAwayTargetTutorial : MonoBehaviour
{
    CubeManager cubeManager;

    async void Start()
    {
        cubeManager = new CubeManager();
        await cubeManager.MultiConnect(2);
        Debug.Assert(cubeManager.navigators.Count>1, "Need at least 2 cubes.");

        // By default, each navigator is able to see all others
        // But you can also manually make a navigator "blind"
        cubeManager.navigators[0].ClearOther();
        cubeManager.navigators[1].ClearOther();
    }

    void Update()
    {
        if (cubeManager.synced)
        {
            var navi0 = cubeManager.navigators[0];
            var navi1 = cubeManager.navigators[1];

            // navigator 0
            navi0.Navi2Target(navi1.handle.pos, maxSpd:50).Exec();

            // navigator 1
            navi1.NaviAwayTarget(navi0.handle.pos, maxSpd:80).Exec();
        }
    }
}
```

### ボイドによる集団制御

> ※ この章のサンプルファイルは、「Assets/toio-sdk/Tutorials/2.Advanced-Navigator/3.BoidsScene/」 にあります。<br>
> ※ この章のウェブサンプルは[【コチラ】](https://morikatron.github.io/t4u/navi/boids/)です。

「ボイド」とは、鳥の集団行動を擬似し、キューブ達を互いに一定距離を保ちながら群れとして移動させるアルゴリズムです。

#### 例 1:　複数のキューブをボイドとして一緒に移動させる

CubeNavigator クラスはデフォルトで衝突回避のみを行う設定になっています。<br>
ボイドによる制御を行うには、まずこの設定を変更する必要があります。

CubeNavigator クラスの mode メンバ変数を規定値 (AVOID： 衝突回避のみ) から BOIDS に変更してください。

```c#
// Navigator のモード
public enum Mode : byte
{
    AVOID = 0,
    BOIDS = 1,
    BOIDS_AVOID = 2,
}
// Navigator のメンバー変数
public Mode mode = Mode.AVOID;
```

以下の例では、複数のキューブがボイドとして一緒に移動します。

<div align="center"><img width=256 src="res/tutorial_navigator/boids.gif"></div>

```c#
public class BoidsTutorial : MonoBehaviour
{
    CubeManager cubeManager;
    async void Start()
    {
        cubeManager = new CubeManager();
        await cubeManager.MultiConnect(5);

        // set to BOIDS only mode
        foreach (var navi in cubeManager.navigators)
            navi.mode = CubeNavigator.Mode.BOIDS;
    }

    void Update()
    {
        // ------ Sync ------
        foreach (var navigator in cubeManager.syncNavigators)
        {
            var mv = navigator.Navi2Target(400, 400, maxSpd:50).Exec();
        }
    }
}
```

> 散らがっていたキューブ達は群がって一緒に目標に向かう事ができましたが、うまく停止できていません。

#### 例 2:　ボイドでないキューブを追加して、より群れらしく行動させる

比較対象としてボイドでないキューブを追加することで、他の複数の個体が群がって行動する効果を表現することが出来ます。

既定設定では全てのキューブが互いに同じボイドの同士 (一つの群れ) と認識していますので、
CubeNavigator クラスの SetRelation 関数を使って特定の個体をボイドから排除する必要があります。

```c#
// CubeNavigator のメソッド
// 他者（達）への認識を設定する
public void SetRelation(List<CubeNavigator> others, Relation relation);
public void SetRelation(List<Navigator> others, Relation relation);
public void SetRelation(Navigator other, Relation relation);
```

ボイドでないキューブを追加して行動させたのが以下の例です。<br>
説明の便利さのため、ボイドでないキューブの LED を赤色に、他のキューブを緑色に点灯させています。

<div align="center"><img width=256 src="res/tutorial_navigator/boids_relation.gif"></div>

```c#
async void Start()
{
    cubeManager = new CubeManager();
    await cubeManager.MultiConnect(6);
    Debug.Assert(cubeManager.navigators.Count>1, "Need at least 2 cubes.");

    // Choose 1 cube not to be of boids
    CubeNavigator navigatorNotBoids = cubeManager.navigators[0];
    foreach (var navigator in cubeManager.navigators)
        if ((navigator.cube as CubeUnity).objName == "Cube Not Boids")
            navigatorNotBoids = navigator;

    // Use LED color to distinguish cubes
    foreach (var navigator in cubeManager.navigators)
    {
        if (navigator == navigatorNotBoids) navigator.cube.TurnLedOn(255,0,0,0); // Red
        else navigator.cube.TurnLedOn(0,255,0,0);  // Green
    }

    // set to BOIDS only mode, except navigatorNotBoids
    foreach (var navigator in cubeManager.navigators)
        if (navigator != navigatorNotBoids) navigator.mode = CubeNavigator.Mode.BOIDS;

    // By default, all navigators are in one group of boids
    // here, separate Red cube from the group
    navigatorNotBoids.SetRelation(cubeManager.navigators, CubeNavigator.Relation.NONE);
    foreach (var navigator in cubeManager.navigators)
        navigator.SetRelation(navigatorNotBoids, CubeNavigator.Relation.NONE);

    started = true;
}
```

ボイドでない赤いキューブがまっすぐに目標に前進し、隣の緑のキューブが後ろの仲間達を少し待ってから目標に向かうようになりました。<br>
より自然な群れの動きになったかと思います。

> BOIDS モードのキューブは ボイドでないキューブを回避することが出来ません。<br>
> そのため、複雑の構成で BOIDS モードを使うことはおすすめしません。 後述する BOIDS_AVOID モードの仕様を検討してください。

### ボイド + 衝突回避

> ※ この章のサンプルファイルは、「Assets/toio-sdk/Tutorials/2.Advanced-Navigator/4.BoidsAvoidScene/」 にあります。<br>
> ※ この章のウェブサンプルは[【コチラ】](https://morikatron.github.io/t4u/navi/boids_avoid/)です。

ボイドと衝突回避の組み合わせを使うことで、ボイドの群がる特性と安定的で自然な回避能力を同時に発揮します。

ボイドのサンプルと同じく、CubeNavigator クラスの mode メンバ変数を BOIDS_AVOID にすることで、 ボイド+衝突回避 のモードにすることが出来ます。

#### 例: キューブの群れが、群れではないキューブを避けて目標に向かう

以下の例では、5 個のキューブの群れが 1 個の群れではないキューブを避けて目標に向かいます。

<div align="center"><img width=256 src="res/tutorial_navigator/boids_avoid.gif"></div>

```c#
public class BoidsAvoidTutorial : MonoBehaviour
{
    CubeManager cubeManager;

    async void Start()
    {
        cubeManager = new CubeManager();
        await cubeManager.MultiConnect(6);
        Debug.Assert(cubeManager.navigators.Count>1, "Need at least 2 cubes.");

        // get Cube (5)
        CubeNavigator navigatorNotBoids = null;
        foreach (var navigator in cubeManager.navigators){
            if (navigator.cube.id == "Cube (5)")
                navigatorNotBoids = navigator;
        }
        //navigatorNotBoids.cube.TurnLedOn(255,0,0,0); // Red

        // set to BOIDS_AVOID mode, except Cube (5) (Red)
        foreach (var navigator in cubeManager.navigators){
            navigator.mode = CubeNavigator.Mode.BOIDS_AVOID;
            navigator.usePred = true;
        }

        // By default, all navigators are in one group of boids
        // here, separate Red cube from the group
        foreach (var navigator in cubeManager.navigators)
            navigator.SetRelation(navigatorNotBoids, CubeNavigator.Relation.NONE);
    }

    void Update()
    {
        // ------ Sync ------
        foreach (var navigator in cubeManager.syncNavigators)
        {
            // Cube (5) stay still
            if (navigator.cube.id != "Cube (5)")
                navigator.Navi2Target(400, 400, maxSpd:50).Exec();
        }
    }
}
```

盤面のスペースが足りなくてボイドの効果があまり見えないですが、衝突を回避しつつ きちんと停止するようになりました。
