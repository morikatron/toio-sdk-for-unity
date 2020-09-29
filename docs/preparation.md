# 事前準備

toio SDK for Unity を使うためや、このドキュメントのサンプルやチュートリアルを行ううえで必要となるものの準備をします。具体的には以下の３つのソフトウェアをインストールします。

- Unity（2019.4 LTS）
- Xcode（11 以降）
- CocoaPods（1.8.4 以降）

## Unity のインストール

Unity のインストールや動作環境については以下の公式のドキュメントを参考にしてください。

- [Installing Unity \- Unity マニュアル](https://docs.unity3d.com/ja/2019.4/Manual/GettingStartedInstallingUnity.html)
（リンク先ページの右矢印→をクリックすると、システム要件やインストール方法の詳細が表示されます）

Unity Hub を使うとバージョンの異なる Unity を複数インストールすることができるのでおすすめです。
Unity のバージョンは **Unity 2019.4 (LTS)** を利用してください。また、インストール時に **iOS Build Support** のモジュールを追加しておくと iOS でのビルドが可能になります（モジュールは、あとでUnity Hubを使って追加することもできます）。

## Xcode のインストール

iOS でアプリケーションを動かすために Xcode を使います。以下のリンクから Xcode の最新版を入手します。

- [‎「Xcode」を Mac App Store で](https://apps.apple.com/jp/app/xcode/id497799835)

## CocoaPods のインストール

iOS 向けのビルドを行う際、toio SDK for Unity の内部で使っている依存関係をインストールするために[Cocoapods](https://cocoapods.org/)が必要となります。

CocoaPodsが既にインストールされているかどうかは、次のコマンドで確認できます。

```sh
pod --version
```

CocoaPodsがインストールされていればそのバージョンが表示され、まだインストールされていなければ"command not found: pod"と表示されますので、下記のコマンドでインストールしてください。

```sh
sudo gem install cocoapods
```

参考: [CocoaPods Guides \- Getting Started](https://guides.cocoapods.org/using/getting-started.html)

もしCocoaPodsのインストール時にエラー（Error installing cocoapodsのような）が出る場合は、少し古いバージョンをインストールすると良いかもしれません。たとえば以下のようなコマンドでバージョン指定をしてみてください。
```sh
sudo gem install -v1.8.4 cocoapods
```
CocoaPodsのインストール可能なバージョンリストは
```sh
gem query -ra -n “^cocoapods$”
```
で表示できます。おためしを！