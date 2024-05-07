## Sample_ConnectName

このサンプルはスキャンしたキューブのLocal Nameを画面にリストアップし、ユーザーが選択して接続できるものです。

### 技術要点

`CubeScanner` の `StartScan` を呼び出し、非同期でタイムアウトまでスキャンし続けることができます。

```csharp
this.scanner.StartScan(OnScanUpdate, OnScanEnd, 20).Forget();  // Forget は非同期関数をawaitせずに呼び出す際に、警告を防ぐためのものです。
```

入力パラメータにある`OnScanUpdate`と`OnScanEnd`は、それぞれスキャンしたキューブの受取と、スキャン終了処理を行うコールバック関数です。

本サンプルの `OnScanUpdate` はスキャン結果を一旦変数`scannedPeripherals`に保存し、`Update`関数でGUIのリストの更新を行っています。以下が主な更新コードです。

```csharp
void Update ()
{
    // ...
    // Display scanned items
    foreach (var peripheral in this.scannedPeripherals) {
        if (peripheral == null) continue;
        var item = TryGetCubeItem(peripheral);
        item.transform.SetSiblingIndex(idx++);
        addrsToRemove.Remove(peripheral.device_address);
    }
    // ...
}
```
※ 接続したキューブはスキャンされないので、表示するには別の処理が必要なので、詳しくはサンプルコードを参照してください。

本サンプルの `OnScanEnd` は以下のように、GUIのスキャンボタンを再度押せるようにする処理を行っています。

```csharp
void OnScanEnd()
{
    this.scanButton.interactable = true;
    this.scanButton.GetComponentInChildren<Text>().text = "Scan";
}
```

### 注意事項

スキャンされたキューブが電源オフにされた場合、すぐには`OnScanUpdate`が返答したリストから消えません。
その状態で接続しようとしますと、基本的にタイムアウトになります。
しかしiOS/MacOSの場合、2回タイムアウトするとBLEデバイス自体へのアクセスができなくなる不具合が発生する可能性があります。
