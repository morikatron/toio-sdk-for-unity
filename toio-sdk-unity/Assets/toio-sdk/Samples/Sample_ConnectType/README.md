## Sample_ConnectType

<div  align="center">
<img src="../../../../../docs/res/cube/sample_connectType.png">
</div>

このサンプルは接続設定の違いによって、シミュレータとリアルで接続設定が変わるか確認するサンプルです。

Unity Editor のヒエラルキーで `scene` オブジェクトを選択し、インスペクター上で Connect Type を切り替えることで、キューブへの接続方法を変えることが出来ます。

### ConnectType の仕様

`ConnectType` は列挙型で、以下の３つの値を設定できます。
- `Auto`：自動（デフォルト値）。Unity Editor で再生ボタンを押して実行する場合はシミュレータ内のキューブと接続する、それ以外（デバイスにビルドしてアプリを実行する場合）はリアルキューブと接続する。
- `Simulator`：すべての場合、シミュレータ内のキューブと接続する。
- `Real`：すべての場合、実機と接続する。（Unity Editor で再生ボタンを押して実行する場合も、実機と接続する。）

開発中に頻繁に試行錯誤する場合は、ビルドしてデバイスでアプリを実行するより、Unity Editor上で確認するほうが効率いいでしょう。
そしてUnity Editorで、実行の早いシミュレータと検証結果が精確な実機環境とを `ConnectType` で上手く使い分けると、より効率的な開発ができます。

### ConnectType をコードで設定する

`CubeManager`を使う場合も、`CubeScanner`と`CubeConnecter`を使う場合も、`ConnectType`タイプの変数を渡せば設定することができます。

```c#
cm = new CubeManager(connectType);
// あるいは
scanner = new CubeScanner(connectType);
connecter = new CubeConnecter(connectType);
```

本サンプルでは、変数`connectType`をパブリック変数に定義することで、Unity Editor のインスペクタから選択できるようにしました。

```c#
public class Sample_ConnectType : MonoBehaviour
{
    public ConnectType connectType;
    // ...
}
```
