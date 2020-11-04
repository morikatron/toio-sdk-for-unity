# 開発前に読む注意事項

## 目次

- [1. toio-sdkにはファイルを追加しない](development_basics.md#1-toio-sdkにはファイルを追加しない)

<br>

# 1. toio-sdkにはファイルを追加しない


<details>
<summary>詳細説明</summary>


#### アセンブリ定義について

本 SDK では Assembly Definition という機能を利用し、スクリプトを複数のアセンブリに分割しています。
この機能を利用して、分割したアセンブリ間の依存関係を設定する事で、コンパイル時間を削減する事が可能です。
Assembly Definitionの詳細については[【コチラ】](https://docs.unity3d.com/ja/2018.4/Manual/ScriptCompilationAssemblyDefinitionFiles.html)をご参照下さい。

#### 本 SDK におけるアセンブリ依存関係

本 SDK では下図のような構成でアセンブリ定義ファイルを配置しています。

```
Assets
├── ble-plugin-unity
│   └── ble-plugin-unity.asmdef
└── toio-sdk
    ├── Scripts
    │   └── toio-sdk-scripts.asmdef
    └── Tests
        ├── EditMode
        │   └── EditMode.asmdef
        └── PlayMode
            └── PlayMode.asmdef
```
アセンブリ定義ファイルにはアセンブリ間の依存関係を設定する事が可能です。
本 SDK では下図のようにアセンブリ間の依存関係を設定しています。<br>

<div align="center">
<img width=500 src="res/development/dependencies.png">
</div>

</details>
