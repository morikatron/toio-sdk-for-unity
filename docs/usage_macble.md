# MacBLE利用方法

ここでは Mac 上の Unity Editor からリアルのキューブに接続する方法を解説します。

## 目次

- [動作確認済み環境](usage_macble.md#動作確認済み環境)
- [導入方法](usage_macble.md#導入方法)
- [使い方](usage_macble.md#使い方)

<br>

## 動作確認済み環境

以下の環境で動作確認しています。必ずしも合わせる必要はありませんが、自分の環境でうまく行かない場合は参考にしてください。

| ツール・ソフトウェア | 確認バージョン         | 推奨バージョン          |
| -------------------- | ---------------------- | ----------------------- |
| OS                   | MacOS Catalina 10.15.7 | ? |
| BluetoothR            | 4.2             | 4.2 以上             |
| Unity                | 2019.4.0f1             | 2019.3 以上             |

> BlueToothRのバージョン確認については、Appleメニューから[この Mac について] > [システムレポート] > [Bluetooth] > [LMPのバージョン]で確認できます。

<br>

## 導入方法

事前作業として、画面の左上にあるAppleメニューから「この Mac について」をクリックし、OSバージョンを確認してください。

確認したOSバージョンが <b>10.x</b> もしくは <b>11.x</b> の場合は、[ビルド済みのbundleファイルをダウンロードする方法](usage_macble.md#ビルド済みのbundleファイルをダウンロードする方法)を参考に作業を進めてください。
それら以外のOSバージョンの場合は、[自分のPCでbundleファイルをビルドする方法](usage_macble.md#自分のpcでbundleファイルをビルドする方法)を参考に作業を進めてください。

<br>

### ビルド済みのbundleファイルをダウンロードする方法

#### 1. bundleファイル のダウンロード

[toio/ble-plugin-unity/Releases](https://github.com/toio/ble-plugin-unity/releases) を開き、事前作業で確認したOSバージョンが <b>10.x</b> の場合は`bleplugin_10.bundle.zip`を、<b>11.x</b> の場合は`bleplugin_11.bundle.zip`をクリックしてダウンロードします。

> 本SDKがv1.3.0時点では、v1.0.0リリースを対象にしています。

#### 2. bundleファイル をUnityプロジェクトにドラッグ&ドロップ

Unityプロジェクトを開き、`Assets > ble-plugin-unity > Plugins` までフォルダを移動します。
ダウンロードしたzipファイルを解答し、bundleファイルをUnityウィンドウへ直接ドラッグ&ドロップします。

<div  align="center">
<img width=600 src="res/usage_macble/add_bundle_file.png"></img>
</div>

<br>

### 自分のPCでbundleファイルをビルドする方法

#### 1. Xcodeプロジェクト のダウンロード

[toio/ble-plugin-unity](https://github.com/toio/ble-plugin-unity) を開き、【Code】>【Download ZIP】の順でクリックしてリポジトリをダウンロードします。<br>

<div  align="center">
<img width=300 src="res/usage_macble/download_zip.png"></img>
</div>

#### 2. bundleファイル のビルド

ダウンロードしたzipファイルを解凍して、`bleplugin_projects/macOS/bleplugin/bleplugin.xcodeproj`にあるXcodeプロジェクトファイルを開きます。プロジェクト設定画面を開き、<b>TARGETS</b>リストにある【bleplugin】をクリック後に【Build Settings】タブをクリックし、<b>Deployment</b> 設定を開きます。
Deployment 設定内にある <b>macOS Deployment Target</b> の右列にある【macOS (バージョン値)】をクリックして、事前作業で確認したOSバージョンを選択してください。

<div  align="center">
<img width=500 src="res/usage_macble/xcode_buildtarget.png"></img>
</div>

上記作業が完了したら、Xcodeの左上にある再生ボタン(:arrow_forward:)をクリックしてビルドを開始し、ビルドが完了するまで待ちます。

#### 3. bundleファイル をUnityプロジェクトにドラッグ&ドロップ

Unityプロジェクトを開き、`Assets > ble-plugin-unity > Plugins` までフォルダを移動します。
ビルドで生成されたbundleファイルをXcodeウィンドウからUnityウィンドウへ直接ドラッグ&ドロップします。

<div  align="center">
<img width=600 src="res/usage_macble/add_bundle.png"></img>
</div>

<br>

## 使い方

[Cubeの接続設定](usage_cube.md#4-cubeの接続設定) をご参照ください。
