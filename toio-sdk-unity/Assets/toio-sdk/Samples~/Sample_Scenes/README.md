## Sample_Scenes

このサンプルは、リアルのキューブとの接続・シミュレータ上のキューブと相関インスタンスを維持したままに、シーン遷移を扱うサンプルです。

三つのシーンで構成されています：
- `preload`：最初にロードするシーン。このシーンで全てのシーンが共有するゲームオブジェクトやインスタンスを保持できるようにします；
- `scene1`：「Home」という意味合いのなシーン。CubeManagerでキューブと接続します。
- `scene2`：普通のシーン。

実行・ビルドするには
- 「Build Settings」で、上記三つのシーンを追加し、'preload'を0番にしてください。

### 技術要点

#### ゲームオブジェクトの保持

**問題点**：リアルのキューブはシーン遷移と関わらず、存在が継続するが、シミュレータのキューブ（ゲームオブジェクト）は、シーンに依存します。結果としては、シーン遷移のせいで同じコードがリアルとシミュレータとの両方に通用できなくなります。

以下のメソッドを使えば、ゲームオブジェクトをシーンに依存しない共通のスペースに移動し、保持することが出来ます。

> [DontDestroyOnLoad(UnityEngine.Object object)](https://docs.unity3d.com/ja/current/ScriptReference/Object.DontDestroyOnLoad.html)

```c#
// Smaple_Scenes_Preload.cs
void Start(){
    // ...

    // Keep Cubes across scenes
    if (CubeScanner.actualTypeOfAuto == ConnectType.Simulator)
    {
        try
        {
            foreach (var c in GameObject.FindGameObjectsWithTag("t4u_Cube"))
                DontDestroyOnLoad(c);
        }
        catch (UnityException){}
    }

    // ...
}
```

ただし、あるシーンをロードする時に、シーンの中にあるオブジェクトは新規に作られます。
あるシーンのオブジェクトを `DontDestroyOnLoad` を使って、また同じシーンをロードすると、同じオブジェクトがまた作られてしまいます。

対策として、`Find` して削除するのもアリですが、本サンプルでは、**`Preload`シーンで Cube オブジェクトを作って保持して、他のシーンでは Cube オブジェクトを置かないようにする方法を勧めます。**

#### 接続と Cube 関連のインスタンスの保持

**問題点**：リアルキューブとの接続、Cube クラス、CubeHandle、CubeNavigator をまとめて管理するクラスは CubeManager です。もし CubeManager を保持せずシーン遷移を行ったら、元のインスタンスを呼ぶ入口が無くなってしまうので、リアルの場合、接続が切れずに再接続もできない状態に陥て、キューブが制御できない状態になります。

CubeManager のインスタンスを、あるクラスの static 変数として持たせば、使い回せます。

```c#
// Sample_Scenes_Preload.cs
public class Sample_Scenes_Preload : MonoBehaviour
{
    // ...
    public static CubeManager cm;
    // ...
}

// Sample_Scenes_scene1.cs
public class Sample_Scenes_scene1 : MonoBehaviour
{
    private CubeManager cm;
    async void Start()
    {
        cm = Sample_Scenes_Preload.cm;
    }
    // ...
}
```

※ 他の方法として、`SceneManager.sceneLoaded` のにコールバックを追加して、コールバック内に `CubeManager` のインスタンスを引き渡すのもできます。

#### スクリプトの保持

ユーザーがキューブを制御するスクリプトを作って、それを保持したい場合は、スクリプトを単独の空オブジェクトにアタッチして、そのオブジェクトを `DontDestroyOnLoad` すれば良いです。

シーン `preload` のインスペクターで、`Sample_Scenes_Preload` の変数 `keep_script` にチェック入れると、`Sample_Scenes_Preload.cs` が保持され、遷移先のシーンのスクリプトは自分を非活動化して、`Sample_Scenes_Preload.cs` が継続的に実行されます。

