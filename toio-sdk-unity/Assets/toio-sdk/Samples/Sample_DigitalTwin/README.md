## Sample_DigitalTwin

このサンプルはリアルキューブの動きをシミュレーターのキューブにリアルタイムに反映する、いわゆる「ディジタルツイン」を実現したものです。

<div align="center">
<img src="../../../../../docs/res/samples/digitaltwin_demo.gif">
</div>
<br>

スクリプトが3つあります。
- `DigitalTwinBinder.cs`：リアルキューブの動きをシミュレーターに反映することを実装したスクリプト
- `DigitalTwinConnecter.cs`：キューブと接続するスクリプト
- `Sample_DigitalTwin.cs`：メインスクリプト、接続したキューブを制御するスクリプト

### サンプルの使い方

#### リアルキューブと接続し、その動きをシミュレーター上のディジタルツインに反映する場合

1. `Sample_DigitalTwin.cs` の `Connect Type` を `Real` にする
1. `Local Names To Connect` に接続したいキューブのローカルネームを追加する
1. （任意）`DigitalTwinBinder.cs` でディジタルツインとして使いたいシミュレーターキューブの指定やマッピング方法の設定をする

#### シミュレーターキューブと接続し、ディジタルツインは使用しない
1. `Sample_DigitalTwin.cs` の `Connect Type` を `Simulator` にする
1. `Local Names To Connect` に接続したいキューブのローカルネーム（オブジェクト名）を追加する

#### 例

この図では、「toio-t5E」という名前のリアルキューブと、「Cube」「Cube2」という名前のシミュレーターキューブを接続ターゲットとして追加しています。同時にシミュレーターの「Cube」と「Cube2」はリアルキューブの動きを反映するディジタルツインとして設定しています。 `Connect Type` が `Real` の場合、「toio-t5E」と接続し、「Cube」のみがディジタルツインとなりますが、 `Connect Type` が `Simulator` の場合、「Cube」と「Cube2」が接続されるようになります。

<div align="center">
<img src="../../../../../docs/res/samples/digitaltwin_prop.png">
</div>
<br>


### `DigitalTwinBinder.cs` のパラメータ

- `Simulators`：リアルキューブを再現するシミュレータキューブ
- `Mat`：シミュレータキューブが置かれるマット
- `Mapping Method`：座標と角度をマッピングする方法
  - `Direct`：リアルキューブの座標と角度をそのままシミュレータキューブに設定する
    - リアルタイム性が高い
    - 情報がノイジーでシミュレータキューブが振動しているように見える
  - `AddForce`：リアルキューブの座標と角度に向けて、シミュレータキューブに力を加える
    - 少し遅延が感じられる
    - 振動が抑えられる
    - シミュレータ上のオブジェクトと衝突した場合、より安定した動きが期待される

