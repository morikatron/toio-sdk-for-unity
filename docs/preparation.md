# 事前準備

toio SDK for Unity を使うためや、このドキュメントのサンプルやチュートリアルを行ううえで必要となるものの準備をします。具体的には以下の３つのソフトウェアが必要となります。

- Unity（2022.3.44f1 LTS）
- Xcode（13 以降）
- git

## Unity のインストール

Unity のインストールや動作環境については以下の公式のドキュメントを参考にしてください。

- [Installing Unity \- Unity マニュアル](https://docs.unity3d.com/ja/2022.3/Manual/GettingStartedInstallingUnity.html)
（リンク先ページの右矢印→をクリックすると、システム要件やインストール方法の詳細が表示されます）

Unity Hub を使うとバージョンの異なる Unity を複数インストールすることができるのでおすすめです。
Unity のバージョンは **Unity 2022.3.44f1LTS** を利用してください。（Unity 2022.3.44f1LTS にて動作確認を行っております。もっと新しい Unity を使いたい場合は、各々の責任でお試しください。Unity 2020LTSを使いたい場合はtoio SDK for Unity v1.4を利用してください。Unity 2021LTSを使いたい場合はtoio SDK for Unity v1.5を利用してください。）

また、インストール時に **iOS Build Support**、**Android Build Support** または **WebGL Build Support** のモジュールを需要に応じて追加しておくと対応プラットフォームでのビルドが可能になります（モジュールは、あとでUnity Hubを使って追加することもできます）。

> ※ プロキシを使う場合、モジュールをインストールできないことがあります。

## Xcode のインストール

iOS でアプリケーションを動かすために Xcode （macOS 専用アプリ）を使います。以下のリンクから Xcode の最新版を入手します。

- [‎「Xcode」を Mac App Store で](https://apps.apple.com/jp/app/xcode/id497799835)

## git について

この後、[toio SDK for Unity のインストール](download_sdk.md) で UniTask のインストールを行う際に、gitコマンドがインストールされている必要があります。

#### macOS

macOS には標準で git が入っているはずですが、確認する場合はターミナルアプリで以下のコマンドを入力してください。

```
$ git --version
```

git がすでに入っている場合は

```
git version 2.21.1 (Apple Git-122.3)
```

このようにバージョン番号が表示されます。もし git が入っていない場合は、コマンドライン・デベロッパ・ツールのインストールを催すダイアログが表示されると思いますので、ダイアログの「インストール」ボタンをクリックして、インストールしてください。

#### Windows

Windows の場合、コマンドプロンプトを起動して以下のコマンドを実行することで、git がインストールされているかを確認できます。

```
> git --version
```

すでに git がインストールされている場合は

```
git version 2.21.0.windows.1
```

このように表示されます。まだ git コマンドが入っていない場合は、[git for windows の公式サイト](https://gitforwindows.org/)からインストーラをダウンロードして、インストールしてください。

gitをインストールした際に、UnityがGitを認識しない場合がありますが、その場合はパソコンを再起動してください。

これで事前準備は完了です。続いて toio SDK for Unity をインストールする場合は [toio SDK for Unity のインストール](download_sdk.md) を参照してください。
