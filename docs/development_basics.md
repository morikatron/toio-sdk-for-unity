# 開発前に読む注意事項

## 目次

- [1. スクリプトの依存関係](development_basics.md#1-スクリプトの依存関係)

<br>

# 1. スクリプトの依存関係

- 重要事項
  - toio-sdk/Scriptsにはソースコードを置かない方が良い

- Unity Assembly Definitionの説明
  - モジュールの依存関係について
- T4Uの依存関係

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

<div align="center">
<img width=500 src="res/development/dependencies.png">
</div>

