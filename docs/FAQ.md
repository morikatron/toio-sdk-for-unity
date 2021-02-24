# FAQ

## 目次

- [基本](FAQ.md#基本)
    - [toio SDK for Unity をダウンロードしましたがサンプルが動きません](FAQ.md#toio-SDK-for-Unity-をダウンロードしましたがサンプルが動きません)

- [シミュレータ関連](FAQ.md#シミュレータ関連)
    - [どのマットを使えば自分の開発に合うのかがよくわかりません](FAQ.md#どのマットを使えば自分の開発に合うのかがよくわかりません)
    - [Stage Prefab は必須ですか](FAQ.md#Stage-Prefab-は必須ですか)
    - [Stage Prefab を使わない場合に注意することはありますか](FAQ.md#Stage-Prefab-を使わない場合に注意することはありますか)

- [ウェブアプリ関連](FAQ.md#ウェブアプリ関連)
    - [WindowsPC でウェブアプリがうまく動きません](FAQ.md#windowspc-でウェブアプリがうまく動きません)

## 基本

### toio SDK for Unity をダウンロードしましたがサンプルが動きません

以下のようなエラーが出力されて動かない場合、UniTask がインストールされていない可能性があります。

```
Assets\toio-sdk-unity\Assets\toio-sdk\Scripts\Cube\Scanner\NearScanner.cs(54,22): error CS0246: The type or namespace name 'UniTask<>' could not be found (are you missing a using directive or an assembly reference?)
```

toio SDK for Unity を動作させるには UniTask のインストールが必須ですので、[【コチラ】](download_sdk.md#UniTask-のインストール)を参考にインストールしてください。

## シミュレータ関連

### どのマットを使えば自分の開発に合うのかがよくわかりません
> Keywords： マット Mat 仕様

各種マットの違いは見た目と座標範囲だけです。仕様は [toio™コア キューブ 技術仕様 2.1.0](https://toio.github.io/toio-spec/docs/info_position_id) と [『開発者向けマット（仮称）』](https://toio.io/blog/detail/20200423-1.html) を参考にしてください。

単にシミュレータで動かしたい場合は、どちらを使っても問題ありません。<br>
シミュレータだけで完結せず、実際にキューブを動かすアプリを開発したい場合は、実際に使うマットと同じタイプを使ったほうが良いです。

また、Mat クラスは選択したマットタイプに応じて、座標範囲、中央座標、座標変換関数などを用意していますので、[ドキュメント](usage_simulator.md#2-Mat-Prefab) を参考にしてください。

### Stage Prefab は必須ですか
> Keywords: Stage

必須ではありません。

[ドキュメント](usage_simulator.md#5-Stage-Prefab) で紹介したように、マット、カメラ、操作に必要なコンポネント等をセットにしたものです。Stage Prefab と Cube Prehab をシーンに入れれば、迅速に基本の開発環境を整えることができます。

### Stage Prefab を使わない場合に注意することはありますか
> Keywords: Stage

Stage 専用のフォーカスやターゲットポール機能は勿論使えなくなります。

キューブの操作に必要な EventSystem などを、自力で追加する必要があります。具体的には [ドキュメント](usage_simulator.md#45-Cube-の操作-CubeInteraction) を参考にしてください。

## ウェブアプリ関連

### WindowsPC でウェブアプリがうまく動きません
> Keywords： Windows Bluetooth BLE

WindowsPC では複数台接続に問題が確認されています。[【コチラ】](build_web.md#windowspc-を使った複数台接続が不安定)をご確認ください。
