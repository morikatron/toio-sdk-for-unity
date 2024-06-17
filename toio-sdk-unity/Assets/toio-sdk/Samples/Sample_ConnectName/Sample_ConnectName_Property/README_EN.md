## Sample_ConnectName_Property

This sample sets the Local Name of the cube to connect to in the Unity editor's properties in advance, and connects to it during runtime when it is scanned.

<div align="center">
<img src="../../../../../../docs_en/res/samples/connectName_prop.png">
</div>
<br>

In the Inspector's "Local Names To Connect" section, you can add the Local Name of the cube you want to connect to. (The Local Name of a simulator cube will be the name of the game object.)

### Technical Highlights

Start scanning and set the OnScan callback to receive the scan results.

```csharp
new CubeScanner(this.connectType).StartScan(OnScan).Forget();
```

During execution, the scanned peripherals list will be checked, and those with names present in localNamesToConnect and not currently connected will be selected for connection.

```csharp
async void OnScan(BLEPeripheralInterface[] peripherals) {
    // ...

    foreach (var peri in peripherals) {
        if (!this.localNamesToConnect.Contains(peri.device_name)) continue;
        if (this.connectingNames.Contains(peri.device_name)) continue;

        // Connect cube ...
    }
}
```

### Related Materials

- You can set the `ConnectType` (whether to connect to a real or simulator cube) in the Inspector. For details on `ConnectType`, please refer to [Sample_ConnectType](../../Sample_ConnectType/README_EN.md).
