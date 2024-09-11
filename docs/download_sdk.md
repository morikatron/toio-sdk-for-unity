# toio SDK for Unity のインストール

## Unity プロジェクト作成

Unity Hub を開き、右上にある【新規作成】をクリック。

<img width=500 src="res/download_sdk/new_project1.png">

プロジェクト作成画面が開かれたら、以下の設定にします。

- テンプレート：3D
- プロジェクト名：なんでも可(ここでは New toio Project)
- 保存先：なんでも可(ここではデスクトップ)

設定が完了したら、【作成】をクリック。

<img width=500 src="res/download_sdk/new_project2.png">

Unity が立ち上がった事を確認しましょう。

## UniTask のインストール

toio SDK for Unity では、Unity に最適化された非同期処理ライブラリ [UniTask](https://github.com/Cysharp/UniTask) を使用しています。したがって本Unity プロジェクトには UniTask (2.1.0 以降) をインストールする必要があります。<br>

UniTask のインストールは以下の手順で行います（2024年9月11日現在。Unity 2022.3.44f1 LTS）。
1. Unity の [ウィンドウ] メニューから [Package Manager] を選んでPackage Managerを開き
1. [+] アイコンから Add package from git URL... を選び
1. 以下のURLを追加する
    - https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask

以下のドキュメントには、UniTaskのインストール手順やスクリーンショットが掲載されていますので、参考にしてください。
* [UniTask の Github リポジトリの説明文](https://github.com/Cysharp/UniTask#install-via-git-url)
* Unity マニュアルの [「Git URL からのインストール」](https://docs.unity3d.com/ja/2022.3/Manual/upm-ui-giturl.html)も参考になります。

## SDK の追加

### 1. unitypackage のダウンロード
[【コチラ】](https://github.com/morikatron/toio-sdk-for-unity/releases/)の最新リリース版の【▼Assets】を開いて【toio-sdk-for-unity.unitypackage】を探し、ダウンロードしてください。

### 2. Unity のプロジェクトにドラッグ&ドロップ
ダウンロードしたフォルダを開いたら、 **【Assets】** フォルダに **【toio-sdk-for-unity.unitypackage】** をドラック&ドロップします。

<img width=500 src="res/download_sdk/import_sdk.png">

### 3.サンプルの実行、動作確認
Unity プロジェクトに読み込まれたら、`Assets/toio-sdk/Tutorials/1.Basic/0.BasicScene/`までフォルダを移動し、`0.BasicScene シーンファイル`をダブルクリックで開きます。

シーンファイルが開いたら、エディタ上部にあるプレイボタンをクリックします。

<img width=300 src="res/download_sdk/play.png">

 以下のようにサンプルが動く事（シミュレーター上）を確認してください。
 ※実物のtoio™コア キューブに接続するには[docs/README.mdの「ビルド」](README.md#-3-ビルド)を参照してください。

<img src="res/download_sdk/sample.gif">

これで toio SDK for Unity のインストールは完了です。本SDKでは様々なチュートリアルが用意されています。各種チュートリアルについては [チュートリアル(Basic)](tutorials_basic.md) を参照してください。
