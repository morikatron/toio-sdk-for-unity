# 技術ドキュメント - 機能説明 - シミュレータ

## 目次

- [1. 概説](sys_simulator.md#1-概説)
- [2. Mat Prefab](sys_simulator.md#2-mat-prefab)
  - [2.1 マットの座標単位からメートルへの変換](sys_simulator.md#21-マットの座標単位からメートルへの変換)
  - [2.2 マットタイプの切り替え](sys_simulator.md#22-マットタイプの切り替え)
  - [2.3 マット上の座標と Unity 上の座標との変換](sys_simulator.md#23-マット上の座標と-unity-上の座標との変換)
- [3. StandardID Prefab](sys_simulator.md#3-standardid-prefab)
  - [3.1. スタンダード ID タイプの切り替え](sys_simulator.md#31-スタンダード-id-タイプの切り替え)
- [4. Cube Prefab](sys_simulator.md#4-cube-prefab)
  - [4.1 定数の定義](sys_simulator.md#41-定数の定義)
  - [4.2. 状態の模擬](sys_simulator.md#42-状態の模擬)
  - [4.3. コマンドの実行](sys_simulator.md#43-コマンドの実行)
- [5. Stage Prefab](sys_simulator.md#5-stage-prefab)
  - [5.1 ターゲットポール](sys_simulator.md#51-ターゲットポール)
  - [5.2 キューブをフォーカス](sys_simulator.md#52-キューブをフォーカス)
- [6. Stage Prefab](sys_simulator.md#6-magnet-prefab)

# 1. 概説

<div align="center">
<img width=500 src="res/simulator/overview.png">
</div>

<br>

Simulator は、toio™コア キューブと通信するスマートデバイスのアプリを開発していく際、Unity Editor 上で手軽に動作チェックができるように作られたテスト用の仮想環境です。

ディレクトリ構成は以下のようになります。

```
Assets/toio-sdk/Scripts/Simulator/  +------+ 直下にスクリプトが置いてある
├── Editor/  +-----------------------------+ インスペクターをカスタマイズする Unity Editor スクリプト
├── Materials/  +--------------------------+ シミュレータのオブジェクトに使われるマテリアル・物理マテリアル
├── Models/  +-----------------------------+ シミュレータのオブジェクトに使われる3Dモデル
├── Prefabs/  +----------------------------+ 直下にプリハブが置いてある
└── AssetLoader/  +------------------------+ 各種素材ファイルとローダースクリプト
    ├── Mat/  +----------------------------+ 各種マットのテクスチャとマテリアル
    ├── Ocatave/  +------------------------+ Sound 機能に使われた音源ファイル
    └── StandardID/  +---------------------+ 各種スタンダード ID のテクスチャ
        ├── toio_collection/  +------------+ トイオ・コレクション
        └── simple_card/  +----------------+ 簡易カード
```


# 2. Mat Prefab

Mat Prefab には、スクリプト Mat.cs と素材をロードするための MatAssetLoader.cs がアタッチされています。

また、Mat.cs のインスペクターはスクリプト Editor/MatEditor.cs によってカスタマイズされています。

以下は Mat.cs の説明になります。

## 2.1. マットの座標単位からメートルへの変換

[toio™コア キューブ 技術仕様/通信仕様/各種機能/読み取りセンサー](https://toio.github.io/toio-spec/docs/ble_id)によると、トイオ・コレクション付属のプレイマットの大きさは縦横 410 単位となっています。<br>
また、マットを実際に測定したところ一辺の長さは 56cm = 0.560 m でした。

ここから、 マットの座標情報と距離(メートル)に変換するための係数 `DotPerM` を以下のように定義しています。

```csharp
public static readonly float DotPerM = 411f/0.560f; // (410+1)/0.560 dot/m
```

## 2.2. マットタイプの切り替え

インスペクターから matType を変更すると、 Mat.cs の ApplyMatType メソッドが実行され、 座標範囲の変更とマテリアルの切り替えが行われます。

実装コード

```csharp
public enum MatType
{
    toio_collection_front = 0,
    toio_collection_back = 1,
    simple_playmat = 2,
    developer = 3,
    gesundroid = 4,
    custom = 5  // 座標範囲をカスタマイズ
}

public MatType matType;

// マットのタイプ、座標範囲の変更を反映
internal void ApplyMatType()
{
    // Resize
    if (matType != MatType.custom)
    {
        var rect = GetRectForMatType(matType);
        xMin = rect.xMin; xMax = rect.xMax;
        yMin = rect.yMin; yMax = rect.yMax;
    }
    this.transform.localScale = new Vector3((xMax-xMin+1)/DotPerM, (yMax-yMin+1)/DotPerM, 1);

    // Change material
    var loader = GetComponent<MatAssetLoader>();
    if (loader)
        GetComponent<Renderer>().material = loader.GetMaterial(matType);
}
```

## 2.3. マット上の座標と Unity 上の座標との変換

Unity 上の座標/角度とマット上の座標/角度との相互変換メソッドを用意しています。

> Mat Prefab が水平に配置されている場合にのみ正しく変換可能です。

実装コード

```csharp
// Unity上の角度を本マット上の角度に変換
public int UnityDeg2MatDeg(double deg)
{
    return (int)(deg-this.transform.eulerAngles.y-90+0.49999f)%360;
}
// 本マット上の角度をUnity上の角度に変換
public float MatDeg2UnityDeg(double deg)
{
    return (int)(deg+this.transform.eulerAngles.y+90+0.49999f)%360;
}

// Unity の3D空間座標から、本マットにおけるマット座標に変換。
public Vector2Int UnityCoord2MatCoord(Vector3 unityCoord)
{
    var matPos = this.transform.position;
    var drad = - this.transform.eulerAngles.y * Mathf.Deg2Rad;
    var _cos = Mathf.Cos(drad);
    var _sin = Mathf.Sin(drad);

    // 座標系移動：本マットに一致させ
    var dx = unityCoord[0] - matPos[0];
    var dy = -unityCoord[2] + matPos[2];

    // 座標系回転：本マットに一致させ
    Vector2 coord = new Vector2(dx*_cos-dy*_sin, dx*_sin+dy*_cos);

    // マット単位に変換
    return new Vector2Int(
        (int)(coord.x*DotPerM + this.xCenter + 0.4999f),
        (int)(coord.y*DotPerM + this.yCenter + 0.4999f)
    );
}
// 本マットにおけるマット座標から、Unity の3D空間に変換。
public Vector3 MatCoord2UnityCoord(double x, double y)
{
    var matPos = this.transform.position;
    var drad = this.transform.eulerAngles.y * Mathf.Deg2Rad;
    var _cos = Mathf.Cos(drad);
    var _sin = Mathf.Sin(drad);

    // メーター単位に変換
    var dx = ((float)x - xCenter)/DotPerM;
    var dy = ((float)y - yCenter)/DotPerM;

    // 座標系回転：Unityに一致させ
    Vector2 coord = new Vector2(dx*_cos-dy*_sin, dx*_sin+dy*_cos);

    // 座標系移動：Unityに一致させ
    coord.x += matPos.x;
    coord.y += -matPos.z;

    return new Vector3(coord.x, matPos.y, -coord.y);
}
```

<br>

# 3. StandardID Prefab

StandardID Prefab には、スクリプト StandardID.cs と素材をロードするための StandardIDAssetLoader.cs がアタッチされています。

また、StandardID.cs のインスペクターはスクリプト Editor/StandardIDEditor.cs によってカスタマイズされています。

以下は StandardID.cs の説明になります。

## 3.1. スタンダード ID タイプの切り替え

スタンダード ID の種類が多いため、個々にマテリアルを用意するのは大変かつ拡張性も悪いため、下図のように画像を Sprite 形式のテクスチャに導入し、スクリプトからメッシュに変換し、オブジェクトのレンダラーに差し替えることで、切り替えを実現しています。

<div align="center">
<img src="res/simulator/standardid.png">
</div>
<br>

実装コード

```csharp
internal void ApplyStandardIDType()
{
    // Load Sprite
    var loader = GetComponent<StandardIDAssetLoader>();
    if (!loader) return;
    Sprite sprite = null;
    if (title == Title.toio_collection)
        sprite = loader.GetSprite(toioColleType);
    else if (title == Title.simple_card)
        sprite = loader.GetSprite(simpleCardType);
    GetComponent<SpriteRenderer>().sprite = sprite;

    // Create Mesh
    var mesh = SpriteToMesh(sprite);
    GetComponentInChildren<MeshFilter>().mesh = mesh;

    // Update Mesh Collider
    GetComponentInChildren<MeshCollider>().sharedMesh = null;
    GetComponentInChildren<MeshCollider>().sharedMesh = mesh;

    // Update Size
    float realWidth = 0.05f;
    if (title == Title.toio_collection)
    {
        if ((int)toioColleType > 32) realWidth = 0.03f;
        else if ((int)toioColleType < 21 || (int)toioColleType > 26) realWidth = 0.0575f;
        else    // Skunk
        {
            if (toioColleType == ToioColleType.id_skunk_blue) realWidth = 0.179f;
            else if (toioColleType == ToioColleType.id_skunk_green) realWidth = 0.162f;
            else if (toioColleType == ToioColleType.id_skunk_yellow) realWidth = 0.145f;
            else if (toioColleType == ToioColleType.id_skunk_orange) realWidth = 0.1335f;
            else if (toioColleType == ToioColleType.id_skunk_red) realWidth = 0.1285f;
            else realWidth = 0.1225f; //toioColleType = ToioColleType.id_skunk_brown
        }
    }
    else if (title == Title.simple_card)
    {
        if (simpleCardType == SimpleCardType.Full) realWidth = 0.297f;
        else realWidth = 0.04f;
    }
    var scale = RealWidthToScale(sprite, realWidth);
    this.transform.localScale = new Vector3(scale, scale, 1);

}

public static float RealWidthToScale(Sprite sprite, float realWidth)
{
    return sprite.pixelsPerUnit/(sprite.rect.width/realWidth);
}

// http://tsubakit1.hateblo.jp/entry/2018/04/18/234424
private Mesh SpriteToMesh(Sprite sprite)
{
    var mesh = new Mesh();
    mesh.SetVertices(Array.ConvertAll(sprite.vertices, c => (Vector3)c).ToList());
    mesh.SetUVs(0, sprite.uv.ToList());
    mesh.SetTriangles(Array.ConvertAll(sprite.triangles, c => (int)c), 0);

    return mesh;
}
```

<br>

# 4. Cube Prefab

Cube Prefab には３つのスクリプトが実装されています。
- `CubeSimulator.cs`：実際のキューブのシミュレーションを実装したもの
  - `CubeSimImpl.cs`：CubeSimulator のバージョン毎の実装のベースクラスとなるもの
  - `CubeSimImpl_v2_0_0.cs`：バージョン 2.0.0 を対応する実装
  - `CubeSimImpl_v2_1_0.cs`：バージョン 2.1.0 を対応する実装
  - `CubeSimImpl_v2_2_0.cs`：バージョン 2.2.0 を対応する実装
- `CubeSimulatorEditor.cs`：`CubeSimulator.cs`のインスペクターをカスタマイズしたもの
- `CubeInteraction.cs`：シミュレータ上で、Cubeオブジェクトを押したりつかんだりする操作を実装したもの
- `AudioAssetLoader.cs`：素材のローダー。

本章は `CubeSimulator` の各バージョンの実装を紹介します。

## 4.1. 定数の定義

[toio™コア キューブ 技術仕様/ハードウェア仕様/形状・サイズ](https://toio.github.io/toio-spec/docs/hardware_shape)に記載されている寸法と
[Mat.DotPerM 定数](sys_simulator.md#21-マットの座標単位からメートルへの変換) から、左右のタイヤの間隔とキューブのサイズを以下のように定義しています。

```csharp
// 左右タイヤの間隔（メートル）
public static readonly float TireWidthM = 0.0266f;
// 左右タイヤの間隔（ドット（マット座標））
public static readonly float TireWidthDot= 0.0266f * Mat.DotPerM;
// キューブのサイズ
public static readonly float WidthM= 0.0318f;
```

[toio™コア キューブ 技術仕様/通信仕様/各種機能/モーター](https://toio.github.io/toio-spec/docs/ble_motor)に記載されているモーターのスペックと、
[toio™コア キューブ 技術仕様/ハードウェア仕様/形状・サイズ](https://toio.github.io/toio-spec/docs/hardware_shape)に記載されているタイヤの直径(0.0125m)から、
マット上の速度と速度指示値の係数を以下のように定義しています。

```csharp
// 速度（ドット毎秒）と指示値の比例
// (dot/s)/u = 4.3 rpm/u * pi * 0.0125m / (60s/m) * DotPerM
public static readonly float VDotOverU =  4.3f*Mathf.PI*0.0125f/60 * Mat.DotPerM; // about 2.06
```

## 4.2. 状態の模擬

###  読み取りセンサー

実際のキューブの読み取りセンサーと同じように、 Unity の Physics.Raycast でキューブの底面から「下」へレイを飛ばし、5mm 以内の距離で当たったオブジェクトが Mat である場合、マット座標を取得します；StandardID の場合、Standard ID を取得します。

> マット座標の取得には Mat の座標変換メソッドを利用しています。

```csharp
// CubeSimImpl_v2_0_0.cs
protected virtual void SimulateIDSensor()
{
    // 読み取りセンサーを模擬
    // Simuate Position ID & Standard ID Sensor
    RaycastHit hit;
    Vector3 gposSensor = cube.transform.Find("sensor").position;
    Ray ray = new Ray(gposSensor, -cube.transform.up);
    if (Physics.Raycast(ray, out hit)) {
        if (hit.transform.gameObject.tag == "Mat" && hit.distance < 0.005f){
            var mat = hit.transform.gameObject.GetComponent<Mat>();
            var coord = mat.UnityCoord2MatCoord(cube.transform.position);
            var deg = mat.UnityDeg2MatDeg(cube.transform.eulerAngles.y);
            var coordSensor = mat.UnityCoord2MatCoord(gposSensor);
            var xSensor = coordSensor.x; var ySensor = coordSensor.y;
            _SetXYDeg(coord.x, coord.y, deg, xSensor, ySensor);
        }
        else if (hit.transform.gameObject.tag == "StandardID" && hit.distance < 0.005f)
        {
            var stdID = hit.transform.gameObject.GetComponentInParent<StandardID>();
            var deg = stdID.UnityDeg2MatDeg(cube.transform.eulerAngles.y);
            _SetSandardID(stdID.id, deg);
        }
        else _SetOffGround();
    }
    else _SetOffGround();
}
```

Position ID と角度をセットするメソッド `_SetXYDeg` は、変更がある場合にコールバック `IDCallback` を呼び出します。

```csharp
// CubeSimImpl_v2_0_0.cs
protected void _SetXYDeg(int x, int y, int deg, int xSensor, int ySensor)
{
    if (this.x != x || this.y != y || this.deg != deg || !this.onMat)
        this.IDCallback?.Invoke(x, y, deg, xSensor, ySensor);
    this.x = x; this.y = y; this.deg = deg;
    this.xSensor = xSensor; this.ySensor = ySensor;
    this.onMat = true;
    this.onStandardID = false;
}
```

Standard ID と角度をセットするメソッド `_SetStandardID` は、変更がある場合にコールバック `StandardIDCallback` を呼び出します。

```csharp
// CubeSimImpl_v2_0_0.cs
protected void _SetSandardID(uint stdID, int deg)
{
    if (this.standardID != stdID || this.deg != deg || !this.onStandardID)
        this.standardIDCallback?.Invoke(stdID, deg);
    this.standardID = stdID;
    this.deg = deg;
    this.onStandardID = true;
    this.onMat = false;
}
```

キューブが Mat や StandardID 上から離れた場合は、メソッド `_SetOffGround` がコールバック `positionIDMissedCallback` 或いは `standardIDMissedCallback` を呼び出します。

```csharp
// CubeSimImpl_v2_0_0.cs
protected void _SetOffGround()
{
    if (this.onMat)
        this.positionIDMissedCallback?.Invoke();
    if (this.onStandardID)
        this.standardIDMissedCallback?.Invoke();
    this.onMat = false;
    this.onStandardID = false;
}
```

### ボタン

ボタン状態が変更された際、コールバック `buttonCallback` を呼び出します。

```csharp
// CubeSimImpl_v2_0_0.cs
protected bool _button;
public override bool button
{
    get {return this._button;}
    internal set
    {
        if (this._button!=value){
            this.buttonCallback?.Invoke(value);
        }
        this._button = value;
        cube._SetPressed(value);
    }
}
```

また、`CubeSimulator._SetPressed` を呼び出して、Cube オブジェクトが押された表現をします。

```csharp
// CubeSimulator.cs
internal void _SetPressed(bool pressed)
{
    this.cubeModel.transform.localEulerAngles
            = pressed? new Vector3(-93,0,0) : new Vector3(-90,0,0);
}
```


### 水平検出

Cube オブジェクトの角度が閾値を超えると、`sloped` を true にします。

```csharp
// CubeSimImpl_v2_0_0.cs
protected virtual void SimulateMotionSensor()
{
    // 水平検出
    if (cube.isSimulateSloped)
    {
        cube.sloped = Vector3.Angle(Vector3.up, cube.transform.up)>45f;
    }
    ...
}
```

`sloped` が変更された時に、`InvokeMotionSensorCallback` を通じてモーションセンサーのコールバックを呼び出します。

```csharp
// CubeSimImpl_v2_0_0.cs
protected bool _sloped;
public override bool sloped
{
    get {return this._sloped;}
    internal set
    {
        if (this._sloped!=value){
            this._sloped = value;
            this.InvokeMotionSensorCallback();
        }
    }
}
```

### 衝突検出

> 衝突検出のシミュレーションは未実装です。

衝突がインスペクターで手動で発生された時に、`TriggerCollision` が呼ばれ、`InvokeMotionSensorCallback` を通じてモーションセンサーのコールバックを呼び出します。

```csharp
// CubeSimImpl_v2_0_0.cs
protected bool _collisonDetected = false;
internal override void TriggerCollision()
{
    this._collisonDetected = true;
    this.InvokeMotionSensorCallback();
}
```

### ダブルタップ

> 2.1.0 の機能です。
ダブルタップのシミュレーションは未実装です。

ダブルタップがインスペクターで手動で押された時に、`TriggerDoubleTap` が呼ばれ、`InvokeMotionSensorCallback` を通じてモーションセンサーのコールバックを呼び出します。

```csharp
// CubeSimImpl_v2_1_0.cs
protected bool _doubleTapped = false;
internal override void TriggerDoubleTap()
{
    this._doubleTapped = true;
    this.InvokeMotionSensorCallback();
}
```

### 姿勢検出

> 2.1.0 の機能です。
原理は水平検出と同じで、Cube オブジェクトの角度が対応方向に閾値を超えたら、`pose` を対応方向にします。

```csharp
// CubeSimImpl_v2_1_0.cs
protected virtual void SimulateMotionSensor()
{
    if(Vector3.Angle(Vector3.up, transform.up)<45f)
    {
        this.pose = Cube.PoseType.up;
    }
    else if(Vector3.Angle(Vector3.up, transform.up)>135f)
    {
        this.pose = Cube.PoseType.down;
    }
    else if(Vector3.Angle(Vector3.up, transform.forward)<45f)
    {
        this.pose = Cube.PoseType.forward;
    }
    else if(Vector3.Angle(Vector3.up, transform.forward)>135f)
    {
        this.pose = Cube.PoseType.backward;
    }
    else if(Vector3.Angle(Vector3.up, transform.right)<45f)
    {
        this.pose = Cube.PoseType.right;
    }
    else if(Vector3.Angle(Vector3.up, transform.right)>135f)
    {
        this.pose = Cube.PoseType.left;
    }
}
```

`pose` が変更された時に、`InvokeMotionSensorCallback` を通じてモーションセンサーのコールバックを呼び出します。

```csharp
// CubeSimImpl_v2_1_0.cs
protected Cube.PoseType _pose = Cube.PoseType.up;
public override Cube.PoseType pose {
    get{ return _pose; }
    internal set{
        if (this._pose != value){
            this._pose = value;
            this.InvokeMotionSensorCallback();
        }
    }
}
```

### シェイク検出

> 2.2.0 の機能です。
シェイク検出のシミュレーションは未実装です。

`shakeLevel` がインスペクターで手動で変更された時に、`InvokeMotionSensorCallback` を通じてモーションセンサーのコールバックを呼び出します。

```csharp
// CubeSimImpl_v2_2_0.cs
protected int _shakeLevel;
public override int shakeLevel
{
    get {return this._shakeLevel;}
    internal set
    {
        if (this._shakeLevel != value){
            this._shakeLevel = value;
            this.InvokeMotionSensorCallback();
        }
    }
}
```

### モーター速度検出

> 2.2.0 の機能です。

モーターのシミュレーションによって計算されたタイヤの速度を変換してモーター速度とします。

```csharp
// CubeSimImpl_v2_2_0.cs
protected void SimulateMotorSpeedSensor()
{
    int left = Mathf.RoundToInt(cube.speedTireL/CubeSimulator.VMeterOverU);
    int right = Mathf.RoundToInt(cube.speedTireR/CubeSimulator.VMeterOverU);
    _SetMotorSpeed(left, right);
}
```

値が変更された時に、対応コールバック `motorSpeedCallback` を呼び出します。

```csharp
// CubeSimImpl_v2_2_0.cs
protected void _SetMotorSpeed(int left, int right)
{
    left = Mathf.Abs(left);
    right = Mathf.Abs(right);
    if (motorSpeedEnabled)
        if (this.leftMotorSpeed != left || this.rightMotorSpeed != right)
            this.motorSpeedCallback?.Invoke(left, right);
    this.leftMotorSpeed = left;
    this.rightMotorSpeed = right;
}
```

### 磁石状態検出

> 2.2.0 の機能です。

CubeSimulator がシーンにある [Magnet Prefab](#6-Magnet-Prefab) を検索し、磁気センサーの位置での合成磁場ベクトルを求めます。

```csharp
internal Vector3 _GetMagneticField()
{
    if (isSimulateMagneticSensor)
    {
        var magnetObjs = GameObject.FindGameObjectsWithTag("t4u_Magnet");
        var magnets = Array.ConvertAll(magnetObjs, obj => obj.GetComponent<Magnet>());

        Vector3 magSensor = transform.Find("MagneticSensor").position;

        Vector3 h = Vector3.zero;
        foreach (var magnet in magnets)
        {
            h += magnet.SumUpH(magSensor);
        }

        this._magneticField = new Vector3(h.z, h.x, -h.y);
    }
    return this._magneticField;
}
```

磁場ベクトルの長さと方向によって、磁石状態が遷移します。

```csharp
// CubeSimImpl_v2_2_0.cs
protected virtual void SimulateMagnetState(Vector3 force)
{
    if (this.magneticMode != Cube.MagneticMode.MagnetState)
    {
        this.magnetState = Cube.MagnetState.None;
        return;
    }

    var e = force.normalized;
    var m = force.magnitude;
    const float orientThreshold = 0.95f;
    Cube.MagnetState state = this.magnetState;

    if (m > 9000 && Vector3.Dot(e, Vector3.forward) > orientThreshold)
        state = Cube.MagnetState.N_Center;
    else if (m > 9000 && Vector3.Dot(e, Vector3.back) > orientThreshold)
        state = Cube.MagnetState.S_Center;
    else if (m > 6000 && Vector3.Dot(e, new Vector3(0, -1, 1).normalized) > orientThreshold)
        state = Cube.MagnetState.N_Right;
    else if (m > 6000 && Vector3.Dot(e, new Vector3(0, 1, 1).normalized) > orientThreshold)
        state = Cube.MagnetState.N_Left;
    else if (m > 6000 && Vector3.Dot(e, new Vector3(0, 1, -1).normalized) > orientThreshold)
        state = Cube.MagnetState.S_Right;
    else if (m > 6000 && Vector3.Dot(e, new Vector3(0, -1, -1).normalized) > orientThreshold)
        state = Cube.MagnetState.S_Left;
    else if (m < 200)
        state = Cube.MagnetState.None;

    _SetMagnetState(state);
}
```

### 磁力の検出

> 2.3.0 の機能です。

磁場ベクトルをキューブ用の単位に変換します。

```csharp
// CubeSimImpl_v2_3_0.cs
protected virtual void SimulateMagneticForce(Vector3 force)
{
    if (this.magneticMode != Cube.MagneticMode.MagneticForce)
    {
        this.magneticForce = Vector3.zero;
        return;
    }

    force /= 450;
    var orient = force.normalized * 10;
    int ox = Mathf.RoundToInt(orient.x);
    int oy = Mathf.RoundToInt(orient.y);
    int oz = Mathf.RoundToInt(orient.z);
    int mag = Mathf.RoundToInt(force.magnitude);
    Vector3 f = new Vector3(ox, oy, oz);
    f.Normalize();
    f *= mag;
    _SetMagneticForce(f);
}
```

### 姿勢角検出

> 2.3.0 の機能です。

Cube Prefab の Unity 座標系でのオイラー角から、仕様書で定義された座標系のオイラー角に変換します。<br>
また、起動時に Yaw 基準値の設定と、Yaw の誤差累積も実装されています。

```csharp
// CubeSimulator.cs
private void _InitIMU()
{
    this._attitudeYawBias = transform.eulerAngles.y;
}
private void _SimulateIMU()
{
    this._attitudeYawBiasD += (UnityEngine.Random.value-0.5f) * 0.1f;
    this._attitudeYawBiasD = Mathf.Clamp(this._attitudeYawBiasD, -1, 1);
    this._attitudeYawBias += (this._attitudeYawBiasD + UnityEngine.Random.value-0.5f) * 0.01f;
}
internal Vector3 _GetIMU()
{
    var e = transform.eulerAngles;
    float roll = e.z;
    float pitch = e.x;
    float yaw = e.y - this._attitudeYawBias;

    return new Vector3(roll, pitch, yaw);
}
```

仕様書座標系のオイラー角によって、CubeUnity クラスに送信するオイラー角とクォータニオンを作成します。<br>
現時点（2023.07.20）では、リアルのコアキューブのクォータニオンがオイラーと別々の座標系のものになっていますので、シミュレーターでも同じく再現しています。（仕様書座標系に一致しているのはオイラーの方です。）

```csharp
// CubeSimImpl_v2_3_0.cs
private float attitudeInitialYaw = 0;
protected virtual void SimulateAttitudeSensor()
{
    var e = cube._GetIMU();
    int cvt(float f) { return (Mathf.RoundToInt(f) + 180) % 360 - 180; }
    var eulers = new Vector3(cvt(e.x), cvt(e.y), cvt(e.z));

    // NOTE Reproducing real BLE protocol's BUG
    var quat = Quaternion.Euler(0, 0, -e.z) * Quaternion.Euler(0, -e.y, 0) * Quaternion.Euler(e.x+180, 0, 0);
    quat = new Quaternion(Mathf.Floor(quat.x*10000)/10000f, Mathf.Floor(quat.y*10000)/10000f,
                            Mathf.Floor(quat.z*10000)/10000f, Mathf.Floor(quat.w*10000)/10000f);

    _SetAttitude(eulers, quat);
}
```

<br>

## 4.3. コマンドの実行

### 命令処理の流れ

シミュレータは以下のようなロジックで [CubeUnity](sys_cube.md#2-cube-クラスの構造) から渡された命令を処理しています。

- CubeUnity が CubeSimulator のメソッドを呼び出すと
  - 遅延 (Delay) 後に実装メソッドを呼び出すコルーチンを開始する
  - 実装メソッドでは、受け取った命令をメンバー変数の「実行中命令」に保持する
- 毎フレーム実行される FixedUpdate() の中で以下のように処理する
  - 「実行中命令」を実行する
  - 「実行中命令」が終了した場合はクリアする

> Cube Prefab の遅延 (Delay) パラメータは実環境で実測した値を設定しています。デバイス、環境等によってかわる可能性があります。

### モーター

レイキャストを利用し、タイヤが地面に当たってるかを調査します。

```csharp
// CubeSimulator.cs
internal bool offGroundL = true;
internal bool offGroundR = true;
private void SimulatePhysics_Input()
{
    // タイヤの着地状態を調査
    // Check if tires are Off Ground
    RaycastHit hit;
    var ray = new Ray(transform.position+transform.up*0.001f-transform.right*0.0133f, -transform.up); // left wheel
    if (Physics.Raycast(ray, out hit) && hit.distance < 0.002f) offGroundL = false;
    ray = new Ray(transform.position+transform.up*0.001f+transform.right*0.0133f, -transform.up); // right wheel
    if (Physics.Raycast(ray, out hit) && hit.distance < 0.002f) offGroundR = false;
}
```

現在のモーター制御命令の目標速度を Unity 座標系での速度に変換し、
強制停止・押された場合によってタイヤ速度を計算してから、着地状態によって Cube 速度を計算し、`CubeSimulator._SetSpeed` に渡します。

```csharp
// CubeSimulator.cs
private void SimulatePhysics_Output()
{
    // タイヤ速度を更新
    if (this.forceStop || this.button || !this.isConnected)   // 強制的に停止
    {
        speedTireL = 0; speedTireR = 0;
    }
    else
    {
        var dt = Time.fixedDeltaTime;
        speedTireL += (motorTargetSpdL - speedTireL) / Mathf.Max(this.motorTau, dt) * dt;
        speedTireR += (motorTargetSpdR - speedTireR) / Mathf.Max(this.motorTau, dt) * dt;
    }

    // 着地状態により、キューブの速度を取得
    // update object's speed
    // NOTES: simulation for slipping shall be implemented here
    speedL = offGroundL? 0: speedTireL;
    speedR = offGroundR? 0: speedTireR;

    // Output
    _SetSpeed(speedL, speedR);
}
```

現在速度から目標速度までの変化量によって、 Unity の Rigidbody.Addforce で力を与え、 位置と角度を Unity の物理エンジンに更新させます。

```csharp
// CubeSimulator.cs
internal void _SetSpeed(float speedL, float speedR)
{
    // 速度変化によって力を与え、位置と角度を更新
    this.rb.angularVelocity = transform.up * (float)((speedL - speedR) / TireWidthM);
    var vel = transform.forward * (speedL + speedR) / 2;
    var dv = vel - this.rb.velocity;
    this.rb.AddForce(dv, ForceMode.VelocityChange);
}
```


#### ※さらなる改善点（現在は未実装の項目）

##### 速度と位置、角度更新の方法
現在の方法では、 Rigidbody.AddForce で確実に目標速度に達するために、マットの摩擦力を 0 に設定し、
本来なら物理法則により生じる 遅れ要素 を目標速度の計算に含めています。<br>
このような物理計算を簡単化したモデルで計算を行っているので、マットを傾けた状態での動作をシミュレーションできません。
もっと正確にモデリングするなら、次のような手順が考えられます：
- モーター制御命令から変換した 目標速度 と 現在速度 の差を、 実際のキューブのファームウェアと同一の制御モジュール（例えば PID）に入力する
- PID の出力した 「電圧」 をモーターモデルに入力する
- モーターモデルの出力した 「電流」 を換算した 「力」 を物理エンジンに与える
- ホイールの Collider、 物理マテリアルなどはなるべくリアルに作成する

### 目標指定付きモーター制御

実機のファームウェアの実装が公開されていないため、シミュレータの目標指定付きモーター制御は、仕様書と実機の動きとを参考に実装されました。その中に推測で作られた部分もあり、実機と差があるかもしれないため、いくつか重要な部分を説明します。

#### 移動タイプが0（回転しながら移動）のケース

`回転しながら移動`の場合、目標がキューブの前方にあるか後方にあるかによって、前進か後退かを決めます。

```csharp
// CubeSimImpl_v2_1_0.cs
protected (float, float) TargetMove_MoveControl(float elipsed, ushort x, ushort y, byte maxSpd, Cube.TargetSpeedType targetSpeedType, float acc, Cube.TargetMoveType targetMoveType)
{
    // ...
    Vector2 targetPos = new Vector2(x, y);
    Vector2 pos = new Vector2(this.x, this.y);
    var dpos = targetPos - pos;
    var dir2tar = Vector2.SignedAngle(Vector2.right, dpos);
    var deg2tar = Deg(dir2tar - this.deg);                    // use when moving forward
    var deg2tar_back = (deg2tar+360)%360 -180;                // use when moving backward
    bool tarOnFront = Mathf.Abs(deg2tar) <= 90;
    // ...
    switch (targetMoveType)
    {
        case (Cube.TargetMoveType.RotatingMove):        // 回転しながら移動
        {
            rotate = tarOnFront? deg2tar : deg2tar_back;
            translate = tarOnFront? spd : -spd;
            break;
        }
        // ...
    }
    // ...
}
```

#### モーターの速度変化タイプで加減速があるケース

加速の場合を例として、指令の実行が始まる際に、パスの長さと最大速度によって加速度が計算されます。指令の実行中は、キューブの位置と関係なく、時間経過と加速度によって加速していきます。

```csharp
// CubeSimImpl_v2_1_0.cs
protected virtual void TargetMoveInit()
{
    // ...
    this.currMotorTargetCmd.acc = ((float)cmd.maxSpd*cmd.maxSpd-this.deadzone*this.deadzone) * CubeSimulator.VDotOverU/2/dist;
    // ...
}
```

#### ステアリング制御

進行方向と目標への角度に比例して、回転指令値`rotate`が計算されます。

しかし、直接に`rotate`と併進指令値`translate`を合わせると（つまり`rotate`は回転の角速度と比例すると）、併進指令値が大きい場合、回転不足が生じます。逆に、`rotate`と`translate`を掛け算して新しい`rotate`値にすると（つまり`rotate`は回転半径と比例すると）、併進指令値が小さい場合に回転不足が生じます。

なので、`translate`の大きさによって、上記二種類の`rotate`の加重平均を取ることで、回転不足を解消します。

```csharp
// CubeSimImpl_v2_1_0.cs
protected void ApplyMotorControl(float translate, float rotate)
{
    var miu = Mathf.Abs(translate / this.maxMotor);
    rotate *= miu * Mathf.Abs(translate/50) + (1-miu) * 1;
    var uL = translate + rotate;
    var uR = translate - rotate;
    // ...
}
```


### サウンド

Unity の AudioSource コンポーネントを利用して MIDI ノートナンバーに応じた音色を再生しています。

#### 基準となる音源の生成

各オクターブの A (A0 から A10 まで) の音色を事前に音声ファイルとして作成しています。

音声は以下の python スクリプトで、1 周期の正弦波をサンプリングした wav ファイルを生成しています。

実装コード

```python
import numpy as np
import wave
import struct

nsamples = 32  # samples in 1 period
sin_array = [int(-np.cos(2*np.pi*i/nsamples)*127) for i in range(nsamples)]

f_A0 = 440/16

duration = 0.0233   # Since Unity 2020, audio shorter than this will not be imported correctly

for i in range(11):
    f = f_A0 * 2**i
    T = 1/f

    audio_array = sin_array * np.ceil(duration/T).astype(int)
    audio = struct.pack("b" * len(audio_array), *audio_array)

    w = wave.Wave_write(str(12*i+9) + '.wav')
    p = (1, 1, nsamples*f, len(audio), 'NONE', 'not compressed')
    w.setparams(p)
    w.writeframes(audio)
    w.close()
```

<br>

この音声ファイルを [toio™コア キューブ 技術仕様/通信仕様/各種機能/サウンド](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-と-note-name) の対応表にしたがって名前を付け、「Assets/toio-sdk/Scripts/Simulator/AssetLoader/Octave」 に配置しています。


#### 再生

あらかじめ用意した A 以外の音階は、 AudioSource の Pitch パラメータを利用して 同じオクターブにある A から変換して再生しています。

```csharp
// CubeSimulator.cs
private int playingSoundId = -1;
internal void _PlaySound(int soundId, int volume){
    if (soundId >= 128) { _StopSound(); return; }
    if (soundId != playingSoundId)
    {
        playingSoundId = soundId;
        int octave = (int)(soundId/12);
        int idx = (int)(soundId%12);
        var loader = GetComponent<AudioAssetLoader>();
        if (!loader) return;
        audioSource.clip = loader.GetAudioCLip(octave);
        audioSource.pitch = (float)Math.Pow(2, ((float)idx-9)/12);
    }
    audioSource.volume = (float)volume/256 * 0.5f;
    if (!audioSource.isPlaying)
        audioSource.Play();
}
```

### ランプ

ランプに光源を配置して発光を表現すると処理が重くなるので、単にマテリアルの色を変えています。

```csharp
// CubeSimulator.cs
internal void _SetLight(int r, int g, int b){
    LED.GetComponent<Renderer>().material.color = new Color(r/255f, g/255f, b/255f);
}
```

<br>

# 5. Stage Prefab

Stage Prefab は、

- Mat Prefab
- カメラ　（物理レイキャスタを含め）
- ライト
- ターゲットポール
- キューブの脱出を防止する 「テーブル」 と ボーダー
- EventSystem

をセットにしたものです。

## 5.1. ターゲットポール

マウスの右クリックまたはドラッグすることで ターゲットポールを設置・移動することができ、
開発者はターゲットポールの位置を取得してキューブの制御に利用することが出来ます。

実装コード

```csharp
void Update(){
    // ターゲットポールを移動
    // Moving TargetPole
    if (isDragging)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && targetPole != null) {
            targetPole.position = new Vector3(hit.point.x, targetPole.position.y, hit.point.z);
        }
    }
    ...
}
```

<br>

プロパティ `tarPoleCoord` でターゲットポールのマット上の座標を取得すると、キューブを動かす時に便利に使えます。


## 5.2. キューブをフォーカス

左クリックした際、マウスカーソル位置からレイを飛ばし、レイが衝突したキューブにスポットライトの焦点を合わせて追従します。


実装コード

```csharp
void Update(){
    ...
    // Keep focusing on focusTarget
    if (focusTarget!=null){
        var tar = new Vector3(0, 0.01f, 0) + focusTarget.position;
        mainLightObj.GetComponent<Light>().transform.LookAt(tar);
        sideLightObj.GetComponent<Light>().transform.LookAt(tar);
    }
    ...
}

private void OnLeftDown()
{
    var camera = Camera.main;
    RaycastHit hit;
    Ray ray = camera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out hit)) {
        if (hit.transform.gameObject.tag == "Cube")
            SetFocus(hit.transform);
        else SetNoFocus();
    }
    else SetNoFocus();
}
```

<br>

プロパティ `focusName` でフォーカスの対象のキューブの名前を取得することが出来ます。<br>
多数のキューブを使った処理のデバッグをする際、個々のキューブの動作をチェックするのに役立ちます。

<br>

# 6. Magnet Prefab

Magnet Prefab には、スクリプト Magnet.cs がアタッチされています。

また、Magnet Prefab が内包した磁荷を表す子オブジェクト達にもスクリプト Magnet.cs がアタッチされていますが、
親オブジェクト Magnet だけのタグが `t4u_Magnet` であるため、CubeSimulator は親オブジェクトだけを一個の磁石として認識します。

Magnet.cs は自身で定義した磁場が指定位置におくベクトルを計算できます。

```csharp
public Vector3 GetSelfH(Vector3 pos)
{
    var src = transform.position;
    var dpos = pos - src;
    var r = dpos.magnitude;
    if (r > maxDistance) return Vector3.zero;
    return maxwell * 10e-8f / (4 * Mathf.PI * mu * r * r * r) * dpos;
}
```

Magnet Prefab の親オブジェクトとすべての子オブジェクトにアタッチされる Magnet.cs が定義した合成磁場を再帰的に求められます。

```csharp
public Vector3 SumUpH(Vector3 pos)
{
    if (Vector3.Distance(pos, transform.position) > maxDistance) return Vector3.zero;

    var magnets = GetComponentsInChildren<Magnet>();
    Vector3 h = Vector3.zero;
    foreach (var magnet in magnets)
    {
        h += magnet.GetSelfH(pos);
    }
    return h;
}
```
